using Cognitic.Tools.Connections.Database;
using DemoFilesInDb.Models;
using DemoFilesInDb.Models.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DemoFilesInDb.Controllers
{
    public class HomeController : Controller
    {
        private static List<string> contentTypes = new List<string>()
        {
            "image/jpeg",
            "image/png"
        };
        
        private readonly ILogger<HomeController> _logger;
        private readonly Connection _connection;

        public HomeController(ILogger<HomeController> logger, Connection connection)
        {
            _logger = logger;
            _connection = connection;
        }

        public IActionResult Index()
        {
            Command command = new Command("Select [Id], [Name], [Type] From Picture;", false);
            return View(_connection.ExecuteReader(command, dr => new DisplayFile() { Id = (int)dr["Id"], Name = (string)dr["Name"] }));
        }

        public IActionResult Details(int id)
        {
            Command command = new Command("Select [Id], [Name], [Type], [Content] From Picture Where Id = @Id;", false);
            command.AddParameters("Id", id);
            DisplayFile displayFile = _connection.ExecuteReader(command, dr => new DisplayFile() { Id = (int)dr["Id"], Name = (string)dr["Name"], Type = (string)dr["Type"], Content = Convert.ToBase64String((byte[])dr["Content"]) }).SingleOrDefault();

            if (displayFile is null)
                return RedirectToAction("Index");

            return View(displayFile);
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(IFormCollection formCollection)
        {
            //Récupère le premier fichier du formulaire
            IFormFile formFile = formCollection.Files[0];
            //Récupère le nom du fichier
            string fileName = formFile.FileName;
            string contentType = formFile.ContentType;
            long length = formFile.Length;           

            if (!contentTypes.Contains(contentType))
            {
                ModelState.AddModelError("", "Type of file is'nt valid!");
                return View();
            }

            if (length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("", "Size of file is invalid");
                return View();
            }

            //Récupère le contenu du fichier
            BinaryReader reader = new BinaryReader(formFile.OpenReadStream());
            byte[] content = reader.ReadBytes((int)formFile.Length);

            Command command = new Command("Insert into Picture ([Name], [Type], [Content]) Values (@Name, @Type, @Content);", false);
            command.AddParameters("@Name", fileName);
            command.AddParameters("@Type", contentType);
            command.AddParameters("@Content", content);
            _connection.ExecuteNonQuery(command);

            return RedirectToAction("Index");
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
    }
}
