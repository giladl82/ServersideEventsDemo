using System;
using System.Collections.Generic;

namespace SSE.Models
{
  public class TodoBase
  {
    public string Id { get; set; }
    public string Content { get; set; }
    public DateTime DueDateTime { get; set; }
  }
  public class Todo : TodoBase
  {
    public List<string> UsersDone { get; set; }
  }

  public class WorkerTodo : TodoBase
  {
    public bool IsDone { get; set; }

    public WorkerTodo (Todo todo, string userName) {
      this.Content = todo.Content;
      this.Id = todo.Id;
      this.IsDone = todo.UsersDone.Contains(userName);
      this.DueDateTime = todo.DueDateTime;
    }
  }
}