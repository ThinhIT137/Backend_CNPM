using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Tour
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Title { get; set; }
        public int? DurationDays { get; set; }
        public int? NumberOfPeople { get; set; }
        public string? DepartureLocationName { get; set; } // VD: "Hà Nội", "Sân bay Tân Sơn Nhất", "Bến xe Miền Đông"
        [Column(TypeName = "decimal(18, 10)")]
        public decimal? DepartureLatitude { get; set; }

        [Column(TypeName = "decimal(18, 10)")]
        public decimal? DepartureLongitude { get; set; }
        public string? Vehicle { get; set; }           // Phương tiện (VD: "Ô tô, Máy bay", "Tàu hỏa")
        public string? TourType { get; set; }          // Loại tour (VD: "Ghép đoàn", "Riêng tư")
        public string? Status { get; set; }
        public Guid? Created_By_UserId { get; set; }
        public int Tourist_Area_Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int RatingTotal { get; set; } = 0;
        public int RatingCount { get; set; } = 0;
        public decimal RatingAverage { get; set; } = 0;
        public int FavoriteCount { get; set; } = 0;
        public int ClickCount { get; set; } = 0;
        public decimal? Price { get; set; }


        // FK
        [ForeignKey("Created_By_UserId")]
        public virtual User? User { get; set; }
        [ForeignKey("Tourist_Area_Id")]
        public virtual Tourist_Area? Tourist_Area { get; set; }

        // điều hướng 1-N
        public virtual ICollection<Tour_Itinerary> Tour_Itinerarys { get; set; } = new List<Tour_Itinerary>();

    }
}
