using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WabPApi.Data;
using WabPApi.Models;
using WabPApi.Services;

namespace WabPApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment hostingEnvironment;
        private IMailService _mailService;

        public BooksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostingEnvironment, IMailService mailService)
        {
            _context = context;
            _userManager = userManager;
            this.hostingEnvironment = hostingEnvironment;
            _mailService = mailService;
        }

        // GET: api/Books
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookDTO>>> GetBook()
        {
            return await _context.Books.Include(x => x.Categories).Where(x => x.Categories.Description != "PROSE").Select(x => new BookDTO {
                Id = x.Id,
                Title = x.Title,
                PicPath = "https://www.wabpapp.com/images/" + x.PicPath,
                Author = x.Author,
                Genre = x.Genre,
                Price = x.Price,
                SpecialPrice = x.SpecialPrice,
                Description = x.Description,
                FullInfo = x.FullInfo
            }).Take(10).ToListAsync();
        }

        [HttpGet("BestBooks")]
        public async Task<ActionResult<IEnumerable<BookDTO>>> BestBooks()
        {
            return await _context.Books.Include(x => x.Categories).Select(x => new BookDTO
            {
                Id = x.Id,
                Title = x.Title,
                PicPath = "https://www.wabpapp.com/images/" + x.PicPath,
                Author = x.Author,
                Genre = x.Genre,
                Price = x.Price,
                SpecialPrice = x.SpecialPrice,
                Category = x.Categories.Description,
                Description = x.Description,
                FullInfo = x.FullInfo
            }).Where(x => x.Category == "PROSE").Take(10).ToListAsync();
        }

        [HttpGet("BestSelling")]
        public async Task<ActionResult<IEnumerable<BookDTO>>> BestSelling()
        {
            var allBest = await _context.Collections.Include(x => x.Book).Select(x => new BookDTO
            {
                Id = x.BookId,
                Title = x.Book.Title,
                PicPath = "https://www.wabpapp.com/images/" + x.Book.PicPath,
                Author = x.Book.Author,
                Genre = x.Book.Genre,
                Price = x.Book.Price,
                SpecialPrice = x.Book.SpecialPrice,
                Category = x.Book.Categories.Description,
                Description = x.Book.Description,
                FullInfo = x.Book.FullInfo
            }).ToListAsync();

            var secAll = allBest.GroupBy(x => new { x.Id, x.Author, x.Title, x.PicPath, x.Genre, x.Price, x.SpecialPrice, x.Category, x.Description, x.FullInfo }).Select(y => new
            {
                filt = y.Key,
                tot = y.Count()
            }).Where(t => t.tot > 2).Select(x => new BookDTO {
                Id = x.filt.Id,
                Title = x.filt.Title,
                PicPath = x.filt.PicPath,
                Author = x.filt.Author,
                Genre = x.filt.Genre,
                Price = x.filt.Price,
                SpecialPrice = x.filt.SpecialPrice,
                Category = x.filt.Category,
                Description = x.filt.Description,
                FullInfo = x.filt.FullInfo
            }).ToList();

            return secAll;
        }

        [HttpGet("MostViewed")]
        public async Task<ActionResult<IEnumerable<BookDTO>>> MostViewed()
        {
            return await _context.Books.Include(x => x.Categories).Select(x => new BookDTO
            {
                Id = x.Id,
                Title = x.Title,
                PicPath = "https://www.wabpapp.com/images/" + x.PicPath,
                Author = x.Author,
                Genre = x.Genre,
                Price = x.Price,
                SpecialPrice = x.SpecialPrice,
                Category = x.Categories.Description,
                Description = x.Description,
                FullInfo = x.FullInfo
            }).Where(x => x.Category == "DRAMA").Take(10).ToListAsync();
        }

        [HttpGet("GetCategories")]
        public async Task<ActionResult<IEnumerable<BookDTO>>> GetCategories(int id)
        {
            return await _context.Books.Include(x => x.Categories).Select(x => new BookDTO
            {
                Id = x.Id,
                Title = x.Title,
                PicPath = "https://www.wabpapp.com/images/" + x.PicPath,
                Author = x.Author,
                Genre = x.Genre,
                Price = x.Price,
                SpecialPrice = x.SpecialPrice,
                Category = x.Categories.Description,
                CategoryId = x.Categories.Id,
                Description = x.Description,
                FullInfo = x.FullInfo
            }).Where(x => x.CategoryId == id).Take(10).ToListAsync();
        }

        [HttpGet("GetAboutUs")]
        public async Task<ActionResult<AboutUs>> GetAboutUs()
        {
            return await _context.AboutUs.FirstOrDefaultAsync();
        }

        [HttpGet("GetBanners")]
        public async Task<ActionResult<IEnumerable<Banner>>> GetBanners()
        {
            var path = Path.Combine(
                hostingEnvironment.WebRootPath, @"images");

            var banners = await _context.Banners.OrderByDescending(x => x.ID).Select(x => new Banner { 
             ID = x.ID,
             Title = x.Title,
             PicPath = "https://www.wabpapp.com/images/" + x.PicPath
            }).Take(6).ToListAsync();
            return banners.OrderBy(x => x.ID).ToList();
        }

        [HttpGet("GetMyCollection")]
        public async Task<ActionResult<IEnumerable<BookDTO>>> GetMyCollection(string Email)
        {
            //byte[] decoded = base64_decode(encoded);
            //byte[] decompressed = Decompress(decoded);
            //File.WriteAllBytes(@"c:\out.pdf", decompressed);
            var path2 = Path.Combine(
                hostingEnvironment.WebRootPath, @"images\compressed");

            //https://www.wabpreader.com.ng/

            var colls = await _context.Collections.Where(x => x.Email == Email).Include(x => x.Book).Select(x => new BookDTO
            {
                Id = x.Book.Id,
                Title = x.Book.Title,
                PicPath = "https://www.wabpapp.com/images/" + x.Book.PicPath,
                Author = x.Book.Author,
                Description = x.Book.Description,
                EbookPath = x.Book.EbookPath,
                Base64Code = x.Book.Base64Code,
                NumberOfDownloads = x.NumberOfDownloads
                //FileSize = GetFileSize(Path.Combine(path2, Path.GetFileNameWithoutExtension(x.Book.EbookPath) + ".txt"))
            }).ToListAsync(); //Remember to check user authentication

            foreach(var item in colls)
            {
                item.FileSize = GetFileSize(Path.Combine(path2, Path.GetFileNameWithoutExtension(item.EbookPath) + ".txt"));
            }
            return colls;
        }

        [HttpPut("UpdateCollection")]
        public async Task<UserCartResponse> UpdateCollection(int id, string email)
        {
            var bolCollection = _context.Collections.Where(x => x.BookId == id && x.Email == email).FirstOrDefault();

            if (bolCollection  != null)
            {
                bolCollection.NumberOfDownloads = bolCollection.NumberOfDownloads + 1;
                _context.Update(bolCollection);
                //_context.Entry(bolCollection).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return new UserCartResponse
                {
                    Message = "Success",
                    IsSuccess = true,
                    Email = email
                };
            }

            return new UserCartResponse
            {
                Message = "Failure",
                IsSuccess = false,
                Email = email
            };
        }

        [HttpGet("CheckDownload")]
        public async Task<int> CheckDownload(int id, string email)
        {
            var getOrderDetails = await _context.OrderDetails.Include(x => x.Order).Where(x => x.BookId == id && x.Order.Email == email).FirstOrDefaultAsync();
            var bolCollection = await _context.Collections.Where(x => x.BookId == id && x.Email == email).FirstOrDefaultAsync();

            if (bolCollection != null && getOrderDetails != null)
            {
                if(bolCollection.NumberOfDownloads < getOrderDetails.Quantity)
                    return 0; //zero means allow
            }

            return 1; //1 means disallow
        }

        private long GetFileSize(string filename)
        {
            FileInfo file = new FileInfo(filename);
            if(file.Exists)
            {
                return file.Length;
            }
            return 0;
        }

        public static byte[] base64_decode(string encodedData)
        {
            byte[] encodedDataAsBytes = Convert.FromBase64String(encodedData);
            return encodedDataAsBytes;
        }

        public static string Decompress(byte[] data)
        {
            using (var compressedStream = new MemoryStream(data))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                var buffer = new byte[4096];
                int read;

                while ((read = zipStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    resultStream.Write(buffer, 0, read);
                }

                return resultStream.ToString();
            }
        }

        //public static byte[] Decompress(byte[] data)
        //{
        //    using (var compressedStream = new MemoryStream(data))
        //    using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
        //    using (var resultStream = new MemoryStream())
        //    {
        //        var buffer = new byte[4096];
        //        int read;

        //        while ((read = zipStream.Read(buffer, 0, buffer.Length)) > 0)
        //        {
        //            resultStream.Write(buffer, 0, read);
        //        }

        //        return resultStream.ToArray();
        //    }
        //}

        [EnableCors("AllowOrigin")]
        [HttpGet("GetMyBook/{id}")]
        public async Task<ActionResult<BookDTO>> GetMyBook(int id)
        {
            var book = await _context.Books.Where(x => x.Id == id).Select(x => new BookDTO
            {
                Id = x.Id,
                Title = x.Title,
                PicPath = "https://www.wabpapp.com/images/" + x.PicPath,
                Author = x.Author,
                //Description = x.Description,
                //FullInfo = x.FullInfo,
                EbookPath = x.EbookPath
            }).FirstOrDefaultAsync();

            if (book == null)
            {
                return NotFound();
            }

            return book;
        }

        // GET: api/Books/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<BookDTO>>> GetBook(int id)
        {
            var book = await _context.Books.Where(x => x.Id == id).Select(x => new BookDTO
            {
                Id = x.Id,
                Title = x.Title,
                PicPath = "https://www.wabpapp.com/images/" + x.PicPath,
                Author = x.Author,
                Genre = x.Genre,
                Price = x.Price,
                SpecialPrice = x.SpecialPrice,
                Description = x.Description,
                FullInfo = x.FullInfo
            }).ToListAsync();

            if (book == null)
            {
                return NotFound();
            }

            return book;
        }

        // PUT: api/Books/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(int id, Book book)
        {
            if (id != book.Id)
            {
                return BadRequest();
            }

            _context.Entry(book).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Books
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        //[HttpPost]
        //public async Task<ActionResult<Book>> PostBook(Book book)
        //{
        //    _context.Books.Add(book);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetBook", new { id = book.Id }, book);
        //}

        [HttpPost("PostContact")]
        public async Task<Mails> PostContact(Mails model)
        {
            var modelData = new MailData
            {
                EmailBody = model.Msg,
                EmailSubject = "Mail from " + model.Name + ", " + model.Email + ", " + model.Mobile,
                EmailToId = model.Email,
                EmailToName = "Wabp Ebooks"
            };

            //var status = await _mailService.SendEmailAsync("info@wabp.com.ng", "Mail from " + model.Name + ", " + model.Email + ", " + model.Mobile, model.Msg);

            var status = await _mailService.SendMailAsync(modelData);

            if(status == true)
            {
                return new Mails
                {
                    Name = model.Name,
                    Msg = "",
                    Email = model.Email,
                    Mobile = model.Mobile
                };
            }

            return new Mails
            {
                Name = null,
                Msg = null,
                Email = null,
                Mobile = null
            };
        }

        [HttpPost]
        //[Authorize]
        public async Task<UserCartResponse> PostBook(string Email, List<MyArray> product)
        {
            var TotalCost = 0;

            //get User details from logged in user
            //var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByEmailAsync(Email);
            if (user != null)
            {
                foreach (var tots in product)
                {
                    var lineTotal = tots.price * tots.quantity;
                    TotalCost += lineTotal;
                }

                //Save Orders
                var orders = new Order
                {
                    Username = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    OrderDate = DateTime.Now,
                    Phone = user.PhoneNumber,
                    Total = TotalCost,
                };
                _context.Orders.Add(orders);

                foreach (var item in product)
                {
                    //Save OrderDetails
                    var orderDetails = new OrderDetail
                    {
                        Order = orders,
                        DeliveryStatus = 0,
                        PaymentStatus = 1,
                        BookId = item.book.Id,
                        Quantity = item.quantity,
                        UnitPrice = item.book.Price
                    };

                    _context.OrderDetails.Add(orderDetails);

                    var collectionDetails = new Collection
                    {
                        BookId = item.book.Id,
                        Email = user.Email,
                        Status = "Paid",
                        UserName = user.UserName,
                        NumberOfDownloads = 0
                    };

                    _context.Collections.Add(collectionDetails);
                }

                await _context.SaveChangesAsync();

                return new UserCartResponse
                {
                    Message = "Success",
                    IsSuccess = true,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    OrderId = orders.OrderId,
                    Total = TotalCost
                };
            }

            return new UserCartResponse
            {
                Message = "Failure",
                IsSuccess = false,
                Email = user.Email,
                Phone = user.PhoneNumber,
                OrderId = 0,
                Total = TotalCost
            };
        }

        // DELETE: api/Books/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Book>> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return book;
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }

        //api/books/search/
        [HttpGet("Search")]
        public async Task<ActionResult<IEnumerable<BookDTO>>> Search()
        {
            return await _context.Books.Select(x => new BookDTO
            {
                Id = x.Id,
                Title = x.Title,
                PicPath = "https://www.wabpapp.com/images/" + x.PicPath,
                Author = x.Author,
                Genre = x.Genre,
                Price = x.Price,
                SpecialPrice = x.SpecialPrice,
                Description = x.Description,
                FullInfo = x.FullInfo
            }).Take(100).ToListAsync();
        }

        [HttpGet("GetOrders")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrders(string username)
        {
            return await _context.OrderDetails.Include(x => x.Book).Where(x => x.Order.Username == username)
                .Select(x => new OrderDTO {
                  BookId = x.BookId,
                  OrderDate = x.Order.OrderDate,
                  OrderId = x.OrderId,
                  PicPath = "https://www.wabpapp.com/images/" + x.Book.PicPath,
                  Quantity = x.Quantity,
                  Title = x.Book.Title,
                  UnitPrice = x.UnitPrice,
                  Username = x.Order.Username
                }).ToListAsync();
        }

        [HttpGet("GetReviews")]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviews(string BookId)
        {
            return await _context.Reviews.Where(x => x.BookId == BookId).ToListAsync();
        }

        [HttpPost("PostReviews")]
        public async Task<UserCartResponse> PostReviews([FromBody] Review model)
        {
            if(ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Username);
                if (user != null)
                {
                    var rev = new Review
                    {
                        Name = user.FirstName + " " + user.LastName,
                        Username = model.Username,
                        Comment = model.Comment,
                        Rating = model.Rating,
                        BookId = model.BookId
                    };

                    _context.Reviews.Add(rev);
                    _context.SaveChanges();
                    return new UserCartResponse
                    {
                        Message = "Success",
                        IsSuccess = true,
                    };
                }
            }

            return new UserCartResponse
            {
                Message = "failure",
                IsSuccess = false,
                Errors = (IEnumerable<string>)ModelState.Select(x => x.Value.Errors)
                           .Where(y => y.Count > 0).ToList() //"Unable to post review, due to server error"
            };
        }
    }

    public class UserCartResponse
    {
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
        public int OrderId { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public decimal Total { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}
