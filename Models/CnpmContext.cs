using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace backend.Models;

public partial class CnpmContext : DbContext
{
    public CnpmContext()
    {
    }

    public CnpmContext(DbContextOptions<CnpmContext> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<Tourist_Area> TouristAreas { get; set; }
    public virtual DbSet<Tourist_Place> TouristPlaces { get; set; }
    public virtual DbSet<Hotel> Hotels { get; set; }
    public virtual DbSet<Tour> Tours { get; set; }
    public virtual DbSet<Tour_Itinerary> TourItineraries { get; set; }
    public virtual DbSet<Favorite> Favorites { get; set; }
    public virtual DbSet<Review> Reviews { get; set; }
    public virtual DbSet<Img> Imgs { get; set; }
    public virtual DbSet<Advertisement> Advertisements { get; set; }
    public virtual DbSet<Marker> Markers { get; set; }
    public virtual DbSet<Report> Reports { get; set; }
    public virtual DbSet<Notification> Notifications { get; set; }
    public virtual DbSet<Booking> Bookings { get; set; }
    public virtual DbSet<Booking_Detail> BookingDetails { get; set; }
    public virtual DbSet<Hotel_Room> HotelRooms { get; set; }
    public virtual DbSet<Tour_Departure> TourDepartures { get; set; }
    public virtual DbSet<Feedback> Feedbacks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=aws-1-ap-south-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.xdtzkeyxwcrhyqtbqifr;Password=r!*#&4U8_7#&drN;SSL Mode=Require;Trust Server Certificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        // ============= USER =============
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");

            entity.Property(e => e.Avt)
                .HasColumnName("avt");

            entity.Property(e => e.Status)
                .HasColumnName("Status")
                .HasDefaultValue("Active");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");

            entity.Property(e => e.ResetPasswordToken).HasColumnName("ResetPasswordToken");
            entity.Property(e => e.ResetPasswordExpiry)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ResetPasswordExpiry");

            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.User_Search_History).HasColumnName("User_Search_History");

