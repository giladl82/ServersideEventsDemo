using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SSE.BLL;
using SSE.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SSE.Controllers
{
  [Route("api/[controller]")]
  public class SseController : Controller
  {
    private static readonly ConcurrentDictionary<string, List<Member>> RoomMembers = new ConcurrentDictionary<string, List<Member>>();
    private static readonly ConcurrentDictionary<string, List<Todo>> TodoListPerRoom = new ConcurrentDictionary<string, List<Todo>>();

    private bool IsAuthorized(string token, out List<Member> members, bool creatorOnly = false, string roomName = "todo")
    {
      if (!RoomMembers.TryGetValue(roomName, out members))
        return false;

      if (string.IsNullOrEmpty(token) || !JwtProvider.IsValid(token))
        return false;

      bool isCreator = bool.Parse(JwtProvider.GetClaim(token, "isCreator"));
      if (!isCreator) return false;

      return true;
    }

    private async Task Send(List<Member> members, Member currentMember, object data, string eventName)
    {
      try
      {
        await currentMember.Stream.WriteAsync("data: " + "{ \"payload\":" +
        JsonSerializer.Serialize(data, data.GetType(), new JsonSerializerOptions
        {
          PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }) + ", \"event\": \"" + eventName + "\"}\n\n");
        await currentMember.Stream.FlushAsync();
      }
      catch (ObjectDisposedException)
      {
        lock (members)
          members.Remove(currentMember);
      }
    }

    private async Task SendToAll(List<Member> members, Todo data, string eventName)
    {
      await Task.WhenAll(members.Select(member =>
             {
               if (member.IsCreator)
               {
                 return Send(members, member, data, eventName);
               }
               else
               {
                 return Send(members, member, new WorkerTodo(data, member.UserName), eventName);
               }
             }));
    }

    [HttpGet("{roomName}/{userName}")]
    public async Task Listen(string roomName, string userName)
    {
      Response.Headers.Add("Cache-Control", "no-cache");
      Response.Headers.Add("X-Accel-Buffering", "no");
      Response.Headers.Add("Content-Type", "text/event-stream");

      using (var stream = new StreamWriter(Response.Body))
      {
        bool isCreator = userName.Equals("creator", StringComparison.CurrentCultureIgnoreCase);
        var member = new Member
        {
          Stream = stream,
          UserName = userName,
          IsCreator = isCreator
        };

        var members = RoomMembers.GetOrAdd(roomName, _ => new List<Member>());
        var todoList = TodoListPerRoom.GetOrAdd(roomName, _ => new List<Todo>());

        lock (members)
          members.Add(member);

        if (member.IsCreator)
        {
          await Send(members, member, todoList, "init");
        }
        else
        {
          await Send(members, member, todoList.Select(todo => new WorkerTodo(todo, userName)).ToList(), "init");
        }

        try
        {
          await Task.Delay(Timeout.Infinite, HttpContext.RequestAborted);
        }
        catch (TaskCanceledException)
        {
        }

        lock (members)
          members.Remove(member);
      }
    }

    [HttpPut("{roomName}")]
    public async Task Create(string roomName, [FromBody] Todo newTodo)
    {
      string token = Request.Cookies["auth-token"];

      IsAuthorized(token, out var members, true, roomName);

      newTodo.Id = Guid.NewGuid().ToString();

      members = SafeCopy<Member>(members);

      TodoListPerRoom.TryGetValue(roomName, out var todoList);
      lock (todoList)
        todoList.Add(newTodo);

      await SendToAll(members, newTodo, "create");
    }

    [HttpPost("{roomName}")]
    public async Task Update(string roomName, [FromBody] Todo updatedTodo)
    {
      string token = Request.Cookies["auth-token"];

      IsAuthorized(token, out var members, true, roomName);

      members = SafeCopy<Member>(members);

      Todo todo;
      TodoListPerRoom.TryGetValue(roomName, out var todoList);
      todoList = SafeCopy<Todo>(todoList);
      todo = todoList.Find(t => t.Id.Equals(updatedTodo.Id));

      if (todo == null) return;

      lock (todo)
      {
        todo.Content = updatedTodo.Content;
        todo.DueDateTime = updatedTodo.DueDateTime;
      }

      await SendToAll(members, todo, "update");
    }

    [HttpDelete("{roomName}")]
    public async Task Delete(string roomName, string id)
    {
      string token = Request.Cookies["auth-token"];

      if (string.IsNullOrEmpty(token) || !JwtProvider.IsValid(token))
        return;

      bool isCreator = bool.Parse(JwtProvider.GetClaim(token, "isCreator"));

      if (!isCreator) return;

      if (!RoomMembers.TryGetValue(roomName, out var members))
        return;

      members = SafeCopy<Member>(members);

      TodoListPerRoom.TryGetValue(roomName, out var todoList);
      lock (todoList)
      {
        int index = todoList.FindIndex(t => !t.Id.Equals(id, StringComparison.CurrentCultureIgnoreCase));
        todoList.RemoveAt(index);
      }

      await Task.WhenAll(members.Select(member => Send(members, member, "{\"success\": true, \"id\": \"" + id + "\"}", "delete")));
    }

    [HttpPost("{roomName}/[action]")]
    public async Task Toggle(string roomName, string id)
    {
      string token = Request.Cookies["auth-token"];

      IsAuthorized(token, out var members, false, roomName);

      string userName = JwtProvider.GetClaim(token, "UserName");

      members = SafeCopy<Member>(members);

      TodoListPerRoom.TryGetValue(roomName, out var todoList);
      Todo todo = todoList.Find(t => t.Id.Equals(id));

      lock (todo)
      {
        if (todo.UsersDone.Find(name => name.Equals(userName)) != null)
        {
          todo.UsersDone.Remove(userName);
        }
        else
        {
          todo.UsersDone.Add(userName);
        }
      }

      await SendToAll(members, todo, "toggle");
    }

    [HttpPost("[action]/{fromRoom}/{toRoom}")]
    public async Task ChangeRoom(string fromRoom, string toRoom)
    {
      string token = Request.Cookies["auth-token"];

      IsAuthorized(token, out var fromMembers, false, fromRoom);
      string userName = JwtProvider.GetClaim(token, "UserName");

      fromMembers = SafeCopy<Member>(fromMembers);
      var member = fromMembers.Where(u => u.UserName.Equals(userName)).FirstOrDefault();

      fromMembers.Remove(member);

      var toMembers = RoomMembers.GetOrAdd(toRoom, _ => new List<Member>());
      toMembers.Add(member);

      var todoList = TodoListPerRoom.GetOrAdd(toRoom, _ => new List<Todo>());

      await Send(toMembers, member, todoList, "update-room");
    }

    private List<T> SafeCopy<T>(List<T> list)
    {
      // Members of the same room share the same instance of `members` variable.
      // Hence, the variable reads and writes by multiple threads.
      // However, `List<T>` is not thread-safe, see https://stackoverflow.com/a/25419149/1400547.
      // The list of members must be copied so we can enumerate without getting this error https://stackoverflow.com/a/604843/1400547
      lock (list)
        return list.ToList();
    }
  }
}
