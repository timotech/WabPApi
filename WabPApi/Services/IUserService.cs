using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WabPApi.Data;
using WabPApi.Models;

namespace WabPApi.Services
{
    public interface IUserService
    {
        Task<UserManagerResponse> RegisterUserAsync(RegisterBindingModel model);
        Task<UserManagerResponse> LoginUserAsync(LoginBindingModel model);
        Task<UserManagerResponse> ConfirmEmailAsync(string userId, string token);
        Task<UserManagerResponse> ForgotPasswordAsync(string userId);
        Task<UserManagerResponse> ResetPasswordAsync(ResetPasswordViewModel model);
        Task<FacebookTokenValidationResult> ValidateAccessTokenAsync(string accessToken);
        Task<FacebookUserInfoResult> GetUserInfoAsync(string accessToken);
        Task<UserManagerResponse> LoginWithFacebook(string accessToken);
        Task<UserManagerResponse> LoginWithGoogle(string accessToken);
        Task<GoogleUserRequest> GetGoogleValidationAccessToken(string accessToken);
    }

    public class UserService : IUserService
    {
        private UserManager<ApplicationUser> _userManager;
        private IConfiguration _configuration;
        private IMailService _mailService;
        private const string TokenValidationUrl = "https://graph.facebook.com/debug_token?input_token={0}&access_token={1}|{2}";
        private const string UserInfoUrl = "https://graph.facebook.com/me?fields=first_name,last_name,picture,email&access_token={0}";
        private const string GoogleTokenValidationUrl = "https://www.googleapis.com/userinfo/v2/me";
        private readonly FacebookAuthSettings _facebookAuthSettings;
        private readonly IHttpClientFactory _httpClientFactory;

        public UserService(UserManager<ApplicationUser> userManager, IConfiguration configuration, IMailService mailService, FacebookAuthSettings facebookAuthSettings, IHttpClientFactory httpClientFactory)
        {
            _userManager = userManager;
            _configuration = configuration;
            _mailService = mailService;
            _facebookAuthSettings = facebookAuthSettings;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<UserManagerResponse> RegisterUserAsync(RegisterBindingModel model)
        {
            if (model == null)
                //throw new NullReferenceException("Invalid or no values supplied!");
                return new UserManagerResponse
                {
                    Message = "Invalid or no values supplied!",
                    IsSuccess = false,
                    Errors = new string[] { "Invalid or no values supplied!" }
                };

            if (model.Password != model.ConfirmPassword)
                return new UserManagerResponse
                {
                    Message = "Confirm password doesn't match the password",
                    IsSuccess = false,
                    Errors = new string[] { "Confirm password doesn't match the password" }
                };

            var identityUser = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.Phone
            };

            var result = await _userManager.CreateAsync(identityUser, model.Password);

            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);

                //var encodedEmailToken = Encoding.UTF8.GetBytes(token);
                //var validEmailToken = WebEncoders.Base64UrlEncode(encodedEmailToken);

                var mailData = new MailData
                {
                    EmailBody = "Thank you for patronising WABP, You have successfully registered to be one of our numerous book lovers. If you have any questions please send a mail to info@wabp.com.ng for your enquiries. Regards West African Book Publishers Ltd",
                    EmailSubject = "West African Book Publishers Registration",
                    EmailToId = identityUser.Email,
                    EmailToName = identityUser.Email
                };

                var status1 = await _mailService.SendMailAsync(mailData);

                //var status = SendMail(identityUser.Email, "West African Book Publishers Registration", "Thank you for patronising WABP, You have successfully registered to be one of our numerous book lovers. If you have any questions please send a mail to info@wabp.com.ng for your enquiries. Regards West African Book Publishers Ltd");

                return new UserManagerResponse
                {
                    Message = "User created successfully!",
                    IsSuccess = true,
                    Email = model.Email,
                    Firstname = model.FirstName,
                    Lastname = model.LastName
                };

                //var confirmationLink = $"{_configuration["AppUrl"]}/api/auth/confirmemail?userId={identityUser.Id}&token={validEmailToken}";

                ////Send Mail Here
                //var status = await _mailService.SendEmailAsync(identityUser.Email, "West African Book Publishers Registration", "Thank you for patronising WABP, Please click or paste the link below in a browser to activate your account. " + confirmationLink + " . If you have any questions please send a mail to info@wabp.com.ng for your enquiries. Regards West African Book Publishers Ltd");

