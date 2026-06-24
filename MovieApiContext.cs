using Microsoft.EntityFrameworkCore;
using MovieApi.Models;

public class MovieApiContext(DbContextOptions<MovieApiContext> options) : DbContext(options)
{
    public DbSet<MovieApi.Models.Movie> Movie { get; set; } = default!;

    public DbSet<Genre> Genres { get; set; } = default!;
    public DbSet<MovieDetails> MovieDetails { get; set; } = default!;
    public DbSet<Review> Reviews { get; set; } = default!;
    public DbSet<Actor> Actors { get; set; } = default!;
    public DbSet<MovieActor> MovieActors { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<MovieActor>()
            .HasKey(ma => new { ma.MovieId, ma.ActorId });

        modelBuilder.Entity<MovieActor>()
            .HasOne(ma => ma.Movie)
            .WithMany(m => m.MovieActors)
            .HasForeignKey(ma => ma.MovieId);

        modelBuilder.Entity<MovieActor>()
            .HasOne(ma => ma.Actor)
            .WithMany(a => a.MovieActors)
            .HasForeignKey(ma => ma.ActorId);
    }
}
