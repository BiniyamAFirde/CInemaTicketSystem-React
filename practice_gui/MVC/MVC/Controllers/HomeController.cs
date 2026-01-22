using Microsoft.AspNetCore.Mvc;

public class CalculatorController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Compute(double n1, double n2)
    {
        ViewBag.Result = n1 + n2;
        return View("Result");
    }

    public IActionResult Return()
    {
        return RedirectToAction("Index");
    }
}