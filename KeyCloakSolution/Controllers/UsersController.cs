using KeyCloakSolution.Domain;
using KeyCloakSolution.Service;
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
    private readonly IUserService _userService;

    public UserController(IKeycloakTokenService keycloakTokenService, IUserService userService)
    {
        this.keycloakTokenService = keycloakTokenService;
        _userService = userService;
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

    [Authorize(Roles = "user")]
    [HttpGet]
    public IActionResult AdminOnly()
    {
        return Ok("Admin access granted.");
    }
    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody]CreateUserDto userDto)
    {
        var user = new User
        {
            username = userDto.UserName,
            firstName = userDto.FirstName,
            lastName = userDto.LastName,
            email = userDto.Email,
            emailVerified = userDto.emailVerified,
            enabled = userDto.Enabled,
            credentials =
            [
                new Credential
                {
                    type = "password",
                    value = userDto.Password,
                    temporary = false
                }
            ]
        };
        await _userService.CreateUser(user);
        return Ok("User access granted.");
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateUser(string userId, [FromBody] CreateUserDto userDto)
    {
        try
        {
          await _userService.Update(userDto,userId);
        return Ok("User updated successfully.");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return BadRequest($"Error: {ex.Message}");
        }
    }

    [Authorize(Roles = "admin")]
    [HttpGet("GetUsers")]
    public async Task<IActionResult> GetUsers([FromQuery] FilterDto filterDto )
    {
        var users = await _userService.Get(filterDto);
        return Ok(users);

    }
    [Authorize(Roles = "admin")]
    [HttpGet("Count")]
    public async Task<IActionResult> UserCount()
    {
        var users = await _userService.Count();
        return Ok(users);

    }



    [Authorize(Roles = "admin")]
    [HttpGet("GetById")]
    public async Task<IActionResult> GetUserById(string userId)
    {
        var users = await _userService.GetById(userId);
        return Ok(users);

    }

    [Authorize(Roles = "admin")]
    [HttpGet("GetUserProfileById")]
    public async Task<IActionResult> GetProfileById(string userId)
    {
        var users = await _userService.GetById(userId);
        return Ok(users);
    }

    [Authorize(Roles = "admin")]
    [HttpDelete]
    public async Task DeleteUser(string userId)
    {
        try
        {
            await _userService.Delete(userId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    [Authorize(Roles = "admin")]
    [HttpPut("SendSetPasswordEmail/{userId}")]

    public async Task<IActionResult> SendPasswordResetEmail(string userId)
    {
        try
        {
            var result = await _userService.SendPasswordResetEmailAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return BadRequest($"Error: {ex.Message}");
        }
    }

    [Authorize]
    [HttpPut("{userId}/reset-password")]
    public async Task<IActionResult> SetPassword(string userId, [FromBody] string newPassword)
    {
        try
        {
            var result = await _userService.ResetPassword(newPassword,userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return BadRequest($"Error: {ex.Message}");
        }
    }
    [Authorize]
    [HttpPut("VerifyEmail/{userId}")]
    public async Task<IActionResult> VerifyEmail(string userId)
    {
        try
        {
            var result = await _userService.VerifyEmail(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return BadRequest($"Error: {ex.Message}");
        }
    }

}