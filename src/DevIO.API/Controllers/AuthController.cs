using DevIO.API.Extensios;
using DevIO.API.ViewModels;
using DevIO.Business.Intefaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevIO.API.Controllers
{
    [Route("api")]
    public class AuthController : MainController
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppSettings _appSettings;

        public AuthController(INotificador notificador , 
                              SignInManager<IdentityUser> signInManager ,
                              UserManager<IdentityUser> userManager,
                              IOptions<AppSettings> appSettings
                              ) : base(notificador)
        {
            _appSettings = appSettings.Value;
            _signInManager = signInManager;
            _userManager = userManager;
        }

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
                return CustomResponse(GerarJwt());
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

            if(result.Succeeded) return CustomResponse(GerarJwt());

            if(result.IsLockedOut)
            {
                NotificarErro("Usuário temporariamente bloqueado por tentativas invalidas");
                return CustomResponse(login);            
            }

            NotificarErro("Usuário ou Senha incorretos");
            return CustomResponse(result);

        }

        private string GerarJwt()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);

            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _appSettings.Emissor,
                Audience = _appSettings.Validoem,
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)

            });

            var encodedToken = tokenHandler.WriteToken(token);
            return encodedToken;
        }
    }
}
