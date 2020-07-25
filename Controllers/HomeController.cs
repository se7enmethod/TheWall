using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; // for session
using Microsoft.AspNetCore.Identity; // for password hashing
using TheWall.Models;

namespace TheWall.Controllers
{
    public class HomeController : Controller
    {

        private WallContext dbContext;

        public HomeController(WallContext context)
        {
            dbContext = context;
        }

        // ROUTE:               METHOD:                VIEW:
        // -----------------------------------------------------------------------------------
        // GET("")              Index()                Index.cshtml
        // POST("/register")    Create(User user)      ------ (Index.cshtml to display errors)
        // POST("/login")       Login(LoginUser user)  ------ (Index.cshtml to display errors)
        // GET("/logout")       Logout()               ------
        // GET("/success")      Success()              Success.cshtml

        [HttpGet("")]
        public IActionResult Index()
        {
            //List<User> AllUsers = dbContext.Users.ToList();
            return View();
        }

        [HttpPost("/register")]
        public IActionResult Create(User user)
        {
            if (ModelState.IsValid)
            {
                // If a User exists with provided email
                if (dbContext.Users.Any(u => u.Email == user.Email))
                {
                    // Manually add a ModelState error to the Email field
                    ModelState.AddModelError("Email", "Email already in use!");
                    return View("Index");
                }

                // hash password
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);

                // create user
                dbContext.Add(user);
                dbContext.SaveChanges();

                // sign user into session
                var NewUser = dbContext.Users.FirstOrDefault(u => u.Email == user.Email);
                int UserId = NewUser.UserId;
                HttpContext.Session.SetInt32("UserId", UserId);
                HttpContext.Session.SetString("UserFirstName", user.FirstName);
                HttpContext.Session.SetString("UserLastName", user.LastName);

                // go to success
                return RedirectToAction("Wall");
            }
            // display errors
            else
            {
                return View("Index");
            }
        }

        [HttpPost("/login")]
        public IActionResult Login(LoginUser user)
        {
            if (ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == user.LoginEmail);
                if (userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("LoginEmail", "Invalid Email/Password");
                    return View("Index");
                }
                // Initialize hasher object
                var hasher = new PasswordHasher<LoginUser>();

                // verify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(user, userInDb.Password, user.LoginPassword);
                if (result == 0)
                {
                    // handle failure (this should be similar to how "existing email" is handled)
                    ModelState.AddModelError("LoginPassword", "Password is invalid.");
                    return View("Index");
                }

                // sign user into session
                int UserId = userInDb.UserId;
                HttpContext.Session.SetInt32("UserId", UserId);
                HttpContext.Session.SetString("UserFirstName", userInDb.FirstName);
                HttpContext.Session.SetString("UserLastName", userInDb.LastName);


                return RedirectToAction("Wall");
            }
            // display errors
            else
            {
                return View("Index");
            }
        }

        [HttpGet("/logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        ///////////// BEGINNING OF CRUD METHODS FOR MESSAGE MODEL /////////////

        // GET ALL Messages
        [HttpGet("wall")]
        public IActionResult Wall()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.AllMessages = dbContext.Messages
                .Include(m => m.Comments)
                .Include(m => m.User)
                .OrderByDescending(m => m.CreatedAt)
                .ToArray();

            if (TempData["MessageIdForError"] != null)
            {
                ViewBag.MessageIdForError = TempData["MessageIdForError"];
                ViewBag.ErrorMessage = "Comments cannot be empty. ";
            }


            return View();
        }

        //  GET One Single Message (Read/Update/Delete this message)

        [HttpPost("message/create")]
        public IActionResult CreateMessage(Message message)
        {
            message.UserId = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));

            if (ModelState.IsValid)
            {
                dbContext.Add(message);
                dbContext.SaveChanges();
                return RedirectToAction("Wall");
            }

            ViewBag.AllMessages = dbContext.Messages
                .Include(m => m.Comments)
                .Include(m => m.User)
                .OrderByDescending(m => m.CreatedAt)
                .ToArray();

            return View("Wall");
        }

        ///////////// END OF CRUD METHODS FOR MESSAGE MODEL /////////////

        ///////////// BEGINNING OF CRUD METHODS FOR COMMENT MODEL /////////////

        // POST Create One Single Comment
        [HttpPost("comment/create/{messageId}")]
        public IActionResult CreateComment(int messageId)
        {
            string content = Request.Form["CommentContent"];
            if (content.Length == 0 || content.Length > 255)
            {
                // error
                TempData["MessageIdForError"] = messageId;
                TempData["ErrorMessage"] = "Comments cannot be empty. ";

                return RedirectToAction("Wall");

            }

            User user = dbContext.Users.FirstOrDefault(u => u.UserId == Convert.ToInt32(HttpContext.Session.GetInt32("UserId")));
            Message message = dbContext.Messages.FirstOrDefault(m => m.MessageId == messageId);
            Comment comment = new Comment() { CommentContent = content, User = user, UserId = user.UserId, Message = message, MessageId = message.MessageId };
            dbContext.Add(comment);
            dbContext.SaveChanges();
            return RedirectToAction("Wall");

        }

        ///////////// END OF CRUD METHODS FOR COMMENT MODEL /////////////

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

