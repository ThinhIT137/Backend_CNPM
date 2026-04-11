using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Hotel_Room
    {
        [Key]
        public int Id { get; set; }
        public int HotelId { get; set; }
        public string RoomName { get; set; } = null!; // VD: "P101", "P205"
        public int Floor { get; set; }                // Tầng (VD: 1, 2, 3)
        public string RoomType { get; set; } = "Standard"; // Standard, VIP, Twin...
        public decimal Price { get; set; }            // Giá phòng/đêm
        public string Status { get; set; } = "Available"; // Available, Booked, Maintenance

        [ForeignKey("HotelId")]
        public virtual Hotel? Hotel { get; set; }
    }
}
