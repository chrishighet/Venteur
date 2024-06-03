
namespace KnightPath.Models
{
    public class KnightPath
    {
        public KnightPath(string starting, string ending)
        {
            Starting = starting;
            Ending = ending;
        }

        /// <summary>
        /// All possible shortest paths from the starting position to the ending position.
        /// </summary>
        public List<string> ShortestPaths { get; set; } = [];

        /// <summary>
        /// Minimum number of moves required to reach the ending position.
        /// </summary>
        public int NumberOfMoves { get; set; }

        /// <summary>
        /// The starting position of the knight.
        /// </summary>
        public string Starting { get; init; }

        /// <summary>
        /// The ending position of the knight.
        /// </summary>
        public string Ending { get; init; }

    }

    /// <summary>
    /// The response object returned by the API.
    /// </summary>
    public class KnightPathResponse : KnightPath
    {
        public KnightPathResponse(Guid operationId, KnightPath knightPath) : base(knightPath.Starting, knightPath.Ending)
        {
            ShortestPaths = knightPath.ShortestPaths;
            NumberOfMoves = knightPath.NumberOfMoves;
            OperationId = operationId;
        }

        /// <summary>
        /// The unique identifier for the operation.
        /// </summary>
        public Guid OperationId { get; init; }
    }
}
