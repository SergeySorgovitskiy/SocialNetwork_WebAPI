using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Repost> Reposts { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Like>(entity =>
            {
                entity.HasKey(l => l.Id);

                entity.HasIndex(l => new { l.UserId, l.PostId }).IsUnique();

                entity.HasOne(l => l.User)
                    .WithMany(u => u.Likes)
                    .HasForeignKey(l => l.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.Post)
                    .WithMany(p => p.Likes)
                    .HasForeignKey(l => l.PostId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(l => l.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<Repost>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.HasIndex(r => new { r.UserId, r.OriginalPostId }).IsUnique();

                entity.HasOne(r => r.User)
                    .WithMany(u => u.Reposts)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.OriginalPost)
                    .WithMany(p => p.Reposts)
                    .HasForeignKey(r => r.OriginalPostId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(r => r.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<Bookmark>(entity =>
            {
                entity.HasKey(b => b.Id);

                entity.HasIndex(b => new { b.UserId, b.PostId }).IsUnique();

                entity.HasOne(b => b.User)
                    .WithMany(u => u.Bookmarks)
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Post)
                    .WithMany(p => p.Bookmarks)
                    .HasForeignKey(b => b.PostId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(b => b.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<User>(u =>
            {
                u.HasIndex(x => x.Email).IsUnique();
                u.HasIndex(x => x.UserName).IsUnique();
            });

            modelBuilder.Entity<Post>(p =>
            {
                p.HasOne(x => x.Author)
                    .WithMany(x => x.Posts)
                    .HasForeignKey(x => x.AuthorId)
                    .OnDelete(DeleteBehavior.Cascade);

                p.Property(x => x.MediaUrls)
                    .HasConversion(
                        v => string.Join(';', v),
                        v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToHashSet());

                p.Property(x => x.Hashtags)
                    .HasConversion(
                        v => string.Join(';', v),
                        v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToHashSet());
            });

            modelBuilder.Entity<Comment>(c =>
            {
                c.HasOne(x => x.Post)
                    .WithMany(x => x.Comments)
                    .HasForeignKey(x => x.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                c.HasOne(x => x.Author)
                    .WithMany(x => x.Comments)
                    .HasForeignKey(x => x.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

                c.HasOne(x => x.ParentComment)
                    .WithMany(x => x.Replies)
                    .HasForeignKey(x => x.ParentCommentId)
                    .OnDelete(DeleteBehavior.Restrict);

                c.Property(x => x.NestingLevel)
                    .HasDefaultValue(0);
            });

            modelBuilder.Entity<Subscription>(s =>
            {
                s.HasKey(x => x.Id);

                s.HasOne(x => x.Follower)
         .WithMany(x => x.Following)
         .HasForeignKey(x => x.FollowerId)
         .OnDelete(DeleteBehavior.Cascade);

                s.HasOne(x => x.Following)
                    .WithMany(x => x.Followers)
                    .HasForeignKey(x => x.FollowingId)
                    .OnDelete(DeleteBehavior.Restrict);

                s.HasIndex(x => new { x.FollowerId, x.FollowingId }).IsUnique();
            });
        }
    }
}

