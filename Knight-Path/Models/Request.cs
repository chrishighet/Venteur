
namespace KnightPath.Models
{
    /// <summary>
    /// Used to queue the Knight movement for later processing.
    /// </summary>
    public class Request
    {
        public Request(Guid operationId, Move move)
        {
            OperationId = operationId;
            Move = move;
        }

        /// <summary>
        /// The unique identifier for the operation.
        /// </summary>
        public Guid OperationId { get; init; }

        /// <summary>
        /// The move to be processed.
        /// </summary>
        public Move Move { get; init; }
    }
}
