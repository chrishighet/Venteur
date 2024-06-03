
namespace KnightPath.Models
{
    internal class Movement
    {
        public Movement(Movement? parentMove)
        {
            ParentMove = parentMove;
        }

        /// <summary>
        /// The move that was made prior to this one
        /// </summary>
        public Movement? ParentMove { get; set; }

        /// <summary>
        /// The horizontal position of this move
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// The vertical position of this move
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Whether this move landed on the target position
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// How many jumps this move took
        /// </summary>
        public int Jumps { get; set; }
    }
}