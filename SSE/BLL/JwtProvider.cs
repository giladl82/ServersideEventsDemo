using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SSE.BLL
{
  public class JwtProvider
  {
    private static readonly int _expirationInMin = 60;
    private static readonly string _secretKey = "THIS_IS_MY_SECRET_KEY";
    private static readonly string _securityAlgorithm = SecurityAlgorithms.HmacSha256Signature;

    private static SecurityKey GetSymmetricSecurityKey()
    {
      byte[] symmetricKey = Encoding.UTF8.GetBytes(_secretKey);
      return new SymmetricSecurityKey(symmetricKey);
    }

    private static TokenValidationParameters GetTokenValidationParameters()
    {
      return new TokenValidationParameters()
      {

      };
    }

    public static string Genrate(List<Claim> claims)
    {
      SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_expirationInMin)),
        SigningCredentials = new SigningCredentials(GetSymmetricSecurityKey(), _securityAlgorithm)
      };

      JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
      SecurityToken securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);
      string token = jwtSecurityTokenHandler.WriteToken(securityToken);
      return token;
    }

    public static bool IsValid(string token)
    {
      try
      {
        return GetTokenClaims(token).Count > 0;
      }
      catch (Exception)
      {
        return false;
      }
    }

    public static List<Claim> GetTokenClaims(string token)
    {
      try
      {
        if (string.IsNullOrEmpty(token))
          throw new ArgumentException("Given token is null or empty.");

        JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        try
        {
          return jwtSecurityTokenHandler.ReadJwtToken(token).Claims.ToList();
        }
        catch (Exception ex)
        {
          throw ex;
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static string GetClaim(string token, string claim)
    {
      List<Claim> claims = GetTokenClaims(token);
      var value = claims.Where(c => c.Type.Equals(claim, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.Value).FirstOrDefault();
      return value;
    }

    public static string GetClaim(List<Claim> claims, string claim)
    {
      var value = claims.Where(c => c.Type.Equals(claim, StringComparison.CurrentCultureIgnoreCase)).Select(c => c.Value).FirstOrDefault();
      return value;
    }
  }
}
