using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SSE.Controllers
{
  [Route("api/[controller]")]
  public class RoomsController : Controller
  {
    [HttpGet]
    public List<string> Get()
    {
      return new List<string>() { "Room1", "Room2", "Room3" };
    }
  }
}
