using System.ComponentModel.DataAnnotations;
namespace CloudSaba.Models
{
    public class CartItem
    {
        [Key]
        public string? ItemId { get; set; }

        public string? CartId { get; set; }

        public double Weight { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public System.DateTime DateCreated { get; set; }

        public string? OrderId { get; set; }
    }
}
