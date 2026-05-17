using KiranaStoreUI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace KiranaStoreUI.Controllers
{
    public class DashBoardController : Controller
    {
        private readonly HttpClient _client;

        public DashBoardController(IHttpClientFactory factory)
        {
            _client = factory.CreateClient("api");
        }

        private void AddJwtToken()
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            AddJwtToken();

            try
            {
                var allSales =
                    await _client.GetFromJsonAsync<List<Sale>>("Sale/GetAllSales")
                    ?? new List<Sale>();

                var products =
                    await _client.GetFromJsonAsync<List<Product>>("Product/GetProducts")
                    ?? new List<Product>();

                var customers =
                    await _client.GetFromJsonAsync<List<Customer>>("Customer/GetCustomers")
                    ?? new List<Customer>();

                var productDict = products.ToDictionary(p => p.ProductId);

                decimal totalProfit = 0;
                decimal totalSalesAmount = 0;

                int totalSales = allSales.Count;
                int totalCustomers = customers.Count;

                DateTime now = DateTime.Now;

                DateTime currentMonthStart =
                    new DateTime(now.Year, now.Month, 1);

                DateTime lastMonthStart =
                    currentMonthStart.AddMonths(-1);

                DateTime lastMonthEnd =
                    currentMonthStart.AddDays(-1);

                decimal currentMonthProfit = 0;
                decimal lastMonthProfit = 0;

                foreach (var sale in allSales)
                {
                    decimal purchaseTotal = 0;
                    decimal sellingTotal = 0;

                    totalSalesAmount += sale.NetAmount;

                    if (sale.SaleItems != null)
                    {
                        foreach (var item in sale.SaleItems)
                        {
                            if (productDict.TryGetValue(item.ProductId, out var product))
                            {
                                purchaseTotal +=
                                    product.PurchasePrice * item.Quantity;

                                sellingTotal +=
                                    item.Price * item.Quantity;
                            }
                        }
                    }

                    decimal profit =
                        sellingTotal - purchaseTotal - sale.Discount;

                    totalProfit += profit;

                    if (sale.SaleDate >= currentMonthStart)
                    {
                        currentMonthProfit += profit;
                    }

                    if (sale.SaleDate >= lastMonthStart &&
                        sale.SaleDate <= lastMonthEnd)
                    {
                        lastMonthProfit += profit;
                    }
                }

                decimal growthPercentage = 0;

                if (lastMonthProfit > 0)
                {
                    growthPercentage =
                        ((currentMonthProfit - lastMonthProfit)
                        / lastMonthProfit) * 100;
                }

                string businessStatus =
                    growthPercentage >= 0
                    ? "Business is Growing 📈"
                    : "Business is Decreasing 📉";

                ViewBag.TotalProfit = Math.Round(totalProfit, 2);
                ViewBag.TotalSalesAmount = Math.Round(totalSalesAmount, 2);
                ViewBag.TotalSales = totalSales;
                ViewBag.TotalCustomers = totalCustomers;
                ViewBag.GrowthPercentage = Math.Round(growthPercentage, 2);
                ViewBag.BusinessStatus = businessStatus;
                ViewBag.CurrentMonthProfit = Math.Round(currentMonthProfit, 2);
                ViewBag.LastMonthProfit = Math.Round(lastMonthProfit, 2);

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;

                return View();
            }
        }
    }
}