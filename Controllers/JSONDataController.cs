using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using AdvDictionaryServer.Models;
using AdvDictionaryServer.DBContext;

namespace AdvDictionaryServer.Controllers
{
    public class JSONDataController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly DictionaryDBContext dbcontext;

        public JSONDataController(UserManager<User> UserManager, SignInManager<User> SignInManager, DictionaryDBContext DictionaryDbContext)
        {
            userManager = UserManager;
            signInManager = SignInManager;
            dbcontext = DictionaryDbContext;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<JsonResult> Fish()
        {
            var fish = JsonConvert.SerializeObject(new {fish = "Awesome"});
            return new JsonResult(fish);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<JsonResult> Login([FromBody]LoginModel model)
        {
            //LoginModel loginModel = JsonConvert.DeserializeObject<LoginModel>(model);
            User user = await userManager.FindByEmailAsync(model.Email);
            if(user!=null){
                var result = await signInManager.PasswordSignInAsync(user,model.Password,false,false);
                if(result==Microsoft.AspNetCore.Identity.SignInResult.Success){
                    Response.ContentType = "application/json";
                    var response = new {Token = (new JwtSecurityTokenHandler()).WriteToken(await CreateToken(user)), Id = user.Id};
                    return new JsonResult(response); //return a jwt
                } else {
                    throw new NotImplementedException(); //return an error with wrong password warning
                }
            }
            throw new NotImplementedException(); //return an error with no such user warning
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<JsonResult> Register([FromBody]RegisterModel registerModel)
        {
            //RegisterModel registerModel = JsonConvert.DeserializeObject<RegisterModel>(registerModelJson);
            User user = new User { Email = registerModel.Email, UserName = registerModel.Email };
            var result = await userManager.CreateAsync(user, registerModel.Password);
            if (result.Succeeded)
            {
                await signInManager.SignInAsync(user, false);
                var jwt = await CreateToken(user);
                var jwtToken = (new JwtSecurityTokenHandler()).WriteToken(jwt);
                Response.ContentType = "application/json";
                var response = new {Token = jwtToken, Id = user.Id};
                return new JsonResult(response);
            }
            throw new NotImplementedException(); //return an error 
        }

        [HttpGet]
        public async Task<JsonResult> GetAllWordsAsync() //??
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public async Task<JsonResult> GetWordPriorities(int amount, int offset)
        {
            throw new NotImplementedException();
        }


        [HttpPost]
        public async Task<JsonResult> SetWordPriority(WordPriority WordPriority)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public async Task<JsonResult> GetLanguages()
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<JsonResult> AddLanguage(Language language)
        {
            throw new NotImplementedException();
        }
        private async Task <JwtSecurityToken> CreateToken(User user){
            return new JwtSecurityToken(
                    issuer:AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    claims: await userManager.GetClaimsAsync(user),
                    notBefore: DateTime.Now,
                    expires: DateTime.Now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(),SecurityAlgorithms.HmacSha256));
        }
        
    }
}
