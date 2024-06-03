using Azure.Storage.Queues;
using KnightPath.Models;
using KnightPath.Services;
using NSubstitute;

namespace KnightPath.Tests
{
    public class KnightServiceTests
    {
        QueueClient queueClient = Substitute.For<QueueClient>();

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData(" ", " ")]
        public async Task MoveKnightAsync_ThrowsArgumentNullException_WithInvalidInput(string? source, string? target)
        {
            var knightService = new KnightService(queueClient);

            await Assert.ThrowsAsync<ArgumentNullException>(() => knightService.QueueKnightMoveAsync(source, target));
        }

        [Theory]
        [InlineData("01", "I1")]
        [InlineData("A0", "I0")]
        [InlineData("91", "H9")]
        public async Task MoveKnightAsync_ThrowsArgumentOutOfRangeException_WithInvalidInput(string? source, string? target)
        {
            var knightService = new KnightService(queueClient);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => knightService.QueueKnightMoveAsync(source, target));
        }

        [Theory]
        [InlineData("A11", "C2")]
        [InlineData("A1", "C21")]
        [InlineData("A", "C21")]
        [InlineData("A1", "C")]
        [InlineData("A 1", "C")]
        [InlineData("A1", "C 2")]
        public async Task MoveKnightAsync_ThrowsArgumentException_WithInvalidInput(string? source, string? target)
        {
            var knightService = new KnightService(queueClient);

            await Assert.ThrowsAsync<ArgumentException>(() => knightService.QueueKnightMoveAsync(source, target));
        }

        [Theory]
        [InlineData("A1", "B8")]
        [InlineData("C4", "A2")]
        [InlineData("E5", "D6")]
        [InlineData("G8", "A1")]
        [InlineData("    g8 ", "a1     ")]
        public async Task MoveKnightAsync_CallsQueueClient_WithValidInput(string? source, string? target)
        {
            var knightService = new KnightService(queueClient);

            await knightService.QueueKnightMoveAsync(source, target);

            await queueClient.Received(1).SendMessageAsync(Arg.Any<string>());
        }

        [Theory]
        [InlineData(0, 0, 5, 5, 4, "F6", 6)]
        [InlineData(7, 4, 1, 2, 4, "B3", 18)]
        public void MoveKnightAsync_GetsCorrectPaths_WithValidInput(int startHorizontal, int startVertical, int targetHorizontal, int targetVertical, int numberOfMoves, string targetLocation, int numberOfPaths)
        {
            var knightService = new KnightService(queueClient);

            var startLocation = new Location(startHorizontal, startVertical);
            var endLocation = new Location(targetHorizontal, targetVertical);

            var move = new Move(startLocation, endLocation);

            var operationId = Guid.NewGuid();

            var request = new Request(operationId, move);

            knightService.MoveKnight(request);

            var result = knightService.GetKnightPathResponse(operationId.ToString());

            Assert.Equal(numberOfPaths, result.ShortestPaths.Count);
            Assert.Equal(numberOfMoves, result.NumberOfMoves);
            Assert.Equal(targetLocation, result.Ending);

        }

        [Theory]
        [InlineData("1a79fe47-b04b-493a-a59e-cc40fedf4dc")]
        [InlineData("00000000-0000-0000-0000-000000000000")]
        public void MoveKnightAsync_ThrowsArgumentException_WithInValidOperationId(string operationId)
        {
            var knightService = new KnightService(queueClient);

            Assert.Throws<ArgumentException>(() => knightService.GetKnightPathResponse(operationId));
        }

        [Fact]
        public void MoveKnightAsync_ReturnsNull_WithValidOperationIdThatDoesNotExist()
        {
            var knightService = new KnightService(queueClient);

            var result = knightService.GetKnightPathResponse(Guid.NewGuid().ToString());

            Assert.Null(result);
        }
    }
}
