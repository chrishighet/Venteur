
namespace KnightPath.Models
{
    /// <summary>
    /// Represents a location on the chessboard
    /// </summary>
    public class Location
    {
        public Location(int horizontal, int vertical)
        {
            Horizontal = horizontal;
            Vertical = vertical;
        }

        public int Horizontal { get; init; }
        public int Vertical { get; init; }

    }
}
