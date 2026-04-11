using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public partial class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Name { get; set; }
    public string? Avt { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? Role { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime? CreatedAt { get; set; }
    public string? ResetPasswordToken { get; set; } // Lưu mã 5 số
    public DateTime? ResetPasswordExpiry { get; set; }
    public string? User_Search_History { get; set; }

    // Điều hướng 1 - N
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    [InverseProperty("User")]
    public virtual ICollection<Tourist_Area> Tourist_Areas { get; set; } = new List<Tourist_Area>();
    [InverseProperty("User")]
    public virtual ICollection<Hotel> Hottels { get; set; } = new List<Hotel>();
    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
    public virtual ICollection<Tourist_Place> Tourist_Place { get; set; } = new List<Tourist_Place>();
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
    public virtual ICollection<Marker> Markers { get; set; } = new List<Marker>();
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();


}
