using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
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
        public IActionResult Fish()
        {
            var fish = JsonConvert.SerializeObject(new {fish = "Awesome, " + User.Claims.Where(c => c.Type == ClaimTypes.Email).Select(c => c.Value).FirstOrDefault()});
            return new JsonResult(fish);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody]LoginModel model)
        {
            //LoginModel loginModel = JsonConvert.DeserializeObject<LoginModel>(model);
            User user = await userManager.FindByEmailAsync(model.Email);
            if(user!=null){
                var result = await signInManager.PasswordSignInAsync(user,model.Password,false,false);
                if(result==Microsoft.AspNetCore.Identity.SignInResult.Success){
                    Response.ContentType = "application/json";
                    var response = new LoginRegisterResponse(){ Token = (new JwtSecurityTokenHandler()).WriteToken(CreateToken(user)), Id = user.Id};
                    return new JsonResult(response); //return a jwt
                } else {
                   
                }
            }
            return NotFound(); //return an error with no such user warning
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<JsonResult> Register([FromBody]RegisterModel registerModel)
        {
            User user = new User { Email = registerModel.Email, UserName = registerModel.Email, NativeLanguage = registerModel.NativeLanguage };
            var result = await userManager.CreateAsync(user, registerModel.Password);
            if (result.Succeeded)
            {
                await signInManager.SignInAsync(user, false);
                var jwt = CreateToken(user);
                var jwtToken = (new JwtSecurityTokenHandler()).WriteToken(jwt);
                Response.ContentType = "application/json";
                var response = new LoginRegisterResponse() { Token = jwtToken, Id = user.Id};
                return new JsonResult(response);
            }
            throw new NotImplementedException();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<JsonResult> GetTranslations([FromBody]GetTranslations translationsModel)
        {
            User user = await GetUser();
            List<Language> allLanguages = dbcontext.Languages.ToList();
            List<Language> languages = allLanguages.Where(l => l.Name == translationsModel.Language).ToList();
            List<Language> languages2 = dbcontext.Languages.Where(l => l.User == user).ToList();
            Language language = dbcontext.Languages.Where(l => l.Name == translationsModel.Language && l.User == user).Single();
            var wordPriorities = dbcontext.WordPriorities
                                    .Where(wp => wp.Language == language && wp.ForeignWord.Word == translationsModel.Word)
                                    .Include(wp => wp.ForeignWord)
                                    .Include(wp => wp.NativePhrase)
                                    .ToList();
            List<WordPrioritiesJSON> wordPrioritiesJSON = new List<WordPrioritiesJSON>();
            foreach(var wp in wordPriorities){
                wordPrioritiesJSON.Add(
                    new WordPrioritiesJSON(){
                        Phrase = new NativePhraseJson() {Phrase = wp.NativePhrase.Phrase},
                        Word = new ForeignWordJSON(){Word = wp.ForeignWord.Word},
                        Language = new LanguageJSON(){Name = wp.Language.Name},
                        Value = wp.Value
                    }
                );
            }
            return new JsonResult(wordPrioritiesJSON);
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<JsonResult> GetForeignWordsAsync([FromBody]GetForeignWordsModel wordsModel)
        {
            User user = await GetUser();
            var nativePhrases = dbcontext.WordPriorities
                                    .Where(wp => wp.Language.User == user && wp.Language.Name == wordsModel.Language)
                                    .Select(wp => wp.ForeignWord)
                                    .Skip(wordsModel.Offset)
                                    .Take(wordsModel.Amount)
                                    .ToList();
            return new JsonResult(nativePhrases);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<JsonResult> GetWordsPriorities([FromBody]GetWordPrioritiesModel wordPrioritiesModel)
        {
            User user = await GetUser();
            var language = await dbcontext.Languages.Where(l => l.User == user && l.Name == wordPrioritiesModel.Language).SingleOrDefaultAsync();
            var wordPriorities = dbcontext.WordPriorities
                                    .Where(wp => wp.Language.User == user && wp.Language.Name == wordPrioritiesModel.Language )
                                    .Include(wp => wp.ForeignWord)
                                    .Include(wp => wp.NativePhrase)
                                    .Include(wp => wp.Language)
                                    .OrderBy(SortingOrderingCreator.CreateOrdering(wordPrioritiesModel.SortingVariant))
                                    .Skip(wordPrioritiesModel.Offset)
                                    .Take(wordPrioritiesModel.Amount)
                                    .ToList();
            List<WordPrioritiesJSON> wordPrioritiesJSON = new List<WordPrioritiesJSON>();
            foreach(var wp in wordPriorities){
                wordPrioritiesJSON.Add(
                    new WordPrioritiesJSON(){
                        Phrase = new NativePhraseJson() {Phrase = wp.NativePhrase.Phrase},
                        Word = new ForeignWordJSON(){Word = wp.ForeignWord.Word},
                        Language = new LanguageJSON(){Name = wp.Language.Name},
                        Value = wp.Value
                    }
                );
            }
            return new JsonResult(wordPrioritiesJSON);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes="Bearer")]
        public async Task<JsonResult> GetWordPrioritiesCount([FromBody]LanguageInputModel languageInputModel)
        {
            User user = await GetUser();
            int amount = await dbcontext.WordPriorities.Where(wp => wp.Language.Name==languageInputModel.Name).CountAsync();
            return new JsonResult(amount);
        } 

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> AddWordsPriorities([FromBody] List<WordPrioritiesJSON> wordPrioritiesJSON)
        {
            User user = await GetUser();
            Language language = dbcontext.Languages.Where(l => (l.User == user) &&( l.Name == wordPrioritiesJSON[0].Language.Name)).Single();
            List<WordPriority> wordPriorities = new List<WordPriority>();
            int priority = 0;
            foreach(var wp in wordPrioritiesJSON){
                priority = wp.Value > 30 ? 30 : wp.Value;
                priority = wp.Value < -30 ? -30 : wp.Value;
                wordPriorities.Add(new WordPriority(){
                    Language = language,
                    ForeignWord = new ForeignWord(){Word = wp.Word.Word},
                    NativePhrase = new NativePhrase() {Phrase = wp.Phrase.Phrase},
                    Value = priority
                });
            } 
            await dbcontext.WordPriorities.AddRangeAsync(wordPriorities);
            await dbcontext.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> UpdateWordsPriorities([FromBody] List<WordPrioritiesJSON> wordPrioritiesJSON)
        {
            User user = await GetUser();
            Language language = dbcontext.Languages.Where(l => l.User == user && l.Name == wordPrioritiesJSON[0].Language.Name).Single();
            var oldWordPriorities = dbcontext.WordPriorities.Where(wp => wp.Language == language && wp.ForeignWord.Word == wordPrioritiesJSON[0].Word.Word);
            var oldNativePhrases = oldWordPriorities.Select(wp => wp.NativePhrase);
            ForeignWord foreignWord = oldWordPriorities.Select(wp => wp.ForeignWord).Single();
            dbcontext.NativePhrases.RemoveRange(oldNativePhrases);
            dbcontext.WordPriorities.RemoveRange(oldWordPriorities);
            List<WordPriority> newWordPriorities = new List<WordPriority>();
            int priority = 0;
            foreach (var wpjson in wordPrioritiesJSON)
            {
                priority = wpjson.Value > 30 ? 30 : wpjson.Value;
                priority = wpjson.Value < -30 ? -30 : wpjson.Value;
                newWordPriorities.Add(new WordPriority(){
                    ForeignWord = foreignWord,
                    Language = language,
                    NativePhrase = new NativePhrase() {Phrase = wpjson.Phrase.Phrase},
                    Value = priority
                });
            }
            await dbcontext.WordPriorities.AddRangeAsync(newWordPriorities);
            await dbcontext.SaveChangesAsync();
            return Ok();
        } 

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> SetWordPriority([FromBody]WordPriority wordPriorityModel)
        {
            NativePhrase nativePhrase = new NativePhrase() {Phrase = wordPriorityModel.NativePhrase.Phrase };
            ForeignWord foreignWord = new ForeignWord() {Word = wordPriorityModel.ForeignWord.Word };
            User user = await GetUser();
            Language language = dbcontext.Languages.Where(l => l.User == user && l.Name ==wordPriorityModel.Language.Name).SingleOrDefault();
            WordPriority wordPriority = new WordPriority() {NativePhrase = nativePhrase, ForeignWord = foreignWord, Language = language,Value = wordPriorityModel.Value };
            dbcontext.WordPriorities.Add(wordPriority);
            await dbcontext.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes="Bearer")]
        public async Task<JsonResult> GenerateQuiz([FromBody] LanguageInputModel languageInputModel) 
        {
            User user = await GetUser();
            Language language = dbcontext.Languages.Where(l => l.User == user & l.Name == languageInputModel.Name).SingleOrDefault();
            List<WordPriority> wordPriorities = new WordPicker(dbcontext,language).GenerateWordsForQuiz(20);
            List<WordPrioritiesJSON> wordPrioritiesJSON = new List<WordPrioritiesJSON>();
            foreach(var wp in wordPriorities)
            {
                wordPrioritiesJSON.Add(new WordPrioritiesJSON()
                {
                    Value = wp.Value,
                    Language = new LanguageJSON() { Name = wp.Language.Name },
                    Phrase = new NativePhraseJson() { Phrase = wp.NativePhrase.Phrase },
                    Word = new ForeignWordJSON() { Word = wp.ForeignWord.Word }
                });
            }
            return new JsonResult(wordPrioritiesJSON);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes ="Bearer")]
        public async Task<IActionResult> SubmitQuiz([FromBody] List<WordPrioritiesJSON> wordPrioritiesJSON)
        {
            User user = await GetUser();
            Language language = dbcontext.Languages.Where(l => l.User == user & l.Name == wordPrioritiesJSON[0].Language.Name).SingleOrDefault();
            List<WordPriority> wordPriorities = new List<WordPriority>();
            WordPriority wordPriority;
            ForeignWord foreignWord;
            NativePhrase nativePhrase;
            foreach(var wpjson in wordPrioritiesJSON)
            {
                foreignWord = await dbcontext.ForeignWords.Where(fw => fw.Word == wpjson.Word.Word).SingleAsync();
                nativePhrase = await dbcontext.NativePhrases.Where(np => np.Phrase == wpjson.Phrase.Phrase).SingleAsync();
                wordPriority = await dbcontext.WordPriorities
                    .Where(wp => wp.Language == language & wp.ForeignWord == foreignWord & wp.NativePhrase == nativePhrase).SingleAsync();
                wordPriority.Value = wpjson.Value;
            }
            await dbcontext.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<JsonResult> GetLanguages()
        {
            User user = await GetUser();
            //var languages = dbcontext.WordPriorities.Where(wp => wp.User == user ).Select(wp => wp.Language).ToList();
            var languageNames = dbcontext.Languages
                                .Where(l => l.User == user).Select(l => l.Name).ToList();
            List<LanguageJSON> languages = new List<LanguageJSON>();
            foreach(var name in languageNames){
                languages.Add(new LanguageJSON(){Name = name});
            }
            return new JsonResult(languages);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> AddLanguage([FromBody]LanguageInputModel languageInputModel)
        {
            
            languageInputModel.Name.ToLower();
            languageInputModel.Name.Trim();
            User user = await GetUser();
            Language language = new Language { Name = languageInputModel.Name, User = user};
            if(!dbcontext.Languages.Contains(language)){
                await dbcontext.Languages.AddAsync(language);
                await dbcontext.SaveChangesAsync();
            }
            return Ok();
        }


        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> ForeignWordExists([FromBody]ForeignWordModel checkWordExists)
        {
            User user = await GetUser();
            Language language = await dbcontext.Languages.Where(l => l.User == user & l.Name == checkWordExists.Language).SingleOrDefaultAsync();
            ForeignWord foreignWord = await dbcontext.ForeignWords.Where(f => f.Word == checkWordExists.ForeignWord).SingleOrDefaultAsync();
            if(await dbcontext.WordPriorities.Where(wp => wp.Language == language & wp.ForeignWord == foreignWord).AnyAsync())
            {
                return Ok();
            }
            return NotFound();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes ="Bearer")]
        public async Task<IActionResult> DeleteWord([FromBody]ForeignWordModel foreignWordModel)
        {
            User user = await GetUser();
            Language language = await dbcontext.Languages.Where(l => l.User == user & l.Name == foreignWordModel.Language).SingleOrDefaultAsync();
            ForeignWord foreignWord = await dbcontext.ForeignWords.Where(f => f.Word == foreignWordModel.ForeignWord).SingleOrDefaultAsync();
            List<NativePhrase> translations;
            if (language == null | foreignWord == null) return NotFound();
            if(await dbcontext.WordPriorities.Where(wp => wp.Language != language & wp.ForeignWord == foreignWord).AnyAsync())
            {
                translations = await dbcontext.WordPriorities.Where(wp => wp.Language != language & wp.ForeignWord == foreignWord)
                    .Select(wp => wp.NativePhrase).ToListAsync();
                var wordPriorities = await dbcontext.WordPriorities.Where(wp => wp.Language == language & wp.ForeignWord == foreignWord).ToListAsync();
                dbcontext.WordPriorities.RemoveRange(wordPriorities);
                await dbcontext.SaveChangesAsync();
            } else
            {
                translations = await dbcontext.WordPriorities.Where(wp => wp.Language == language & wp.ForeignWord == foreignWord)
                    .Select(wp => wp.NativePhrase).ToListAsync();
                var wordPriorities = await dbcontext.WordPriorities.Where(wp => wp.Language == language & wp.ForeignWord == foreignWord).ToListAsync();
                dbcontext.WordPriorities.RemoveRange(wordPriorities);
                dbcontext.ForeignWords.Remove(foreignWord);
                await dbcontext.SaveChangesAsync();
            }
            foreach(var nativePhrase in translations)
            {
                if(!await dbcontext.WordPriorities.Where(wp => wp.NativePhrase == nativePhrase).AnyAsync())
                {
                    dbcontext.NativePhrases.Remove(nativePhrase);
                }
            }
            await dbcontext.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> RenameWord([FromBody]RenameWord renameWord)
        {
            User user = await GetUser();
            Language language = await dbcontext.Languages.Where(l => l.User == user & l.Name == renameWord.Language).SingleOrDefaultAsync();
            ForeignWord originalWord = await dbcontext.ForeignWords.Where(f => f.Word == renameWord.OriginalWord).SingleOrDefaultAsync();
            ForeignWord newWord = await dbcontext.ForeignWords.Where(f => f.Word == renameWord.NewWord).SingleOrDefaultAsync();
            if(newWord == null)
            {
                newWord = new ForeignWord() { Word = renameWord.NewWord };
                dbcontext.ForeignWords.Add(newWord);
                await dbcontext.SaveChangesAsync();
                newWord = await dbcontext.ForeignWords.Where(f => f.Word == renameWord.NewWord).SingleAsync();
            }
            if (language == null || originalWord == null) return NotFound();
            if (await dbcontext.WordPriorities.Where(wp => wp.ForeignWord == newWord && wp.Language == language).AnyAsync()) return Conflict();

            if(!await dbcontext.WordPriorities.Where(wp => wp.Language!=language & wp.ForeignWord == originalWord).AnyAsync())
            {
                dbcontext.ForeignWords.Remove(originalWord);
            } 
            var wordPriorities = await dbcontext.WordPriorities.Where(wp => wp.Language == language && wp.ForeignWord == originalWord).ToListAsync();
            foreach (var wp in wordPriorities)
            {
                wp.ForeignWord = newWord;
            }
            await dbcontext.SaveChangesAsync();
            return Ok();
        }

        private JwtSecurityToken CreateToken(User user){
            var claims = new List<Claim>(){
                new Claim(ClaimTypes.Email,user.Email)
            };
            return new JwtSecurityToken(
                    issuer:AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    claims: claims,
                    notBefore: DateTime.Now,
                    expires: DateTime.Now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(),SecurityAlgorithms.HmacSha256));
        }

        private async Task<User> GetUser(){
            return await userManager.FindByEmailAsync(User.Claims.Where(c => c.Type == ClaimTypes.Email).Select(c => c.Value).SingleOrDefault());
        }
        
    }
}
