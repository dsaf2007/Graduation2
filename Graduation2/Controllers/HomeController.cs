using Graduation2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace Graduation2.Controllers
{
    public class HomeController : Controller
    {
        // private readonly ILogger<HomeController> _logger;
        
        private IWebHostEnvironment environment;
        public HomeController(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }

        // public HomeController(ILogger<HomeController> logger)
        // {
        //     _logger = logger;
        // }

        public IActionResult Index()
        {
            // 한글 인코딩
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            string baseFilePath = this.environment.WebRootPath;

            // string inputFile = Path.Combine(baseFilePath, "upload",fileNames[0]);
            string gradeFile = Path.Combine(baseFilePath, "student_score.xlsx");

            UserInfo userInfo = new UserInfo();
            userInfo.GetUserSubject(gradeFile); // 수강 과목 리스트 및 이수 학점

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
