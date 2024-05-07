using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WabPApi.Data;
using WabPApi.Models;

namespace WabPApi.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            db = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "Admin");
        }

        [AllowAnonymous]
        public IActionResult About()
        {
            var model = db.AboutUs.FirstOrDefault();
            return View(model);
        }
        
        [AllowAnonymous]
        public IActionResult Policy()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Terms()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult DeleteMyData()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> DeleteMyData(string username)
        {
            if (!string.IsNullOrEmpty(username.Trim()))
            {
                var user = await userManager.FindByEmailAsync(username.Trim());
                if(user != null)
                {
                    var result = await userManager.DeleteAsync(user);
                    if(result.Succeeded)
                    {
                        TempData["message"] = $"Oops {username}, we are sad you left, please find it possible to join us again!";                        
                        return RedirectToAction(nameof(DeleteMyData));
                    }
                }
                else
                {
                    TempData["message"] = $"User with email: {username} cannot be found!";
                    return RedirectToAction(nameof(DeleteMyData));
                }
            }
            else
            {
                TempData["message"] = "Please provide your registered email!!!";
                return RedirectToAction(nameof(DeleteMyData));
            }
            return View();
        }
    }
}
