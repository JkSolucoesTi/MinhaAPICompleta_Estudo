using DevIO.API.Controllers;
using DevIO.Business.Intefaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.API.V2.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/teste")]
    public class TesteController : MainController
    {
        private readonly ILogger _logger;

        public TesteController(INotificador notificador, IUser appUser, ILogger<TesteController> logger) : base(notificador, appUser)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Valor()
        {
            
            _logger.LogTrace(" TRACE ");//log para desenvolvimento
            _logger.LogDebug(" DEBUG ");//loga para desenvolvimento
            _logger.LogInformation(" INFORMATION "); //Gravar x mas não importante
            _logger.LogWarning(" WARNING ");// situação de erro , exemplo 404
            _logger.LogError(" ERROR "); // situação de erro de fato
            _logger.LogCritical(" CRITICAL  "); // algo critico na aplicação , instabilidade etc
           
            return "Sou a V2";
        }
    }
}
