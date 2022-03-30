﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.Models.Users;
using api.Services;
using Microsoft.IdentityModel.Tokens;

namespace api.Models
{
    public class JwtToken : IJwtToken
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public JwtToken(IConfiguration configuration, IUserService userService)
        {
            _configuration = configuration;
            _userService = userService;

        }

        public string GenToken(AuthRequest user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("AppSettings:JwtToken").Value);
            var fullUser = _userService.GetUserByUsername(user.UserName);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("name", fullUser.Username),
                                                    new Claim("role", "User"),
                                                    new Claim("id",fullUser._id)}),
                Expires = DateTime.UtcNow.AddMinutes(20),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);

        }

        public string RenewToken(string existingToken)
        {
            var userName = TokenToUsername(existingToken);
            if (userName == null)
                return null;
            var authUser = new AuthRequest();
            authUser.UserName = userName;

            return GenToken(authUser);

        }

        public string TokenToUsername(string token)
        {
            if (token == null)
                return null;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("AppSettings:JwtToken").Value);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                return jwtToken.Claims.First(x => x.Type == "name").Value;
            }
            catch
            {
                return null;
            }

        }

        public string GenGuestToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("AppSettings:JwtToken").Value);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("name",""),
                                                    new Claim("role", "Guest"),
                                                    new Claim("id","")}),
                Expires = DateTime.UtcNow.AddMinutes(20),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);

        }



    }
}
