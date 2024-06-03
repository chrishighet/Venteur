using KnightPath.Models;

namespace KnightPath.Services.Interfaces
{
    public interface IKnightService
    {
        /// <summary>
        /// Gets the <see cref="KnightPathResponse"/> for the given <see cref="operationId""/>
        /// </summary>
        /// <param name="operationId"></param>
        /// <returns></returns>
        KnightPathResponse? GetKnightPathResponse(string? operationId);

        /// <summary>
        /// Queues the Knight's movement from the source to the target
        /// </summary>
        /// <param name="source">The starting position of the Knight</param>
        /// <param name="target">The desired ending position of the Knight</param>
        /// <returns>The operationId for the request</returns>
        Task<Guid> QueueKnightMoveAsync(string? source, string? target);

        /// <summary>
        /// Moves the Knight from the <see cref="Request.Source"/> to the <see cref="Request.Target"/> for the given <see cref="Request.OperationId"/>
        /// </summary>
        /// <param name="request">The object containing the operationId, source, and target</param>
        void MoveKnight(Request request);
    }
}