                //if (status == "Success")
                //{
                //    return new UserManagerResponse
                //    {
                //        Message = "User created successfully! Before you can Login, Please confirm your email by clicking on the confirmation link we have emailed you. Don't forget to check your spam mail, incase you did not see the mail.",
                //        IsSuccess = true,
                //        Email = model.Email
                //    };
                //}
                //else
                //{
                //    return new UserManagerResponse
                //    {
                //        Message = "User created successfully! But your email could not be reached, you need to confirm your email before you can login. It will be advisable to use an existing email!!!",
                //        IsSuccess = false,
                //        Email = model.Email
                //    };
                //}

            }

            return new UserManagerResponse
            {
                Message = "User not created",
                IsSuccess = false,
                Errors = result.Errors.Select(e => e.Description)
            };
        }

        private string SendMail(string ToEmail, string subject, string message)
        {
            try
            {
                MailMessage mm = new MailMessage("info@wabp.com.ng", ToEmail);
                mm.Subject = subject;
                mm.Body = message;

                mm.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "209.205.208.10";
                //smtp.EnableSsl = true;
                NetworkCredential NetworkCred = new NetworkCredential();
                NetworkCred.UserName = "info@wabp.com.ng";
                NetworkCred.Password = "@wabp2018";
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = NetworkCred;
                //smtp.Port = 8889;
                smtp.Send(mm);
                return "Success";
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        public async Task<UserManagerResponse> LoginUserAsync(LoginBindingModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return new UserManagerResponse
                {
                    Message = "User does not exist",
                    IsSuccess = false,
                    Errors = new string[] { "User does not exist" }
                };
            }

            var result = await _userManager.CheckPasswordAsync(user, model.Password);

            if (!result)
            {
                return new UserManagerResponse
                {
                    Message = "Invalid Password",
                    IsSuccess = false,
                    Errors = new string[] { "Invalid Password" }
                };
            }

            var claims = new[]
            {
                new Claim("Email", model.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AuthSettings:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["AuthSettings:Issuer"],
                audience: _configuration["AuthSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            string tokenAsString = new JwtSecurityTokenHandler().WriteToken(token);

            return new UserManagerResponse
            {
                Message = tokenAsString,
                IsSuccess = true,
                ExpireDate = token.ValidTo,
                Email = user.Email,
                Firstname = user.FirstName,
                Lastname = user.LastName
            };
        }

        public async Task<UserManagerResponse> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "User not found",
                    Errors = new string[] { "User not found" }
                };

            var decodedToken = WebEncoders.Base64UrlDecode(token);
            string normalToken = Encoding.UTF8.GetString(decodedToken);

            var result = await _userManager.ConfirmEmailAsync(user, normalToken);

            if (result.Succeeded)
                return new UserManagerResponse
                {
                    IsSuccess = true,
                    Message = "Email confirmed successfully"
                };

            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Email did not confirm",
                Errors = result.Errors.Select(x => x.Description)
            };
        }

        public async Task<UserManagerResponse> ForgotPasswordAsync(string userId)
        {
            var user = await _userManager.FindByEmailAsync(userId);
            if (user == null)
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "No user associated with email",
                    Errors = new string[] { "No user associated with the email" }
                };

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Encoding.UTF8.GetBytes(token);
            var validToken = WebEncoders.Base64UrlEncode(encodedToken);

            //string url = $"{_configuration["AppUrl"]}/ResetPassword?email={userId}&token={validToken}";

            var mailData = new MailData
            {
                EmailBody = "Follow the instructions to reset your password. To reset your password, please copy the code below and paste it in the verification code section of the password reset in the wabp mobile app. Code: " + validToken,
                EmailSubject = "West African Book Publishers Forgot Password",
                EmailToId = userId,
                EmailToName = userId
            };

            var result = await _mailService.SendMailAsync(mailData);

            //var res = SendMail(userId, "Wabp Reset Password", "Follow the instructions to reset your password. To reset your password, please copy the code below and paste it in the verification code section of the password reset in the wabp mobile app. Code: " + validToken);
            if (result)
                return new UserManagerResponse
                {
                    IsSuccess = true,
                    Message = "Reset Password code has been sent to your email successfully!", //"Reset Password URL has been sent to the email successfully!"
                    Email = user.Email,
                    Errors = new string[] { "No Errors" }
                };
            else
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "Unable to send reset Password code to your email!",
                    Email = user.Email,
                    Errors = new string[] { "The process timedout or server error" }
                    //Errors = new string[] { res }
                };
        }

        public async Task<UserManagerResponse> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "No user associated with the email",
                    Errors = new string[] { "No user associated with the email" }
                };

            if (model.Password != model.ConfirmPassword)
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "Password doesn't match with confirm password",
                    Errors = new string[] { "Password doesn't match with confirm password" }
                };

            var decodedToken = WebEncoders.Base64UrlDecode(model.Code);
            string normalToken = Encoding.UTF8.GetString(decodedToken);

            var result = await _userManager.ResetPasswordAsync(user, normalToken, model.Password);

            if (result.Succeeded)
                return new UserManagerResponse
                {
                    IsSuccess = true,
                    Message = "Password has been reset successfully!"
                };

            return new UserManagerResponse
            {
                Message = "Something went wrong",
                IsSuccess = false,
                Errors = result.Errors.Select(e => e.Description)
            };
        }

        public async Task<FacebookTokenValidationResult> ValidateAccessTokenAsync(string accessToken)
        {
            var formattedUrl = string.Format(TokenValidationUrl, accessToken, _facebookAuthSettings.AppId, _facebookAuthSettings.AppSecret);
            var result = await _httpClientFactory.CreateClient().GetAsync(formattedUrl);
            result.EnsureSuccessStatusCode();

            var responseAsString = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<FacebookTokenValidationResult>(responseAsString);
        }

        public async Task<FacebookUserInfoResult> GetUserInfoAsync(string accessToken)
        {
            var formattedUrl = string.Format(UserInfoUrl, accessToken, _facebookAuthSettings.AppId, _facebookAuthSettings.AppSecret);
            var result = await _httpClientFactory.CreateClient().GetAsync(formattedUrl);
            result.EnsureSuccessStatusCode();

            var responseAsString = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<FacebookUserInfoResult>(responseAsString);
        }

        public async Task<UserManagerResponse> LoginWithFacebook(string accessToken)
        {
            var validatedTokeResult = await ValidateAccessTokenAsync(accessToken);

            if (!validatedTokeResult.Data.IsValid)
            {
                return new UserManagerResponse
                {
                    Errors = new[] { "Invalid facebook token" },
                    Message = "Invalid facebook token",
                    IsSuccess = false,
                };
            }

            var userInfo = await GetUserInfoAsync(accessToken);
            var user = await _userManager.FindByEmailAsync(userInfo.Email);

            if (user == null)
            {
                var identityUser = new ApplicationUser
                {
                    Email = userInfo.Email,
                    UserName = userInfo.Email,
                    FirstName = userInfo.FirstName,
                    LastName = userInfo.LastName,
                    PhoneNumber = ""
                };

                var result = await _userManager.CreateAsync(identityUser);

                if (!result.Succeeded)
                {
                    return new UserManagerResponse
                    {
                        Message = "User not created, " + result.Errors.Select(e => e.Description),
                        IsSuccess = false,
                        Errors = result.Errors.Select(e => e.Description)
                    };

                }
                else
                {

                    //var token = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);

                    var modelData = new MailData
                    {
                        EmailBody = "Thank you for patronising WABP, You have successfully registered to be one of our numerous book lovers. If you have any questions please send a mail to info@wabp.com.ng for your enquiries. Regards West African Book Publishers Ltd",
                        EmailSubject = "West African Book Publishers Registration",
                        EmailToId = identityUser.Email,
                        EmailToName = "Wabp Ebooks"
                    };

                    var status = await _mailService.SendMailAsync(modelData);

                    return await GenerateUserToken(identityUser);
                }
            }

            return await GenerateUserToken(user);
        }

        private async Task<UserManagerResponse> GenerateUserToken(ApplicationUser user)
        {
            var claims = new[]
            {
                new Claim("Email", user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AuthSettings:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["AuthSettings:Issuer"],
                audience: _configuration["AuthSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            string tokenAsString = new JwtSecurityTokenHandler().WriteToken(token);

            return new UserManagerResponse
            {
                Message = tokenAsString,
                IsSuccess = true,
                ExpireDate = token.ValidTo,
                Email = user.Email,
                Firstname = user.FirstName,
                Lastname = user.LastName
            };
        }

        public async Task<GoogleUserRequest> GetGoogleValidationAccessToken(string accessToken)
        {
            var formattedUrl = string.Format(GoogleTokenValidationUrl, accessToken);
            var result = await _httpClientFactory.CreateClient().GetAsync(formattedUrl);
            result.EnsureSuccessStatusCode();

            var responseAsString = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GoogleUserRequest>(responseAsString);
        }

        public async Task<UserManagerResponse> LoginWithGoogle(string accessToken)
        {
            var validatedTokeResult = await GetGoogleValidationAccessToken(accessToken);

            //if (!validatedTokeResult.Data.IsValid)
            //{
            //    return new UserManagerResponse
            //    {
            //        Errors = new[] { "Invalid facebook token" },
            //        Message = "Invalid facebook token",
            //        IsSuccess = false,
            //    };
            //}

            var userInfo = await GetUserInfoAsync(accessToken);
            var user = await _userManager.FindByEmailAsync(userInfo.Email);

            if (user == null)
            {
                var identityUser = new ApplicationUser
                {
                    Email = userInfo.Email,
                    UserName = userInfo.Email,
                    FirstName = userInfo.FirstName,
                    LastName = userInfo.LastName,
                    PhoneNumber = ""
                };

                var result = await _userManager.CreateAsync(identityUser);

                if (!result.Succeeded)
                {
                    return new UserManagerResponse
                    {
                        Message = "User not created",
                        IsSuccess = false,
                        Errors = result.Errors.Select(e => e.Description)
                    };

                }
                else
                {

                    //var token = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);

                    var modelData = new MailData
                    {
                        EmailBody = "Thank you for patronising WABP, You have successfully registered to be one of our numerous book lovers. If you have any questions please send a mail to info@wabp.com.ng for your enquiries. Regards West African Book Publishers Ltd",
                        EmailSubject = "West African Book Publishers Registration",
                        EmailToId = identityUser.Email,
                        EmailToName = "Wabp Ebooks"
                    };

                    var status = await _mailService.SendMailAsync(modelData);

                    //var status = await _mailService.SendEmailAsync(identityUser.Email, "West African Book Publishers Registration", "Thank you for patronising WABP, You have successfully registered to be one of our numerous book lovers. If you have any questions please send a mail to info@wabp.com.ng for your enquiries.<br/> Regards West African Book Publishers Ltd");

                    return await GenerateUserToken(identityUser);
                }
            }

            return await GenerateUserToken(user);
        }

        //public async Task<IActionResult> GoogleAuthenticate([FromBody] GoogleUserRequest request)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState.Values.SelectMany(it => it.Errors).Select(it => it.ErrorMessage));
        //    return Ok(GenerateUserToken(await _userService.AuthenticateGoogleUserAsync(request)));
        //}

        //public async Task AuthenticateGoogleUserAsync(GoogleUserRequest request)
        //{
        //    Payload payload = await ValidateAsync(request.IdToken, new ValidationSettings
        //    {
        //        Audience = new[] { Startup.StaticConfig["Authentication: Google:ClientId"] }
        //    });

        //    return await GetOrCreateExternalLoginUser(GoogleUserRequest.PROVIDER, payload.Subject, payload.Email, payload.GivenName, payload.FamilyName);
        //}

        //private async Task GetOrCreateExternalLoginUser(string provider, string key, string email, string firstName, string lastName)
        //{
        //    var user = await _userManager.FindByLoginAsync(provider, key);
        //    if (user != null)
        //        return user;
        //    user = await _userManager.FindByEmailAsync(email);
        //    if (user == null)
        //    {
        //        user = new AppUser
        //        {
        //            Email = email,
        //            UserName = email,
        //            FirstName = firstName,
        //            LastName = lastName,
        //            Id = key,
        //        };
        //        await _userManager.CreateAsync(user);
        //    }

        //    var info = new UserLoginInfo(provider, key, provider.ToUpperInvariant());
        //    var result = await _userManager.AddLoginAsync(user, info);
        //    if (result.Succeeded)
        //        return user;
        //    return null;

        //}
    }

    public class UserManagerResponse
    {
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public DateTime? ExpireDate { get; set; }
        public string Email { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
    }
}
