using KeyCloakSolution.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace KeyCloakSolution.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly IKeycloakTokenService keycloakTokenService;



    public UserController(IKeycloakTokenService keycloakTokenService)
    {
        this.keycloakTokenService = keycloakTokenService;
    }



    [HttpPost("token")]
    public async Task<IActionResult> AuthorizeAsync([FromBody] KeycloakUserDto keycloakUserDto)
    {
        try
        {

            var response = await keycloakTokenService
                .GetTokenResponseAsync(keycloakUserDto)
                .ConfigureAwait(false);

            return new OkObjectResult(response);
        }
        catch (Exception)
        {
            return BadRequest("An error has occured!");
        }
    }

    [Authorize]
    [HttpGet("check/authorization")]
    public IActionResult CheckKeycloakAuthorization()
    {
        return new OkObjectResult(HttpStatusCode.OK);
    }  
    
    [HttpGet("signin-oidc")]
    public IActionResult SigninOidc()
    {
        return new OkObjectResult(HttpStatusCode.OK);
    }

    [Authorize(Roles = "admin")]
    [HttpGet]
    public IActionResult AdminOnly()
    {
        return Ok("Admin access granted.");
    }
}