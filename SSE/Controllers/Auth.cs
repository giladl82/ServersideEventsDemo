using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SSE.BLL;
using SSE.Models;

namespace SSE.Controllers
{
  [Route("api/[controller]")]
  public class AuthController : Controller
  {
    [HttpGet("[action]")]
    public ActionResult<User> Login(string userName, string password)
    {
      if (!password.Equals("123456"))
      {
        return Forbid();
      }

      bool isCreator = false;

      if (userName.Equals("Creator", StringComparison.CurrentCultureIgnoreCase))
      {
        isCreator = true;
      }
      else if (!userName.ToLower().StartsWith("worker"))
      {
        return Forbid();
      }

      List<Claim> claims = new List<Claim>();
      claims.Add(new Claim("userName", userName));
      claims.Add(new Claim("isCreator", isCreator.ToString()));

      string token = JwtProvider.Genrate(claims);

      User user = new User
      {
        UserName = userName,
        Token = token,
        IsCreator = isCreator
      };

      return Ok(user);
    }
  }
}
