using BTA.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
//using BTA.Models.RootObject;

namespace BTA.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        //private readonly IBookRepository _bookRepository;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            //_bookRepository = bookRepository;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index([Bind("Image,Title,Author,Rating,Comment,Pages,Status,FriendType")] Book book)
        {
            if (!ModelState.IsValid)
            {
                return View(book);
            }
            //book.OwnerId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            book.OwnerId = "2";

            int len = book.Comment.Length;
            if (len > 200)
            {
                book.Comment.Substring(0, 198);
            }

            await _unitOfWork.BookRepository.AddBook(book);


            return RedirectToAction(actionName: "AddNewBook", controllerName: "Home"); ; //TODO send the book title to the view
        }

        public async Task<IActionResult> Library(string status)
        {
            //var OwnerId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var OwnerId = "2";
            if (OwnerId == null) { return View(); }/*TODO implement error message*/

            var books = await _unitOfWork.BookRepository.GetBooks(OwnerId, status);
            return View(books);            
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> FetchBook(string book, string author)
        {
            Rootobject rootobject = new Rootobject();
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync($"https://www.googleapis.com/books/v1/volumes?q={@book}+inauthor:{@author}&maxResults=4&key=AIzaSyAwwGyMT-qHq2_mn_fLR6BoWTze9nsCa_E"))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    try
                    {
                        rootobject = JsonConvert.DeserializeObject<Rootobject>(apiResponse);
                    }
                    catch (Exception ex)
                    {
                        rootobject.items = null;
                    }
                }
            }
            return View(rootobject);
        }

        public async Task<IActionResult> DeleteBook(int bookId)
        {
            await _unitOfWork.BookRepository.RemoveBook(bookId);

            return RedirectToAction("Library");
        }

        public async Task<IActionResult> Analitycs()
        {
            //var ownerId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var ownerId = "2";
            string status = string.Empty;

            var analitics = await _unitOfWork.BookRepository.GetBooks(ownerId, status);
            if (analitics == null)
            {
                return NotFound();
            }
            var totalBooks = analitics.Count();
            var alradyRead = analitics.Count(a => a.Status == "alrady-read");
            var willRead = analitics.Count(a => a.Status == "will-read-soon");
            var readingNow = analitics.Count(a => a.Status == "reading-now");
            var wantToRead = analitics.Count(a => a.Status == "want-to-read");

            TotalBooks BooksAnalitycs = new TotalBooks();

            BooksAnalitycs.wantToRead = wantToRead;
            BooksAnalitycs.willRead = willRead;
            BooksAnalitycs.Totalbooks = totalBooks;
            BooksAnalitycs.readingNow = readingNow;
            BooksAnalitycs.alradyRead = alradyRead;


            return View(BooksAnalitycs);
        }

        public IActionResult AddNewBook()
        {
            return View();
        }
    }
}