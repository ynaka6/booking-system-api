using app.Application.Services;
using app.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Cryptography;

namespace app.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    private IConfiguration _configuration;

    private readonly ApplicationDbContext _db;

    public AuthenticationService(IConfiguration configuration, ApplicationDbContext db)
    {
        _configuration = configuration;
        _db = db;
    }

    public string GenerateToken(IAuthenticationUser user)
    {
        var token = new JwtSecurityToken(
            _configuration["JWT:ValidIssuer"], // issuer
            _configuration["JWT:ValidAudience"], // audience
            user.Claims(),
            expires: DateTime.Now.AddSeconds(10000), // 有効期限
            signingCredentials: CreateSigningCredentials()
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // public int? ValidateJwtToken(string token)
    // {
    //     if (token == null)
    //         return null;

    //     var tokenHandler = new JwtSecurityTokenHandler();
    //     var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
    //     try
    //     {
    //         tokenHandler.ValidateToken(token, new TokenValidationParameters
    //         {
    //             ValidateIssuerSigningKey = true,
    //             IssuerSigningKey = new SymmetricSecurityKey(key),
    //             ValidateIssuer = false,
    //             ValidateAudience = false,
    //             // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
    //             ClockSkew = TimeSpan.Zero
    //         }, out SecurityToken validatedToken);

    //         var jwtToken = (JwtSecurityToken)validatedToken;
    //         var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

    //         // return user id from JWT token if validation successful
    //         return userId;
    //     }
    //     catch
    //     {
    //         // return null if validation fails
    //         return null;
    //     }
    // }

    public RefreshToken GenerateRefreshToken(string ipAddress)
    {
        var refreshToken = new RefreshToken
        {
            Token = getUniqueToken(),
            ExpiredDate = DateTime.Now.AddDays(7),
            CreatedByIp = ipAddress
        };

        return refreshToken;

        string getUniqueToken()
        {
            // token is a cryptographically strong random sequence of values
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            // TODO: need check?
            // ensure token is unique by checking against db
            // var tokenIsUnique = !_db.Users.Any(u => u.RefreshTokens.Any(t => t.Token == token));

            // if (!tokenIsUnique)
            //     return getUniqueToken();

            return token;
        }
    }

    private SigningCredentials CreateSigningCredentials()
    {
        return new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])
            ),
            SecurityAlgorithms.HmacSha256
        );
    }
}