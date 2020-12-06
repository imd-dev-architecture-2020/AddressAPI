using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BasisRegisters.Vlaanderen;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AddressDispatchService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly ILogger<AddressController> _logger;
        private readonly IBasisRegisterService _basisRegisterService;
        public AddressController(ILogger<AddressController> logger, IBasisRegisterService basisRegisterService)
        {
            _logger = logger;
            // notice that we don't add "BasisRegisterService" manually, it's generated using the NSwag generators
            // https://docs.microsoft.com/en-us/aspnet/core/web-api/microsoft.dotnet-openapi?view=aspnetcore-5.0
            // see also the csproj for this.
            _basisRegisterService = basisRegisterService;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetSchoolAddresses()
        {
            try
            {
                // Needless to say, normally you'd cache the result. New addresses don't pop up every five minutes :).
                var addresses = await _basisRegisterService
                    .AddressMatchAsync("Mechelen", null, "2800", null, null, "Zandpoortvest", "60", null, null);
                addresses.Warnings.ToList().ForEach(x => _logger.LogWarning($"{x.Code} {x.Message}"));
                return Ok(addresses.AdresMatches.Select(x => new
                {
                    Address = $"{x.Straatnaam.Straatnaam} {x.Huisnummer}",
                    Municipality = x.Gemeente.Gemeentenaam
                }));
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error");
                return BadRequest();
            }
        }
    }
}