using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using AdvDictionaryServer.Models;

namespace AdvDictionaryServer.Controllers
{
    public class JSONDataController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;

        public JSONDataController(UserManager<User> UserManager, SignInManager<User> SignInManager)
        {
            userManager = UserManager;
            signInManager = SignInManager;
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
        public async Task<JsonResult> SetWordPriority(Language language)
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

        
    }
}
