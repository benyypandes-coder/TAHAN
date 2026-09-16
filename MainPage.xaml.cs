using System.Collections.ObjectModel;
using KurtDhylanMotoShopInventory.Models;
using KurtDhylanMotoShopInventory.Services;

namespace KurtDhylanMotoShopInventory;

public partial class MainPage : ContentPage
{
    readonly ObservableCollection<InventoryItem> products = new();
    readonly ObservableCollection<ServiceItem> services = new();

    public MainPage()
    {
        InitializeComponent();
        ProductList.ItemsSource = products;
        ServiceList.ItemsSource = services;
        Loaded += async (_, _) => await RefreshData();
    }

    async Task RefreshData()
    {
        products.Clear();
        foreach (var p in await InventoryStore.LoadProducts()) products.Add(p);
        services.Clear();
        foreach (var s in await InventoryStore.LoadServices()) services.Add(s);
        UpdateStats();
    }

    void UpdateStats()
    {
        ProductCount.Text = products.Count.ToString();
        ServiceCount.Text = services.Count.ToString();
        LowStockCount.Text = products.Count(x => x.Stock <= 5).ToString();
    }

    async void AddProduct_Clicked(object sender, EventArgs e)
    {
        var name = await DisplayPromptAsync("Add Product", "Product name:");
        if (string.IsNullOrWhiteSpace(name)) return;
        var brand = await DisplayPromptAsync("Add Product", "Brand:");
        var model = await DisplayPromptAsync("Add Product", "Model:");
        var priceText = await DisplayPromptAsync("Add Product", "Price:");
        var stockText = await DisplayPromptAsync("Add Product", "Stocks:");

        decimal.TryParse(priceText, out var price);
        int.TryParse(stockText, out var stock);

        products.Add(new InventoryItem { Name = name, Brand = brand ?? "", Model = model ?? "", Price = price, Stock = stock });
        await InventoryStore.SaveProducts(products.ToList());
        UpdateStats();
    }

    async void AddService_Clicked(object sender, EventArgs e)
    {
        var name = await DisplayPromptAsync("Add Service", "Labor / service name:");
        if (string.IsNullOrWhiteSpace(name)) return;
        var priceText = await DisplayPromptAsync("Add Service", "Labor price:");
        decimal.TryParse(priceText, out var price);
        services.Add(new ServiceItem { Name = name, LaborPrice = price });
        await InventoryStore.SaveServices(services.ToList());
        UpdateStats();
    }

    void ProductSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        var q = e.NewTextValue?.Trim() ?? "";
        ProductList.ItemsSource = string.IsNullOrEmpty(q)
            ? products
            : products.Where(p => $"{p.Name} {p.Brand} {p.Model}".Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    void ServiceSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        var q = e.NewTextValue?.Trim() ?? "";
        ServiceList.ItemsSource = string.IsNullOrEmpty(q)
            ? services
            : services.Where(s => s.Name.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    async void Refresh_Clicked(object sender, EventArgs e) => await RefreshData();
}
