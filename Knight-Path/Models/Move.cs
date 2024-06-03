
namespace KnightPath.Models
{
    public class Move
    {
        public Move(Location start, Location target)
        {
            Start = start;
            Target = target;
        }

        public Location Start { get; init; }
        public Location Target { get; init; }
    }
}
