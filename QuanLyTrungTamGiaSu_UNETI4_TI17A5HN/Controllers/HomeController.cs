using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Models;

namespace QuanLyTrungTamGiaSu_UNETI4_TI17A5HN.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult KhongCoQuyen()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
