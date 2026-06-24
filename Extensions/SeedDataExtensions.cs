using MovieApi.Models;

namespace MovieApi.Extensions
{
    public static class SeedDataExtensions
    {
        public static void SeedData(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MovieApiContext>();

            context.Database.EnsureCreated();
            if (context.Movie.Any())
            {
                return;
            }

            var scifi = new Genre { Name = "Sci-Fi" };
            var action = new Genre { Name = "Action" };
            var romance = new Genre { Name = "Romance" };
            context.Genres.AddRange(scifi, action, romance);

            var actor1 = new Actor { Name = "Keanu Reeves" };
            var actor2 = new Actor { Name = "Carrie-Anne Moss" };
            var actor3 = new Actor { Name = "Chuck Norris" };
            context.Actors.AddRange(actor1, actor2, actor3);

            var movie1 = new Movie
            {
                Title = "The Matrix",
                Year = 1999,
                Duration = 136,
                Genre = scifi,
                Details = new MovieDetails
                {
                    Synopsis = "A computer hacker learns from mysterious rebels about the true nature of his reality.",
                    Director = "Lana Wachowski"
                }
            };

            var movie2 = new Movie
            {
                Title = "The Matrix Resurrections",
                Year = 2021,
                Duration = 148,
                Genre = scifi,
                Details = new MovieDetails
                {
                    Synopsis = "Return to a world of two realities: one, everyday life; the other, what lies behind it.",
                    Director = "Lana Wachowski"
                }
            };

            var movie3 = new Movie
            {
                Title = "Code of Silence",
                Year = 1985,
                Duration = 100,
                Genre = action,
                Details = new MovieDetails
                {
                    Synopsis = "A Chicago cop is caught in the middle of a gang war while his own comrades shun him because he wants to take down an irresponsible cop.",
                    Director = "Andrew Davis"
                }
            };
            context.Movie.AddRange(movie1, movie2, movie3);

            var review1 = new Review { Movie = movie1, ReviewerName = "Niklas", Comment = "Masterpiece!", Rating = 5 };
            var review2 = new Review { Movie = movie1, ReviewerName = "Anna", Comment = "Mind-bending story.", Rating = 4 };
            var review3 = new Review { Movie = movie2, ReviewerName = "John", Comment = "Not as good as the original.", Rating = 2 };
            var review4 = new Review { Movie = movie3, ReviewerName = "Karl", Comment = "It's Chuck Norris", Rating = 3 };
            context.Reviews.AddRange(review1, review2, review3, review4);

            context.MovieActors.AddRange(
                new MovieActor { Movie = movie1, Actor = actor1, Role = "Neo" },
                new MovieActor { Movie = movie1, Actor = actor2, Role = "Trinity" },
                new MovieActor { Movie = movie2, Actor = actor1, Role = "Neo" },
                new MovieActor { Movie = movie2, Actor = actor2, Role = "Trinity" },
                new MovieActor { Movie = movie3, Actor = actor3, Role = "Eddie Cusack" }
            );

            context.SaveChanges();
        }
    }
}
