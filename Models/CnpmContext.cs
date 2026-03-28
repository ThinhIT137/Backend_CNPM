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
    public virtual DbSet<Hottel> Hottels { get; set; }
    public virtual DbSet<Tour> Tours { get; set; }
    public virtual DbSet<Tour_Itinerary> TourItineraries { get; set; }
    public virtual DbSet<Favorite> Favorites { get; set; }
    public virtual DbSet<Review> Reviews { get; set; }
    public virtual DbSet<Img> Imgs { get; set; }


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
            entity.Property(e => e.Avt).HasColumnName("avt");
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
        // ============= Hottel =============
        modelBuilder.Entity<Hottel>(entity =>
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

            entity.Property(e => e.RatingTotal)
                  .HasColumnName("rating_total").HasDefaultValue(0);

            // FK User
            entity.HasOne(d => d.User)
                  .WithMany(u => u.Hottels)
                  .HasForeignKey(d => d.Created_By_UserId)
                  .HasConstraintName("fk_hotel_user");

            // FK Tourist Area
            entity.HasOne(d => d.Tourist_Place)
                  .WithMany(p => p.Hottels)
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

            // ===== FK TOUR =====
            entity.HasOne(d => d.Tour)
                  .WithMany(p => p.Tour_Itinerarys)
                  .HasForeignKey(d => d.TourId)
                  .HasConstraintName("fk_itinerary_tour");

            // ===== FK TOURIST PLACE =====
            entity.HasOne(d => d.Tourist_Place)
                  .WithMany(p => p.Tour_Itineraries)
                  .HasForeignKey(d => d.Tourist_Place_Id)
                  .HasConstraintName("fk_itinerary_place");

            // ===== INDEX =====
            entity.HasIndex(e => e.TourId)
                  .HasDatabaseName("ix_itinerary_tour");

            entity.HasIndex(e => e.Tourist_Place_Id)
                  .HasDatabaseName("ix_itinerary_place");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