            // index
            entity.HasIndex(e => e.Role)
                .HasDatabaseName("ix_users_role");
        });
        // ============= RefreshToken =============
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");

            entity.HasKey(e => e.Id)
                  .HasName("refresh_tokens_pkey");

            entity.Property(e => e.Id)
                  .HasColumnName("id");

            entity.Property(e => e.UserId)
                  .HasColumnName("user_id")
                  .IsRequired();

            entity.Property(e => e.TokenHash)
                  .HasColumnName("token_hash")
                  .IsRequired();

            entity.Property(e => e.ExpiresAt)
                  .HasColumnName("expires_at")
                  .HasColumnType("timestamp without time zone");

            entity.Property(e => e.IsRevoked)
                  .HasColumnName("is_revoked");

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("now()")
                  .HasColumnType("timestamp without time zone")
                  .HasColumnName("created_at");

            entity.HasOne(e => e.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("fk_refresh_tokens_users");

            // index 
            entity.HasIndex(e => e.TokenHash)
                  .HasDatabaseName("ix_refresh_tokens_token_hash");
            entity.HasIndex(e => e.UserId)
                  .HasDatabaseName("ix_refresh_tokens_user_id");
            entity.HasIndex(e => e.ExpiresAt)
                  .HasDatabaseName("ix_refresh_tokens_expires_at");
        });
        // ============= Hotel =============
        modelBuilder.Entity<Hotel>(entity =>
        {
            entity.ToTable("hottels");

            entity.HasKey(e => e.Id)
                  .HasName("hottels_pkey");

            entity.Property(e => e.Id)
                  .HasColumnName("id");

            entity.Property(e => e.Name)
                  .HasColumnName("name");

            entity.Property(e => e.Address)
                  .HasColumnName("address");

            entity.Property(e => e.Latitude)
                  .HasColumnName("latitude");

            entity.Property(e => e.Longitude)
                  .HasColumnName("longitude");

            entity.Property(e => e.Description)
                  .HasColumnName("description");

            entity.Property(e => e.Title)
                  .HasColumnName("title");

            entity.Property(e => e.Status)
                  .HasColumnName("status");

            entity.Property(e => e.NumberOfPeople)
                  .HasColumnName("number_of_people");

            entity.Property(e => e.Created_By_UserId)
                  .HasColumnName("created_by_user_id");

            entity.Property(e => e.Tourist_Place_Id)
                  .HasColumnName("tourist_place_id");

            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasColumnType("timestamp without time zone")
                  .HasDefaultValueSql("now()");

            entity.Property(e => e.RatingCount)
                  .HasColumnName("rating_count").HasDefaultValue(0);

            entity.Property(e => e.RatingAverage)
                  .HasColumnName("rating_average").HasDefaultValue(0);

            entity.Property(e => e.FavoriteCount)
                  .HasColumnName("favorite_count").HasDefaultValue(0);

            entity.Property(e => e.ClickCount)
                  .HasColumnName("click_count").HasDefaultValue(0);

            entity.Property(e => e.Price)
                  .HasColumnName("price");

            entity.Property(e => e.RatingTotal)
                  .HasColumnName("rating_total").HasDefaultValue(0);

            // FK User
            entity.HasOne(d => d.User)
                  .WithMany(u => u.Hottels)
                  .HasForeignKey(d => d.Created_By_UserId)
                  .HasConstraintName("fk_hotel_user");

            // FK Tourist Area
            entity.HasOne(d => d.Tourist_Place)
                  .WithMany(p => p.Hotels)
                  .HasForeignKey(d => d.Tourist_Place_Id)
                  .HasConstraintName("fk_hotel_tourist_place");

            //index
            entity.HasIndex(e => e.Tourist_Place_Id)
                  .HasDatabaseName("ix_hottels_tourist_place_id");

            entity.HasIndex(e => e.Created_By_UserId)
                  .HasDatabaseName("ix_hottels_created_by_user");

            entity.HasIndex(e => e.Name)
                  .HasDatabaseName("ix_hottels_name");

            entity.HasIndex(e => e.CreatedAt)
                  .HasDatabaseName("ix_hottels_created_at");

            entity.HasIndex(e => e.RatingAverage)
                  .HasDatabaseName("ix_hottels_rating");

            entity.HasIndex(e => e.ClickCount)
                  .HasDatabaseName("ix_hottels_click");

            entity.HasIndex(e => new { e.RatingAverage, e.ClickCount })
                  .HasDatabaseName("ix_hottels_popular");
        });
        // ============= Tourist_Area =============
        modelBuilder.Entity<Tourist_Area>(entity =>
        {
            entity.ToTable("tourist_areas");

            entity.HasKey(e => e.Id)
                  .HasName("tourist_areas_pkey");

            entity.Property(e => e.Id)
                  .HasColumnName("id");

            entity.Property(e => e.Name)
                  .HasColumnName("name");

            entity.Property(e => e.Address)
                  .HasColumnName("address");

            entity.Property(e => e.Latitude)
                  .HasColumnName("latitude");

            entity.Property(e => e.Longitude)
                  .HasColumnName("longitude");

            entity.Property(e => e.Description)
                  .HasColumnName("description");

            entity.Property(e => e.Title)
                  .HasColumnName("title");

            entity.Property(e => e.Status)
                   .HasColumnName("Status").HasDefaultValue("Available"); ;

            entity.Property(e => e.Created_By_UserId)
                  .HasColumnName("created_by_user_id");

            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasColumnType("timestamp without time zone")
                  .HasDefaultValueSql("now()");

            entity.Property(e => e.RatingCount)
                  .HasColumnName("rating_count").HasDefaultValue(0);

            entity.Property(e => e.RatingAverage)
                  .HasColumnName("rating_average").HasDefaultValue(0);

            entity.Property(e => e.FavoriteCount)
                  .HasColumnName("favorite_count").HasDefaultValue(0);

            entity.Property(e => e.ClickCount)
                  .HasColumnName("click_count").HasDefaultValue(0);

            entity.Property(e => e.RatingTotal)
                  .HasColumnName("rating_total").HasDefaultValue(0);

            // FK User (1 User tạo nhiều Tourist Area)
            entity.HasOne(d => d.User)
                  .WithMany(u => u.Tourist_Areas)
                  .HasForeignKey(d => d.Created_By_UserId)
                  .HasConstraintName("fk_tourist_area_user");

            // Quan hệ 1 TouristArea - N TouristPlaces
            entity.HasMany(d => d.Tourist_Places)
                  .WithOne(p => p.Tourist_Area)
                  .HasForeignKey(p => p.Tourist_Area_Id)
                  .HasConstraintName("fk_tourist_place_area");

            // Quan hệ 1 TouristArea - N Tour 
            entity.HasMany(d => d.Tours)
                  .WithOne(p => p.Tourist_Area)
                  .HasForeignKey(p => p.Tourist_Area_Id)
                  .HasConstraintName("fk_tour_tourist_area");

            //index 
            entity.HasIndex(e => e.Created_By_UserId)
                 .HasDatabaseName("ix_tourist_areas_created_by_user");

            entity.HasIndex(e => e.Name)
                  .HasDatabaseName("ix_tourist_areas_name");

            entity.HasIndex(e => e.CreatedAt)
                  .HasDatabaseName("ix_tourist_area_created_at");

            entity.HasIndex(e => e.RatingAverage)
                  .HasDatabaseName("ix_tourist_area_rating");

            entity.HasIndex(e => e.ClickCount)
                  .HasDatabaseName("ix_tourist_area_click");

            entity.HasIndex(e => new { e.RatingAverage, e.ClickCount })
                  .HasDatabaseName("ix_tourist_area_popular");
        });
        // ============= Tour =============
        modelBuilder.Entity<Tour>(entity =>
        {
            entity.ToTable("tours");

            entity.HasKey(e => e.Id).HasName("tour_id");

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.Name)
                .HasColumnName("name");

            entity.Property(e => e.Description)
                .HasColumnName("description");

            entity.Property(e => e.Title)
                .HasColumnName("title");

            entity.Property(e => e.DurationDays)
                .HasColumnName("duration_days");

            entity.Property(e => e.NumberOfPeople)
                .HasColumnName("number_of_people");

            entity.Property(e => e.DepartureLocationName)
               .HasColumnName("DepartureLocationName");

            entity.Property(e => e.DepartureLatitude)
                  .HasColumnName("DepartureLatitude");

            entity.Property(e => e.DepartureLongitude)
                  .HasColumnName("DepartureLongitude");

            entity.Property(e => e.Vehicle)
               .HasColumnName("Vehicle");

            entity.Property(e => e.TourType)
               .HasColumnName("TourType");

            entity.Property(e => e.Status)
               .HasColumnName("Status").HasDefaultValue("Available");

            entity.Property(e => e.Price)
                .HasColumnName("price");

            entity.Property(e => e.Created_By_UserId)
                .HasColumnName("created_by_user_id");

            entity.Property(e => e.Tourist_Area_Id)
                .HasColumnName("tourist_area_id");

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("now()")
                  .HasColumnType("timestamp without time zone")
                  .HasColumnName("created_at");

            entity.Property(e => e.RatingCount)
                .HasColumnName("rating_count").HasDefaultValue(0);

            entity.Property(e => e.RatingAverage)
                .HasColumnName("rating_average").HasDefaultValue(0);

            entity.Property(e => e.FavoriteCount)
                .HasColumnName("favorite_count").HasDefaultValue(0);

            entity.Property(e => e.ClickCount)
                .HasColumnName("click_count").HasDefaultValue(0);

            entity.Property(e => e.RatingTotal)
                .HasColumnName("rating_total").HasDefaultValue(0);

            // FK User
            entity.HasOne(d => d.User)
                .WithMany(u => u.Tours)
                .HasForeignKey(d => d.Created_By_UserId)
                .HasConstraintName("fk_tour_user");

            // FK Tourist Area
            entity.HasOne(d => d.Tourist_Area)
                .WithMany(p => p.Tours)
                .HasForeignKey(d => d.Tourist_Area_Id)
                .HasConstraintName("fk_tour_tourist_area");

            //index 
            entity.HasIndex(e => e.Tourist_Area_Id)
                  .HasDatabaseName("ix_tours_tourist_area_id");

            entity.HasIndex(e => e.Created_By_UserId)
                  .HasDatabaseName("ix_tours_created_by_user");

            entity.HasIndex(e => e.Name)
                  .HasDatabaseName("ix_tours_name");

            entity.HasIndex(e => e.CreatedAt)
                  .HasDatabaseName("ix_tours_created_at");

            entity.HasIndex(e => e.RatingAverage)
                  .HasDatabaseName("ix_tours_rating");

            entity.HasIndex(e => e.ClickCount)
                  .HasDatabaseName("ix_tours_click");

            entity.HasIndex(e => new { e.RatingAverage, e.ClickCount })
                  .HasDatabaseName("ix_tours_popular");
        });
        // ============= Favorite =============
        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.ToTable("favorites");

            entity.HasKey(e => e.Id)
                  .HasName("favorites_pkey");

            entity.Property(e => e.Id)
                  .HasColumnName("id");

            entity.Property(e => e.UserId)
                  .HasColumnName("user_id");

            entity.Property(e => e.EntityType)
                  .HasColumnName("entity_type");

            entity.Property(e => e.EntityId)
                  .HasColumnName("entity_id");

            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasColumnType("timestamp without time zone")
                  .HasDefaultValueSql("now()");


            // FK User
            entity.HasOne(d => d.user)
                  .WithMany(p => p.Favorites)
                  .HasForeignKey(d => d.UserId)
                  .HasConstraintName("fk_favorite_user");

            //index
            entity.HasIndex(e => e.UserId)
                  .HasDatabaseName("ix_favorites_user_id");

            entity.HasIndex(e => new { e.EntityType, e.EntityId })
                  .HasDatabaseName("ix_favorites_entity");

            entity.HasIndex(e => new { e.UserId, e.EntityType, e.EntityId })
                  .HasDatabaseName("ix_favorites_user_entity");
        });
        // ============= Img =============
        modelBuilder.Entity<Img>(entity =>
        {
            entity.ToTable("imgs");

            entity.HasKey(e => e.Id)
                  .HasName("imgs_pkey");

            entity.Property(e => e.Id)
                  .HasColumnName("id");

            entity.Property(e => e.url)
                  .HasColumnName("url");

            entity.Property(e => e.IsCover)
                  .HasColumnName("is_cover");

            entity.Property(e => e.EntityType)
                  .HasColumnName("entity_type");

            entity.Property(e => e.EntityId)
                  .HasColumnName("entity_id");

            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasColumnType("timestamp without time zone")
                  .HasDefaultValueSql("now()");

            // index 
            entity.HasIndex(e => new { e.EntityType, e.EntityId })
                  .HasDatabaseName("ix_imgs_entity");
            entity.HasIndex(e => e.IsCover)
                  .HasDatabaseName("ix_imgs_cover");
        });
        // ============= Review =============
        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("reviews");

            entity.HasKey(e => e.Id)
                  .HasName("reviews_pkey");

            entity.Property(e => e.Id)
                  .HasColumnName("id");

            entity.Property(e => e.UserId)
                  .HasColumnName("user_id");

            entity.Property(e => e.EntityType)
                  .HasColumnName("entity_type");

            entity.Property(e => e.EntityId)
                  .HasColumnName("entity_id");

            entity.Property(e => e.Comment)
                  .HasColumnName("comment");

            entity.Property(e => e.Score)
                  .HasColumnName("score");

            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasColumnType("timestamp without time zone")
                  .HasDefaultValueSql("now()");

            // FK User
            entity.HasOne(d => d.user)
                  .WithMany(p => p.Reviews)
                  .HasForeignKey(d => d.UserId)
                  .HasConstraintName("fk_review_user");

            //index
            entity.HasIndex(e => e.UserId)
                  .HasDatabaseName("ix_reviews_user_id");

            entity.HasIndex(e => new { e.EntityType, e.EntityId })
                  .HasDatabaseName("ix_reviews_entity");

            entity.HasIndex(e => e.Score)
                  .HasDatabaseName("ix_reviews_score");
        });
        // ============= Tourist_Place =============
        modelBuilder.Entity<Tourist_Place>(entity =>
        {
            entity.ToTable("tourist_places");

            entity.HasKey(e => e.Id)
                  .HasName("tourist_places_pkey");

            entity.Property(e => e.Id)
                  .HasColumnName("id");

            entity.Property(e => e.Name)
                  .HasColumnName("name");

            entity.Property(e => e.Address)
                  .HasColumnName("address");

            entity.Property(e => e.Latitude)
                  .HasColumnName("latitude");

            entity.Property(e => e.Longitude)
                  .HasColumnName("longitude");

            entity.Property(e => e.Description)
                  .HasColumnName("description");

            entity.Property(e => e.Title)
                  .HasColumnName("title");

            entity.Property(e => e.Status).HasColumnName("status")
                   .HasDefaultValue("Available");

            entity.Property(e => e.Created_By_UserId)
                  .HasColumnName("created_by_user_id");

            entity.Property(e => e.Tourist_Area_Id)
                  .HasColumnName("tourist_area_id");

            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasColumnType("timestamp without time zone")
                  .HasDefaultValueSql("now()");

            entity.Property(e => e.RatingTotal)
                   .HasColumnName("rating_total").HasDefaultValue(0);

            entity.Property(e => e.RatingCount)
                  .HasColumnName("rating_count").HasDefaultValue(0);

            entity.Property(e => e.RatingAverage)
                  .HasColumnName("rating_average").HasDefaultValue(0);

            entity.Property(e => e.FavoriteCount)
                  .HasColumnName("favorite_count").HasDefaultValue(0);

            entity.Property(e => e.ClickCount)
                  .HasColumnName("click_count").HasDefaultValue(0);

            // ===== FK USER =====
            entity.HasOne(d => d.User)
                  .WithMany(u => u.Tourist_Place)
                  .HasForeignKey(d => d.Created_By_UserId)
                  .HasConstraintName("fk_tourist_place_user");

            // ===== FK TOURIST AREA =====
            entity.HasOne(d => d.Tourist_Area)
                  .WithMany(p => p.Tourist_Places)
                  .HasForeignKey(d => d.Tourist_Area_Id)
                  .HasConstraintName("fk_tourist_place_area");

            // ===== INDEX =====
            entity.HasIndex(e => e.Tourist_Area_Id)
                  .HasDatabaseName("ix_tourist_places_area");

            entity.HasIndex(e => e.Created_By_UserId)
                  .HasDatabaseName("ix_tourist_places_user");

            entity.HasIndex(e => e.Name)
                  .HasDatabaseName("ix_tourist_places_name");

            entity.HasIndex(e => e.RatingAverage)
                  .HasDatabaseName("ix_tourist_places_rating");

            entity.HasIndex(e => new { e.RatingAverage, e.ClickCount })
                  .HasDatabaseName("ix_tourist_places_popular");
        });
        // ============= Tour_Itinerary =============
        modelBuilder.Entity<Tour_Itinerary>(entity =>
        {
            entity.ToTable("tour_itineraries");

            entity.HasKey(e => e.Id)
                  .HasName("tour_itineraries_pkey");

            entity.Property(e => e.Id)
                  .HasColumnName("id");

            entity.Property(e => e.Title)
                  .HasColumnName("title");

            entity.Property(e => e.Description)
                  .HasColumnName("description");

            entity.Property(e => e.TourId)
                  .HasColumnName("tour_id");

            entity.Property(e => e.Tourist_Place_Id)
                  .HasColumnName("tourist_place_id");

            entity.Property(e => e.DayNumber)
                  .HasColumnName("day_number");

            entity.HasOne(d => d.Tour)
                  .WithMany(p => p.Tour_Itinerarys)
                  .HasForeignKey(d => d.TourId)
                  .HasConstraintName("fk_itinerary_tour");

            entity.HasOne(d => d.Tourist_Place)
                  .WithMany(p => p.Tour_Itineraries)
                  .HasForeignKey(d => d.Tourist_Place_Id)
                  .HasConstraintName("fk_itinerary_place");

            entity.HasIndex(e => e.TourId)
                  .HasDatabaseName("ix_itinerary_tour");

            entity.HasIndex(e => e.Tourist_Place_Id)
                  .HasDatabaseName("ix_itinerary_place");
        });
        modelBuilder.Entity<Advertisement>(entity =>
        {
            entity.ToTable("advertisements");

            entity.HasKey(e => e.Id)
                  .HasName("advertisements_pkey");

            entity.Property(e => e.Id)
                 .HasColumnName("id");

            entity.Property(e => e.Title)
                 .HasColumnName("title");

            entity.Property(e => e.Description)
                  .HasColumnName("description");

            entity.Property(e => e.Position)
                 .HasColumnName("position");

            entity.Property(e => e.Size)
                 .HasColumnName("size");

            entity.Property(e => e.Url)
                  .HasColumnName("url");

            entity.Property(e => e.IsActive)
                  .HasColumnName("is_active")
                  .HasDefaultValue(true);

            entity.Property(e => e.Start_date)
                  .HasColumnName("start_date")
                  .HasColumnType("timestamp without time zone")
                  .HasDefaultValueSql("now()");

            entity.Property(e => e.End_date)
                  .HasColumnName("end_date")
                  .HasColumnType("timestamp without time zone")
                  .HasDefaultValueSql("now() + interval '31 days'");

            entity.Property(e => e.Name)
                 .HasColumnName("name");

            entity.Property(e => e.Phone)
                 .HasColumnName("phone");

            entity.HasIndex(e => e.Position)
                  .HasDatabaseName("ix_advertisements_position");

            entity.HasIndex(e => new { e.IsActive, e.Start_date, e.End_date })
                  .HasDatabaseName("ix_advertisements_active_date");
        });

        modelBuilder.Entity<Marker>(entity =>
        {
            entity.ToTable("markers");

            entity.HasKey(e => e.Id)
                  .HasName("markers_pkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Latitude).HasColumnName("latitude");
            entity.Property(e => e.Longitude).HasColumnName("longitude");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Description).HasColumnName("description");

            entity.Property(e => e.IsPublic)
                  .HasColumnName("is_public")
                  .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasColumnType("timestamp without time zone")
                  .HasDefaultValueSql("now()");

            entity.Property(e => e.CreatedByUserId)
                  .HasColumnName("created_by_user_id");

            entity.Property(e => e.TouristPlaceId)
                  .HasColumnName("tourist_place_id");

            entity.HasOne(d => d.User)
                  .WithMany(u => u.Markers)
                  .HasForeignKey(d => d.CreatedByUserId)
                  .HasConstraintName("fk_marker_user");

            entity.HasIndex(e => e.CreatedByUserId)
                  .HasDatabaseName("ix_markers_user");

            entity.HasOne(d => d.Tourist_Place)
                  .WithMany()
                  .HasForeignKey(d => d.TouristPlaceId)
                  .HasConstraintName("FK_Markers_TouristPlace");

            entity.HasIndex(e => e.TouristPlaceId)
                  .HasDatabaseName("ix_markers_touristPlace");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.ToTable("reports");

            entity.HasKey(e => e.Id)
                  .HasName("reports_pkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EntityType).HasColumnName("entity_type");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Description).HasColumnName("description");

            entity.Property(e => e.Status)
                  .HasColumnName("status")
                  .HasDefaultValue("Pending");

            entity.Property(e => e.ReportedByUserId)
                  .HasColumnName("reported_by_user_id");

            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasColumnType("timestamp without time zone")
                  .HasDefaultValueSql("now()");

            entity.HasOne(d => d.User)
                  .WithMany(u => u.Reports)
                  .HasForeignKey(d => d.ReportedByUserId)
                  .HasConstraintName("fk_report_user");

            entity.HasIndex(e => new { e.EntityType, e.EntityId })
                  .HasDatabaseName("ix_reports_entity");
            entity.HasIndex(e => e.ReportedByUserId)
                  .HasDatabaseName("ix_reports_user");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");

            entity.HasKey(e => e.Id)
                  .HasName("notifications_pkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.Property(e => e.IsRead)
                  .HasColumnName("is_read")
                  .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasColumnType("timestamp without time zone")
                  .HasDefaultValueSql("now()");

            entity.HasOne(d => d.User)
                  .WithMany(u => u.Notifications)
                  .HasForeignKey(d => d.UserId)
                  .HasConstraintName("fk_notification_user");

            entity.HasIndex(e => e.UserId)
                  .HasDatabaseName("ix_notifications_user");
        });
        // ============= Hotel_Room =============
        modelBuilder.Entity<Hotel_Room>(entity =>
        {
            entity.ToTable("hotel_rooms");
            entity.HasKey(e => e.Id).HasName("hotel_rooms_pkey");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.HotelId).HasColumnName("hotel_id");
            entity.Property(e => e.RoomName).HasColumnName("room_name").IsRequired();
            entity.Property(e => e.Floor).HasColumnName("floor");
            entity.Property(e => e.RoomType).HasColumnName("room_type").HasDefaultValue("Standard");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Status).HasColumnName("status").HasDefaultValue("Available");

            // FK
            entity.HasOne(d => d.Hotel)
                  .WithMany(p => p.Rooms)
                  .HasForeignKey(d => d.HotelId)
                  .OnDelete(DeleteBehavior.Cascade) // Xóa Hotel thì xóa luôn Room
                  .HasConstraintName("fk_hotel_room");

            // Index
            entity.HasIndex(e => e.HotelId).HasDatabaseName("ix_hotel_rooms_hotel_id");
            entity.HasIndex(e => e.Status).HasDatabaseName("ix_hotel_rooms_status");
        });

        // ============= Tour_Departure =============
        modelBuilder.Entity<Tour_Departure>(entity =>
        {
            entity.ToTable("tour_departures");
            entity.HasKey(e => e.Id).HasName("tour_departures_pkey");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TourId).HasColumnName("tour_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date").HasColumnType("timestamp without time zone");
            entity.Property(e => e.TotalSeats).HasColumnName("total_seats");
            entity.Property(e => e.AvailableSeats).HasColumnName("available_seats");
            entity.Property(e => e.BookedSeats).HasColumnName("booked_seats").HasColumnType("jsonb"); // Dùng jsonb của PostgreSQL cho lẹ
            entity.Property(e => e.Status).HasColumnName("status").HasDefaultValue("Open");

            // FK
            entity.HasOne(d => d.Tour)
                  .WithMany(p => p.Departures)
                  .HasForeignKey(d => d.TourId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("fk_tour_departure");

            // Index
            entity.HasIndex(e => e.TourId).HasDatabaseName("ix_tour_departures_tour_id");
            entity.HasIndex(e => e.StartDate).HasDatabaseName("ix_tour_departures_start_date");
        });

        // ============= Booking =============
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable("bookings");
            entity.HasKey(e => e.Id).HasName("bookings_pkey");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.BookingType).HasColumnName("booking_type").IsRequired();
            entity.Property(e => e.ContactName).HasColumnName("contact_name").IsRequired();
            entity.Property(e => e.ContactPhone).HasColumnName("contact_phone").IsRequired();
            entity.Property(e => e.ContactAddress).HasColumnName("contact_address");

            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.TotalAmount).HasColumnName("total_amount");
            entity.Property(e => e.PaymentStatus).HasColumnName("payment_status").HasDefaultValue("Unpaid");
            entity.Property(e => e.BookingStatus).HasColumnName("booking_status").HasDefaultValue("Pending");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp without time zone").HasDefaultValueSql("now()");

            // FK
            entity.HasOne(d => d.User)
                  .WithMany() // Không cần khai báo ngược lại trong User Model cho đỡ rối
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.Restrict) // Không cho phép xóa User nếu họ đã có Booking
                  .HasConstraintName("fk_booking_user");

            // Index
            entity.HasIndex(e => e.UserId).HasDatabaseName("ix_bookings_user_id");
            entity.HasIndex(e => e.BookingType).HasDatabaseName("ix_bookings_type");
            entity.HasIndex(e => e.BookingStatus).HasDatabaseName("ix_bookings_status");
        });

        // ============= Booking_Detail =============
        modelBuilder.Entity<Booking_Detail>(entity =>
        {
            entity.ToTable("booking_details");
            entity.HasKey(e => e.Id).HasName("booking_details_pkey");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.HotelRoomId).HasColumnName("hotel_room_id");
            entity.Property(e => e.TourDepartureId).HasColumnName("tour_departure_id");
            entity.Property(e => e.SeatNumber).HasColumnName("seat_number");
            entity.Property(e => e.IsPrivateTour).HasColumnName("is_private_tour").HasDefaultValue(false);
            entity.Property(e => e.UnitPrice).HasColumnName("unit_price");

            // FK
            entity.HasOne(d => d.Booking)
                  .WithMany(p => p.BookingDetails)
                  .HasForeignKey(d => d.BookingId)
                  .OnDelete(DeleteBehavior.Cascade) // Xóa Booking thì xóa luôn Detail
                  .HasConstraintName("fk_booking_detail_booking");

            entity.HasOne(d => d.HotelRoom)
                  .WithMany() // Không khai báo ngược lại bên bảng Hotel_Room cho đỡ rối
                  .HasForeignKey(d => d.HotelRoomId)
                  .OnDelete(DeleteBehavior.SetNull) // Nếu Khách sạn xóa phòng, detail vẫn còn nhưng gán ID phòng = Null (bảo vệ hóa đơn)
                  .HasConstraintName("fk_booking_detail_hotel_room");

            entity.HasOne(d => d.TourDeparture)
                  .WithMany()
                  .HasForeignKey(d => d.TourDepartureId)
                  .OnDelete(DeleteBehavior.SetNull)
                  .HasConstraintName("fk_booking_detail_tour_departure");
            // Index
            entity.HasIndex(e => e.BookingId).HasDatabaseName("ix_booking_details_booking_id");
            entity.HasIndex(e => e.HotelRoomId).HasDatabaseName("ix_booking_details_hotel_room_id");
            entity.HasIndex(e => e.TourDepartureId).HasDatabaseName("ix_booking_details_tour_departure_id");
        });
        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.ToTable("feedbacks");
            entity.HasKey(e => e.Id).HasName("feedbacks_pkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Subject).HasColumnName("subject");
            entity.Property(e => e.Message).HasColumnName("message");

            entity.Property(e => e.Status)
                  .HasColumnName("status")
                  .HasDefaultValue("New"); // Mặc định là mới gửi

            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasColumnType("timestamp without time zone")
                  .HasDefaultValueSql("now()");

            entity.HasOne(d => d.User)
                  .WithMany(u => u.Feedbacks)
                  .HasForeignKey(d => d.UserId)
                  .HasConstraintName("fk_feedback_user"); // Ràng buộc khóa ngoại
        });

        OnModelCreatingPartial(modelBuilder);
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
