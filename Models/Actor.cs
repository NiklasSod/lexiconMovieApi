namespace MovieApi.Models
{
    public class Actor
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public List<MovieActor> MovieActors { get; set; } = new();
    }
}
