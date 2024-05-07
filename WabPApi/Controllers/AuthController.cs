using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WabPApi.Models;
using WabPApi.Services;

namespace WabPApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IUserService _userService;
        private readonly IConfiguration _configuration;

        public AuthController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        // api/auth/register
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterBindingModel model)
        {
            UserManagerResponse result = null;

            if (ModelState.IsValid)
            {
                result = await _userService.RegisterUserAsync(model);

                if (result.IsSuccess)
                    return Ok(result);
            }

            return BadRequest(result);
        }

        //api/auth/login
        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginBindingModel model)
        {
            UserManagerResponse result = null;

            if (ModelState.IsValid)
            {
                result = await _userService.LoginUserAsync(model);

                if (result.IsSuccess)
                    return Ok(result);

                return BadRequest(result);
            }

            return BadRequest(result);
        }

        //api/auth/confirmemail?userId&token
        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                return NotFound();

            var result = await _userService.ConfirmEmailAsync(userId, token);

            if (result.IsSuccess)
            {
                return Redirect($"{_configuration["AppUrl"]}/confirmemail.html");
            }

            return BadRequest(result);
        }

        //api/auth/forgotpassword?Email=username
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
                return NotFound();

            var result = await _userService.ForgotPasswordAsync(email);
            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        //api/auth/resetpassword
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.ResetPasswordAsync(model);

                if (result.IsSuccess)
                    return Ok(result);

                return BadRequest(result);
            }

            return BadRequest("Some properties are not valid");
        }

        [HttpPost("LoginFacebook")]
        public async Task<IActionResult> LoginFacebook(string token)
        {
            UserManagerResponse result = null;

            if (ModelState.IsValid)
            {
                result = await _userService.LoginWithFacebook(token);

                if (result.IsSuccess)
                    return Ok(result);

                return BadRequest(result);
            }

            return BadRequest(result);
        }

        [HttpPost("FacebookLogin")]
        public async Task<IActionResult> LoginWithFacebook(string accessToken)
        {
            UserManagerResponse result = null;

            if (ModelState.IsValid)
            {
                result = await _userService.LoginWithFacebook(accessToken);

                if (result.IsSuccess)
                    return Ok(result);

                return BadRequest(result);
            }

            return BadRequest(result);
        }

        [HttpPost("GoogleLogin")]
        public async Task<IActionResult> LoginWithGoogle(string accessToken)
        {
            UserManagerResponse result = null;

            if (ModelState.IsValid)
            {
                result = await _userService.LoginWithGoogle(accessToken);

                if (result.IsSuccess)
                    return Ok(result);

                return BadRequest(result);
            }

            return BadRequest(result);
        }        
    }
}
