[HttpPost]
public async Task<IActionResult> Create(SaleCustomerVM vm)
{
    AddJwtToken();

    try
    {
        // ✅ Validation
        if (vm == null || vm.Sale == null || vm.Customer == null)
        {
            ModelState.AddModelError("", "Customer is required");
            return View(vm);
        }

        // ✅ Create Customer First
        var custResponse =
            await _client.PostAsJsonAsync(
                "Customer/AddCustomer",
                vm.Customer);

        // ❌ Customer API failed
        if (!custResponse.IsSuccessStatusCode)
        {
            var custError =
                await custResponse.Content.ReadAsStringAsync();

            ModelState.AddModelError("",
                $"Customer creation failed : {custError}");

            ViewBag.NextInvoice = vm.Sale.InvoiceNumber;

            return View(vm);
        }

        // ✅ Read Created Customer
        var createdCustomer =
            await custResponse.Content.ReadFromJsonAsync<Customer>();

        if (createdCustomer == null ||
            createdCustomer.CustomerId <= 0)
        {
            ModelState.AddModelError("",
                "Invalid customer returned from API");

            ViewBag.NextInvoice = vm.Sale.InvoiceNumber;

            return View(vm);
        }

        // ✅ Assign CustomerId to Sale
        vm.Sale.CustomerId =
            createdCustomer.CustomerId;

        // ✅ Set Sale Date
        vm.Sale.SaleDate = DateTime.Now;

        // ✅ Load products
        var products =
            await _client.GetFromJsonAsync<List<Product>>(
                "Product/GetProducts");

        decimal totalAmount = 0;

        // ✅ Calculate totals
        if (vm.Sale.SaleItems != null)
        {
            foreach (var item in vm.Sale.SaleItems)
            {
                var product =
                    products.FirstOrDefault(
                        p => p.ProductId == item.ProductId);

                if (product == null)
                    continue;

                // ✅ Auto price
                item.Price = product.SellingPrice;

                // ✅ Total
                item.Total =
                    item.Quantity * item.Price;

                totalAmount += item.Total;

                // ✅ Prevent circular reference
                item.Product = null;
                item.Sale = null;
            }
        }

        // ✅ Final Amounts
        vm.Sale.TotalAmount = totalAmount;
        vm.Sale.NetAmount =
            totalAmount - vm.Sale.Discount;

        // ✅ Create Sale
        var saleResponse =
            await _client.PostAsJsonAsync(
                "Sale/AddSale",
                vm.Sale);

        // ✅ Success
        if (saleResponse.IsSuccessStatusCode)
        {
            TempData["SuccessMsg"] =
                $"Sale created successfully! Invoice: {vm.Sale.InvoiceNumber}";

            return RedirectToAction("Index");
        }

        // ❌ Sale Failed
        var saleError =
            await saleResponse.Content.ReadAsStringAsync();

        ModelState.AddModelError("",
            $"Sale creation failed : {saleError}");

        ViewBag.NextInvoice = vm.Sale.InvoiceNumber;

        return View(vm);
    }
    catch (Exception ex)
    {
        ModelState.AddModelError("",
            ex.Message);

        ViewBag.NextInvoice = vm?.Sale?.InvoiceNumber;

        return View(vm);
    }
}
