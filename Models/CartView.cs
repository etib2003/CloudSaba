namespace CloudSaba.Models
{
    public class CartView
    {
        public List<CartItem> CartItems { get; set; }
        //public List<Flavour> Flavours { get; set; }

        public decimal Total()
        {
            decimal total = 0;
            foreach (var item in CartItems) { total += item.Price; }
            return total;
        }
    }
}
