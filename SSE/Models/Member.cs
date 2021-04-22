using System.IO;

namespace SSE.Models
{
  public class Member
  {
    public string UserName { get; set; }
    public bool IsCreator { get; set; }
    public StreamWriter Stream { get; set; }
  }
}
