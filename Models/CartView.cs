namespace CloudSaba.Models
{
    public class CartView
    {
        public CartItem CartItem { get; set; }
        public IceCream IceCream { get; set; }

        //public decimal Total()
        //{
        //    decimal total = 0;
        //    foreach (var item in CartItems) { total += item.Price; }
        //    return total;
        //}
    }
}
