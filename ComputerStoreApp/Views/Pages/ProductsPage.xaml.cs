using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ComputerStoreApp.Connect;
using ComputerStoreApp.Models;

namespace ComputerStoreApp.Views.Pages
{
    public partial class ProductsPage : Page
    {
        private List<dynamic> _allProducts;

        public ProductsPage()
        {
            InitializeComponent();
            LoadCategories();
            LoadManufacturers();
            LoadProducts();
        }

        private void LoadCategories()
        {
            try
            {
                var categories = Connect.Connection.entities.Products
                    .Select(p => p.Category)
                    .Where(c => c != null)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                categories.Insert(0, "Все категории");
                cmbCategory.ItemsSource = categories;
                cmbCategory.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}");
            }
        }

        private void LoadManufacturers()
        {
            try
            {
                var manufacturers = Connect.Connection.entities.Products
                    .Select(p => p.Manufacturer)
                    .Where(m => m != null)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList();

                manufacturers.Insert(0, "Все производители");
                cmbManufacturer.ItemsSource = manufacturers;
                cmbManufacturer.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки производителей: {ex.Message}");
            }
        }

        private void LoadProducts()
        {
            try
            {
                var products = Connect.Connection.entities.Products.ToList();
                var productsWithPrice = new List<dynamic>();

                foreach (var product in products)
                {
                    var lastDeliveryItem = (from di in Connect.Connection.entities.DeliveryItems
                                            join d in Connect.Connection.entities.Deliveries on di.DeliveryId equals d.DeliveryId
                                            where di.ProductId == product.ProductId
                                            orderby d.DeliveryDate descending
                                            select di).FirstOrDefault();

                    decimal price = lastDeliveryItem != null ? lastDeliveryItem.PurchasePrice * 1.3m : 0;

                    productsWithPrice.Add(new
                    {
                        product.ProductId,
                        product.SKU,
                        product.Name,
                        product.Category,
                        product.Manufacturer,
                        product.WarrantyMonths,
                        Price = price
                    });
                }

                _allProducts = productsWithPrice;
                dgProducts.ItemsSource = _allProducts;
                UpdateStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}");
            }
        }

        private void FilterProducts()
        {
            if (_allProducts == null) return;

            string searchText = txtSearch.Text.ToLower();
            string selectedCategory = cmbCategory.SelectedItem?.ToString();
            string selectedManufacturer = cmbManufacturer.SelectedItem?.ToString();

            var filtered = _allProducts.Where(p =>
                (string.IsNullOrEmpty(searchText) ||
                 p.Name.ToLower().Contains(searchText) ||
                 p.SKU.ToLower().Contains(searchText) ||
                 p.Manufacturer.ToLower().Contains(searchText)) &&
                (selectedCategory == "Все категории" || p.Category == selectedCategory) &&
                (selectedManufacturer == "Все производители" || p.Manufacturer == selectedManufacturer));

            dgProducts.ItemsSource = filtered.ToList();
            UpdateStatus(filtered.Count());
        }

        private void UpdateStatus(int? count = null)
        {
            int total = count ?? (_allProducts?.Count ?? 0);
            int displayed = (dgProducts.ItemsSource as IEnumerable<dynamic>)?.Count() ?? 0;

            if (total == displayed)
                txtStatus.Text = $"Показано {total} товаров";
            else
                txtStatus.Text = $"Найдено {displayed} из {total} товаров";
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterProducts();
        }

        private void CmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterProducts();
        }

        private void CmbManufacturer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterProducts();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            cmbCategory.SelectedIndex = 0;
            cmbManufacturer.SelectedIndex = 0;
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int productId = (int)button.Tag;

            var product = _allProducts.FirstOrDefault(p => p.ProductId == productId);
            if (product != null)
            {
                CartPage.AddToCart(productId, product.Name, product.Price, product.SKU);
                MessageBox.Show($"✓ {product.Name} добавлен в корзину!",
                              "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}