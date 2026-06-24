using AutoMapper;
using MovieApi.DTOs.Actor;
using MovieApi.DTOs.Detail;
using MovieApi.DTOs.Movie;
using MovieApi.DTOs.MovieDetail;
using MovieApi.DTOs.Review;
using MovieApi.Models;

namespace MovieApi.Profiles
{
    public class MovieProfile : Profile
    {
        public MovieProfile()
        {
            CreateMap<Review, ReviewDto>();

            CreateMap<ReviewCreateDto, Review>();
            CreateMap<ReviewUpdateDto, Review>();

            CreateMap<MovieDetails, DetailDto>();

            CreateMap<MovieActor, ActorDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Actor != null ? src.Actor.Id : 0))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Actor != null ? src.Actor.Name : string.Empty));

            CreateMap<Movie, MovieDetailDto>()
                .ForMember(dest => dest.GenreName, opt => opt.MapFrom(src => src.Genre != null ? src.Genre.Name : string.Empty))
                .ForMember(dest => dest.Detail, opt => opt.MapFrom(src => src.Details))
                .ForMember(dest => dest.Actors, opt => opt.MapFrom(src => src.MovieActors));

            CreateMap<Movie, MovieDto>()
                .ForMember(dest => dest.GenreName, opt => opt.MapFrom(src => src.Genre != null ? src.Genre.Name : string.Empty));

            CreateMap<Movie, MovieUpdateDto>();
            CreateMap<MovieUpdateDto, Movie>();

            CreateMap<ActorUpdateDto, Actor>();
            CreateMap<ActorCreateDto, Actor>();
            CreateMap<Actor, ActorDto>();
        }
    }
}