using Microsoft.EntityFrameworkCore;
using MovieApi.Models;

namespace MovieApi;

public interface IMovieApiContext
{
    DbSet<Movie> Movie { get; set; }
    DbSet<Genre> Genres { get; set; }
    DbSet<MovieDetails> MovieDetails { get; set; }
    DbSet<Review> Reviews { get; set; }
    DbSet<Actor> Actors { get; set; }
    DbSet<MovieActor> MovieActors { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}
