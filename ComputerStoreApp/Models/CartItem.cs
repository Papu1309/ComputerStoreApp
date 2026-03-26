namespace ComputerStoreApp.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string SKU { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total => Quantity * Price;

        public override bool Equals(object obj)
        {
            if (obj is CartItem other)
                return ProductId == other.ProductId;
            return false;
        }

        public override int GetHashCode()
        {
            return ProductId.GetHashCode();
        }
    }
}