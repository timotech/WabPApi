using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WabPApi.Data;
using WabPApi.Models;

namespace WabPApi.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment hostingEnvironment;
        public Size OriginalImageSize { get; set; }        //Store original image size.
        public Size NewImageSize { get; set; }

        public AdminController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            db = context;
            this.hostingEnvironment = hostingEnvironment;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddCategory(int? Id)
        {
            var AllCategories = db.Categories.ToList();
            //AllCategories.Insert(0, new Category { ID = 0, Description = "Select Vehicle Type" });
            ViewBag.AllCategories = AllCategories;

            var model = new Categories();
            if (Id != null && Id != 0)
            {
                var details = db.Categories.Where(x => x.Id == Id).FirstOrDefault();
                if (details != null)
                    model = details;
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult AddCategory(Categories model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id != 0)
                {
                    var cat = db.Categories.Where(x => x.Id == model.Id).FirstOrDefault();
                    if (cat != null)
                    {
                        cat.Description = model.Description;
                    }
                    db.Categories.Update(cat);
                }
                else
                    db.Categories.Add(model);

                db.SaveChanges();
                return RedirectToAction("AddCategory", new { Id = 0 });
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult DeleteCategory(int Id)
        {
            if (Id != 0)
            {
                var getCat = db.Categories.Where(x => x.Id == Id).FirstOrDefault();
                db.Categories.Remove(getCat);
                db.SaveChanges();
            }
            return RedirectToAction("AddCategories", new { Id = 0 });
        }

        [HttpGet]
        public IActionResult AddBanner(int? ID)
        {
            var model = new Banner();
            if (ID != null && ID != 0)
            {
                var details = db.Banners.Where(x => x.ID == ID).FirstOrDefault();
                if (details != null)
                    model = details;
            }
            var allProducts = db.Banners.OrderByDescending(x => x.ID).ToList();
            ViewBag.Banners = allProducts;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddBanner(Banner model)
        {
            if ((model.fleUploadImage != null) && (model.fleUploadImage.Length > 0))
            {
                model.PicPath = await SaveImage(model.fleUploadImage);
            }

            if(ModelState.IsValid)
            {
                if(model.ID > 0)
                {
                    var banner = db.Banners.Where(x => x.ID == model.ID).FirstOrDefault();

                    banner.PicPath = model.PicPath;
                    banner.Title = model.Title;
                    banner.DateAdded = DateTime.Now;

                    db.Banners.Update(banner);
                }
                else
                {
                    model.DateAdded = DateTime.Now;
                    db.Banners.Add(model);
                }
                
                await db.SaveChangesAsync();
                return RedirectToAction("AddBanner", new { ID = 0 });
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult DeleteBanner(int ID)
        {
            if (ID != 0)
            {
                var getProd = db.Banners.Where(x => x.ID == ID).FirstOrDefault();
                db.Banners.Remove(getProd);
                db.SaveChanges();
            }
            return RedirectToAction("AddBanner");
        }

        [HttpGet]
        public IActionResult AddBook(int? Id)
        {
            var model = new Book();
            if (Id != null && Id != 0)
            {
                var details = db.Books.Where(x => x.Id == Id).FirstOrDefault();
                model = details;
            }
            //var categories = db.Category.Select(x => new SelectListItem { Text = x.Description, Value = x.ID.ToString() }).ToList();
            var AllCategories = db.Categories.ToList();
            AllCategories.Insert(0, new Categories { Id = 0, Description = "Select Category" });
            ViewBag.Categories = AllCategories;

            var allProducts = db.Books.Include(x => x.Categories).OrderBy(x => x.Description).ToList();
            ViewBag.AllProducts = allProducts;

            //Temporary code to add base64 to epubs must be commented out
            //foreach (var item in allProducts.Where(x => x.EbookPath != null).ToList())
            //{
            //    ConvertToBase64AndSave(item);
            //}

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddBook(Book model)
        {
            if ((model.fleUploadImage != null) && (model.fleUploadImage.Length > 0))
            {
                model.PicPath = await SaveImage(model.fleUploadImage);
            }

            if ((model.fleUploadEbook != null) && (model.fleUploadEbook.Length > 0))
            {

                //string fileExtention = model.fleUploadEbook.ContentType;
                //int fileLenght = (int)model.fleUploadEbook.Length;
                var path = Path.Combine(
                        hostingEnvironment.WebRootPath, @"images\ebooks");
                string uniqueFileName = Guid.NewGuid().ToString(); // Path.GetFileName(model.fleUploadEbook.FileName);
                uniqueFileName = uniqueFileName.Replace("-", string.Empty);

                string filePath = Path.Combine(path, uniqueFileName + ".epub");
                model.EbookPath = uniqueFileName;

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.fleUploadEbook.CopyToAsync(stream);
                }

                var path2 = Path.Combine(
                hostingEnvironment.WebRootPath, @"images\compressed"); //Path.GetFileNameWithoutExtension(model.EbookPath)
                string filePath2 = Path.Combine(path2, uniqueFileName + ".txt");

                byte[] bytes = System.IO.File.ReadAllBytes(filePath);
                string file = Convert.ToBase64String(bytes);

                //byte[] file = System.IO.File.ReadAllBytes(filePath);
                // Compress
                //byte[] compress = Compress(file);

                //string encoded = base64_encode(compress);

                //model.Base64Code = encoded;
                System.IO.File.WriteAllText(filePath2, file);

            }

            if (ModelState.IsValid)
            {
                if (model.Id > 0)
                {
                    var getBook = db.Books.Where(x => x.Id == model.Id).FirstOrDefault();
                    if (getBook != null)
                    {                
                        getBook.Base64Code = model.Base64Code;
                        getBook.Title = model.Title;
                        getBook.Description = model.Description;
                        getBook.FullInfo = model.FullInfo;
                        getBook.CategoryId = model.CategoryId;
                        getBook.PicPath = model.PicPath;
                        getBook.Price = model.Price;
                        getBook.SpecialPrice = model.SpecialPrice;
                        getBook.DateAdded = DateTime.Now;
                        getBook.Genre = model.Genre;
                        getBook.Author = model.Author;
                        getBook.EbookPath = model.EbookPath;
                    }
                    db.Books.Update(getBook);
                }
                else
                {
                    model.DateAdded = DateTime.Now;
                    db.Books.Add(model);
                }


                db.SaveChanges();
                return RedirectToAction("AddBook", new { Id = 0 });
            }

            var AllCategories = db.Categories.ToList();
            AllCategories.Insert(0, new Categories { Id = 0, Description = "Select Category" });
            ViewBag.Categories = AllCategories;

            return View(model);
        }

        public static byte[] Compress(byte[] data)
        {
            using (var compressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(data, 0, data.Length);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }

        public string base64_encode(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            return Convert.ToBase64String(data);
        }

        public void ConvertToBase64AndSave(Book model)
        {
            if (model != null)
            {
                if (model.EbookPath != null)
                {
                    var path = Path.Combine(
                hostingEnvironment.WebRootPath, @"images\ebooks");
                    string filePath = Path.Combine(path, model.EbookPath);

                    var path2 = Path.Combine(
                hostingEnvironment.WebRootPath, @"images\compressed");
                    string filePath2 = Path.Combine(path2, Path.GetFileNameWithoutExtension(model.EbookPath) + ".txt");

                    byte[] bytes = System.IO.File.ReadAllBytes(filePath);
                    string file = Convert.ToBase64String(bytes);

                    //byte[] file = System.IO.File.ReadAllBytes(filePath);

                    //// Compress
                    //byte[] compress = Compress(file);

                    //string encoded = base64_encode(compress);

                    //model.Base64Code = encoded;
                    System.IO.File.WriteAllText(filePath2, file);
                }
                
                //db.Books.Update(model);
                //db.SaveChanges();
            }            
        }

        private void CompressThis(string inFile)
        {
            var path = Path.Combine(
                hostingEnvironment.WebRootPath, @"images\ebooks");
            string filein = Path.Combine(path, inFile);

            var path2 = Path.Combine(
                hostingEnvironment.WebRootPath, @"images\compressed");
            string compressedFileName = Path.Combine(path2, inFile);

            using (FileStream sourceFile = System.IO.File.OpenRead(filein))
            using (FileStream destinationFile = System.IO.File.Create(compressedFileName))
            using (GZipStream output = new GZipStream(destinationFile, CompressionMode.Compress))
            {
                sourceFile.CopyTo(output);
            }
        }

        [HttpPost]
        public IActionResult DeleteBook(int Id)
        {
            if (Id != 0)
            {
                var getProd = db.Books.Where(x => x.Id == Id).FirstOrDefault();
                db.Books.Remove(getProd);
                db.SaveChanges();
            }
            return RedirectToAction("AddBook");
        }

        [HttpGet]
        public IActionResult ViewAllCustomers()
        {
            ViewBag.AllCustomers = db.Users.ToList();
            return View();
        }

        [HttpGet]
        public IActionResult Customers()
        {
            ViewBag.AllCustomers = db.Orders.Join(db.Collections, o => o.Email, c => c.Email, (o, c) => new
            {
                o.FirstName,
                o.LastName,
                o.Email,
                o.Phone,
                Status = c.NumberOfDownloads == 1 ? "Downloaded" : "Not Yet",
                c.Id,
                c.BookId
            })
                .Join(db.Books, q => q.BookId, b => b.Id, (q, b) => new
                {
                    q.FirstName,
                    q.LastName,
                    q.Email,
                    q.Phone,
                    q.Status,
                    q.Id,
                    BookName = b.Title,
                    b.PicPath
                })
                .Select(x => new CustomerInfo { Email = x.Email, FirstName = x.FirstName, LastName = x.LastName, Phone = x.Phone, Status = x.Status, Id = x.Id, BookName = x.BookName, PicPath = x.PicPath }).Distinct().OrderBy(x => x.Email).ToList();
            return View();
        }

        [HttpPost]
        public IActionResult EditCustomer(int Id)
        {
            if(Id != 0)
            {
                var col = db.Collections.Where(x => x.Id == Id).FirstOrDefault();
                if(col != null)
                {
                    col.NumberOfDownloads = 0;
                    db.Collections.Update(col);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Customers");
        }

        [HttpGet]
        public IActionResult EditAboutUs()
        {
            var model = new AboutUs();
            return View(model);
        }

        [HttpPost]
        public IActionResult EditAboutUs(AboutUs model)
        {
            if(ModelState.IsValid)
            {
                var getDetails = db.AboutUs.FirstOrDefault();
                if(getDetails != null)
                {
                    getDetails.Description = model.Description;
                    db.AboutUs.Update(getDetails);
                }
                else
                {
                    db.AboutUs.Add(model);
                }
                
                db.SaveChanges();
                return RedirectToAction("EditAboutUs");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Orders()
        {
            var model = db.OrderDetails.Include(d => d.Order).Include(x => x.Book).OrderByDescending(x => x.DeliveryStatus == 0).ThenByDescending(x => x.Order.OrderDate).ToList();
            return View(model);
        }

        public IActionResult Reports(DateTime? fromDate, DateTime? toDate)
        {
            var totAmount = 0;

            if (fromDate != null && toDate != null)
            {
                ViewBag.Sales = db.OrderDetails.Include(d => d.Order).Include(x => x.Book).Where(x => x.Order.OrderDate >= fromDate && x.Order.OrderDate <= toDate).GroupBy(x => new { x.Book.Title, x.Book.PicPath, x.BookId, x.Quantity, x.UnitPrice }).Select(y => new
                {
                    filt = y.Key,
                    quant = y.Count(),
                    tot = y.Sum(b => b.UnitPrice)
                }).Select(t => new Sales
                {
                    BookName = t.filt.Title,
                    PicPath = t.filt.PicPath,
                    UnitPrice = t.filt.UnitPrice,
                    Quantity = t.quant,
                    Total = t.tot
                }).ToList();                
            }
            else
            {
                ViewBag.Sales = db.OrderDetails.Include(d => d.Order).Include(x => x.Book).GroupBy(x => new { x.Book.Title, x.Book.PicPath, x.BookId, x.Quantity, x.UnitPrice }).Select(y => new
                {
                    filt = y.Key,
                    quant = y.Count(),
                    tot = y.Sum(b => b.UnitPrice)
                }).Select(t => new Sales
                {
                    BookName = t.filt.Title,
                    PicPath = t.filt.PicPath,
                    UnitPrice = t.filt.UnitPrice,
                    Quantity = t.quant,
                    Total = t.tot
                }).ToList();
            }

            foreach (var item in ViewBag.Sales)
            {
                totAmount += item.Total;
            }
            ViewBag.Total = totAmount;
            return View();
        }

        [HttpPost]
        public IActionResult Reports(DateTime fromDate, DateTime toDate)
        {
            return RedirectToAction("Reports", new { fromDate, toDate });
        }


        public IActionResult ProcessOrder(int? Id)
        {
            //Update order status, 0 = in process, 1 = on the way to delivery, 2 = delivered
            var getOrder = db.OrderDetails.Where(x => x.OrderDetailId == Id).FirstOrDefault();
            if (getOrder != null)
            {
                getOrder.DeliveryStatus = 1;
                db.Update(getOrder);
                db.SaveChanges();
            }

            return RedirectToAction("Orders");
        }

        public IActionResult Deliveries()
        {
            var model = db.OrderDetails.Include(d => d.Order).Include(x => x.Book).OrderByDescending(x => x.Order.OrderDate).Where(x => x.DeliveryStatus != 0).ToList();
            return View(model);
        }

        public IActionResult ProcessDelivery(int? Id)
        {
            //Update order status, 0 = in process, 1 = on the way to delivery, 2 = delivered
            var getOrder = db.OrderDetails.Where(x => x.OrderDetailId == Id).FirstOrDefault();
            if (getOrder != null)
            {
                getOrder.DeliveryStatus = 2;
                db.Update(getOrder);
                db.SaveChanges();
            }

            return RedirectToAction("Deliveries");
        }

        public static Image ScaleImage(Image image, int maxHeight)
        {
            var ratio = (double)maxHeight / image.Height;
            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);
            var newImage = new Bitmap(newWidth, newHeight);
            using (var g = Graphics.FromImage(newImage))
            {
                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }

        public async Task<string> SaveImage(IFormFile formFile)
        {
            var strFilePath = "";

            if ((formFile != null) && (formFile.Length > 0))
            {
                string fileExtention = formFile.ContentType;
                int fileLenght = (int)formFile.Length;
                var path = Path.Combine(
                        hostingEnvironment.WebRootPath, "images");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(formFile.FileName);
                string filePath = Path.Combine(path, uniqueFileName);

                if (fileExtention == "image/png" || fileExtention == "image/jpeg" || fileExtention == "image/x-png")
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        Bitmap bmpPostedImage = new Bitmap(formFile.OpenReadStream());
                        if (fileLenght >= 1048576)
                        {
                            Image objImage = ScaleImage(bmpPostedImage, 600);
                            // Saving image in jpeg format
                            objImage.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                        }
                        else if (bmpPostedImage.Width > 1500)
                        {
                            Image objImage = ScaleImage(bmpPostedImage, 600);
                            // Saving image in jpeg format
                            objImage.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                        }
                        else
                        {
                            await formFile.CopyToAsync(stream);
                        }
                    }
                }
                strFilePath = uniqueFileName;
            }
            return strFilePath;
        }

        [HttpGet]
        public async Task<IActionResult> CreateRoles(string Id = "")
        {
            ListRoles();

            if (Id != "")
            {
                var role = await roleManager.FindByIdAsync(Id);
                if (role != null)
                {
                    var model = new RolesViewModel
                    {
                        ID = role.Id,
                        RoleName = role.Name
                    };

                    return View(model);
                }
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoles(RolesViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityRole identityRole = new IdentityRole
                {
                    Name = model.RoleName
                };

                IdentityResult result = await roleManager.CreateAsync(identityRole);

                if (result.Succeeded)
                {
                    return RedirectToAction("CreateRoles");
                }

                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        public void ListRoles()
        {
            var roles = roleManager.Roles.ToList();
            ViewBag.AllRoles = roles;
        }

        [HttpGet]
        public IActionResult EditRoles(string Id)
        {
            return RedirectToAction("CreateRoles", Id);
        }

        public async Task<IActionResult> DeleteRoles(string Id)
        {
            if (Id != "")
            {
                var role = await roleManager.FindByIdAsync(Id);
                if (role != null)
                {
                    var result = await roleManager.DeleteAsync(role);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("CreateRoles");
                    }
                }
            }

            return RedirectToAction("CreateRoles");
        }

        [HttpGet]
        public IActionResult Register()
        {
            ListRoles();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RolesRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    //await signInManager.SignInAsync(user, isPersistent: false);  //Sign In new User

                    //Add user to selected role
                    var role = await roleManager.FindByIdAsync(model.UserRole);

                    IdentityResult roles = await userManager.AddToRoleAsync(user, role.Name);
                    if (!roles.Succeeded)
                    {
                        throw new ApplicationException("Adding user '" + user.UserName + "' to '" + model.UserRole + "' role failed with error(s): " + (object)roles.Errors);
                    }

                    //Confirm user's email automatically
                    //var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    //var result2 = await userManager.ConfirmEmailAsync(user, token);

                    //if (result2.Succeeded == false)
                    //{
                    //    ModelState.AddModelError("ConfirmationError", "Could not confirm users email, pls try again by recreating the user");
                    //}

                    return RedirectToAction("Register");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            ListRoles();
            return View(model);
        }
    }

    public class CustomerInfo
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
        public string BookName { get; set; }
        public string PicPath { get; set; }
    }

    public class Sales
    {
        public string BookName { get; set; }
        public int Quantity { get; set; }
        public string PicPath { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }
}
