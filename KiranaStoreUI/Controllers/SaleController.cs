using KiranaStoreUI.Models;
using KiranaStoreUI.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace KiranaStoreUI.Controllers
{
    public class SaleController : Controller
    {
        private readonly HttpClient _client;

        public SaleController(IHttpClientFactory factory)
        {
            _client = factory.CreateClient("api");
        }

        // ---------------- JWT ----------------
        private void AddJwtToken()
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // ---------------- INDEX ----------------
        public async Task<IActionResult> Index()
        {
            AddJwtToken();

            var sales =
                await _client.GetFromJsonAsync<List<Sale>>(
                    "Sale/GetAllSales");

            return View(sales);
        }

        // ---------------- CREATE GET ----------------
        public async Task<IActionResult> Create()
        {
            AddJwtToken();

            string nextInvoice =
                await _client.GetStringAsync(
                    "Sale/GetNextInvoice");

            var vm = new SaleCustomerVM
            {
                Sale = new Sale
                {
                    InvoiceNumber = nextInvoice,
                    SaleDate = DateTime.Now,
                    SaleItems = new List<SaleItem>()
                },
                Customer = new Customer()
            };

            ViewBag.NextInvoice = nextInvoice;

            return View(vm);
        }

        // ---------------- CREATE POST ----------------
        [HttpPost]
        public async Task<IActionResult> Create(SaleCustomerVM vm)
        {
            AddJwtToken();

            try
            {
                if (vm == null ||
                    vm.Sale == null ||
                    vm.Customer == null)
                {
                    ModelState.AddModelError("",
                        "Customer is required");

                    return View(vm);
                }

                // ✅ Create Customer
                var custResponse =
                    await _client.PostAsJsonAsync(
                        "Customer/AddCustomer",
                        vm.Customer);

                if (!custResponse.IsSuccessStatusCode)
                {
                    var custError =
                        await custResponse.Content.ReadAsStringAsync();

                    ModelState.AddModelError("",
                        $"Customer creation failed : {custError}");

                    return View(vm);
                }

                // ✅ Get Created Customer
                var createdCustomer =
                    await custResponse.Content
                        .ReadFromJsonAsync<Customer>();

                if (createdCustomer == null)
                {
                    ModelState.AddModelError("",
                        "Customer not returned");

                    return View(vm);
                }

                // ✅ Assign CustomerId
                vm.Sale.CustomerId =
                    createdCustomer.CustomerId;

                vm.Sale.SaleDate = DateTime.Now;

                // ✅ Fix circular refs
                if (vm.Sale.SaleItems != null)
                {
                    foreach (var item in vm.Sale.SaleItems)
                    {
                        item.Product = null;
                        item.Sale = null;
                    }
                }

                // ✅ Create Sale
                var saleResponse =
                    await _client.PostAsJsonAsync(
                        "Sale/AddSale",
                        vm.Sale);

                if (saleResponse.IsSuccessStatusCode)
                {
                    TempData["SuccessMsg"] =
                        "Sale Created Successfully";

                    return RedirectToAction("Index");
                }

                var saleError =
                    await saleResponse.Content.ReadAsStringAsync();

                ModelState.AddModelError("",
                    $"Sale creation failed : {saleError}");

                return View(vm);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("",
                    ex.Message);

                return View(vm);
            }
        }
    }
}
