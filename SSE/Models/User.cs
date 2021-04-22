using System.IO;

namespace SSE.Models
{
  public class User {
    public string UserName { get; set; }
    public string Token { get; set; }
    public bool IsCreator { get; set; }
  }
}