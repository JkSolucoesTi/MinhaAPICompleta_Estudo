﻿using DevIO.API.Controllers;
using DevIO.API.Extensios;
using DevIO.API.ViewModels;
using DevIO.Business.Intefaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DevIO.API.v1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}")]
//    [DisableCors]
    public class AuthController : MainController
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppSettings _appSettings;
        private readonly ILogger _logger;

        public AuthController(INotificador notificador , 
                              SignInManager<IdentityUser> signInManager ,
                              UserManager<IdentityUser> userManager,
                              IOptions<AppSettings> appSettings,
                              IUser user,
                              ILogger<AuthController> logger
                              ) : base(notificador,user)
        {
            _appSettings = appSettings.Value;
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
        }

//        [EnableCors("Development")]
        [HttpPost("nova-conta")]
        public async Task<IActionResult>Registrar(RegisterUserViewModel register)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var User = new IdentityUser
            {
                UserName = register.Email,
                Email = register.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(User, register.Password);

            if(result.Succeeded)
            {
                await _signInManager.SignInAsync(User, false);
                return CustomResponse(await GerarJwt(User.Email));
            }
             foreach(var error in result.Errors)
            {
                NotificarErro(error.Description);
            }

            return CustomResponse(register);
        }

        [HttpPost("entrar")]
        public async Task<IActionResult> Login(LoginUserViewModel login)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);
            var result = await _signInManager.PasswordSignInAsync(login.Email, login.Password,false,true);

            if (result.Succeeded)
            {
                _logger.LogInformation("Usuario logado com sucesso");
                return CustomResponse(await GerarJwt(login.Email));
            }

            if(result.IsLockedOut)
            {
                NotificarErro("Usuário temporariamente bloqueado por tentativas invalidas");
                return CustomResponse(login);            
            }

            NotificarErro("Usuário ou Senha incorretos");
            return CustomResponse(result);

        }

        private async Task<LoginResponseViewModel> GerarJwt(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var claims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64));


            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim("role", userRole));
            }

            var identityClaims = new ClaimsIdentity();
            identityClaims.AddClaims(claims);


            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);

            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _appSettings.Emissor,
                Audience = _appSettings.Validoem,
                Subject = identityClaims,
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)

            });

            var encodedToken = tokenHandler.WriteToken(token);
            var response = new LoginResponseViewModel()
            {
                AccessToken = encodedToken,
                ExpiresIn = TimeSpan.FromHours(Convert.ToDouble(_appSettings.ExpiracaoHoras)).TotalSeconds,
                UserToken = new UserTokenViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Claims = claims.Select(c => new ClaimViewModel { Type = c.Type, Value = c.Value })
                }
            };

            return response;
        }

        private static long ToUnixEpochDate(DateTime date)        
            => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970,1,1,0,0,0,TimeSpan.Zero)).TotalSeconds);
        
    }
}