using Azure.Storage.Queues;
using KnightPath.Models;
using KnightPath.Services.Interfaces;
using System.Collections.Concurrent;
using System.Text.Json;

namespace KnightPath.Services
{
    public class KnightService : IKnightService
    {
        // possible knight XY movement
        private readonly int[] _xMovement = [2, 1, -1, -2, -2, -1, 1, 2];
        private readonly int[] _yMovement = [1, 2, 2, 1, -1, -2, -2, -1];

        // in-memory storage of shortest knight path moves, real-world this could be stored in a Db, or could even be pre-computed
        private readonly ConcurrentDictionary<Move, Models.KnightPath> _knightPaths = new();

        private readonly ConcurrentDictionary<Guid, Models.KnightPath> _operationsStore = new();

        private readonly QueueClient _queueClient;

        public KnightService(QueueClient queueClient)
        {
            _queueClient = queueClient;
        }

        /// <inheritdoc />
        public async Task<Guid> QueueKnightMoveAsync(string? source, string? target)
        {
            var operationId = Guid.NewGuid();

            var start = ConvertPositionToLocation(source);

            var end = ConvertPositionToLocation(target);

            var move = new Move(start, end);

            var request = new Request(operationId, move);

            await _queueClient.SendMessageAsync(JsonSerializer.Serialize(request));

            return operationId;
        }

        /// <inheritdoc />
        public KnightPathResponse? GetKnightPathResponse(string? operationId)
        {
            if (!Guid.TryParse(operationId, out var guid))
            {
                throw new ArgumentException("Operation Id is invalid", nameof(operationId));
            }

            if (guid == Guid.Empty)
            {
                throw new ArgumentException("Operation Id is empty", nameof(operationId));
            }

            // return null if the operation Id is not found
            if (!_operationsStore.TryRemove(guid, out var knightPath))
            {
                return null;
            }

            var knightPathResponse = new KnightPathResponse(guid, knightPath);

            return knightPathResponse;
        }

        /// <inheritdoc />
        public void MoveKnight(Request request)
        {
            // check if the shortest move has already been calculated
            if (_knightPaths.TryGetValue(request.Move, out var kPath))
            {
                _operationsStore.TryAdd(request.OperationId, kPath);
            }

            Movement topMostMove = new(null)
            {
                X = request.Move.Start.Horizontal,
                Y = request.Move.Start.Vertical
            };

            var moves = new List<Movement>() { topMostMove };

            bool shortPathFound = false;
            var jumpCount = 0;

            while (!shortPathFound)
            {
                // only loop through the most recently added nodes
                var topMostMoves = moves.Where(x => x.Jumps == jumpCount).ToList();
                jumpCount++;
                for (var x = topMostMoves.Count - 1; x >= 0; x--)
                {
                    Movement thisMove = topMostMoves[x];

                    // check all 8 possible moves
                    for (int i = 0; i < 8; i++)
                    {
                        var newX = thisMove.X + _xMovement[i];
                        var newY = thisMove.Y + _yMovement[i];
                        if (IsMoveValid(newX, newY))
                        {
                            if (newX == request.Move.Target.Horizontal && newY == request.Move.Target.Vertical)
                            {
                                shortPathFound = true;
                            }
                            moves.Add(new Movement(thisMove)
                            {
                                X = newX,
                                Y = newY,
                                Success = newX == request.Move.Target.Horizontal && newY == request.Move.Target.Vertical,
                                Jumps = jumpCount
                            });
                        }
                    }
                }
            }

            // find all paths that were successful landing on the target
            // and work backwards to find the path
            var successfulMoves = moves.Where(x => x.Success).ToList();

            var knightPath = new Models.KnightPath(ConvertXYToString(request.Move.Start.Horizontal, request.Move.Start.Vertical), ConvertXYToString(request.Move.Target.Horizontal, request.Move.Target.Vertical));
            foreach (var successfulMove in successfulMoves)
            {
                List<string> movesRequired = [];
                TraverseNodes(successfulMove, movesRequired);
                knightPath.ShortestPaths.Add(String.Join(":", movesRequired));
                knightPath.NumberOfMoves = successfulMove.Jumps;
            }

            // add to our local cache
            if (!_knightPaths.TryAdd(request.Move, knightPath))
            {
                throw new Exception("Failed to add to cache");
            }

            if (!_operationsStore.TryAdd(request.OperationId, knightPath))
            {
                throw new Exception("Failed to store the operation to the local store");
            }
        }

        private static void TraverseNodes(Movement move, List<string> moves)
        {
            moves.Add(ConvertXYToString(move.X, move.Y));

            if (move.ParentMove is not null)
            {
                TraverseNodes(move.ParentMove, moves);
            }
            moves.Reverse();
        }

        private static bool IsMoveValid(int x, int y)
        {
            return (x >= 0 && x <= 7 && y >= 0 && y <= 7);
        }

        private static string ConvertXYToString(int x, int y)
        {
            char xChar = (char)(65 + x);
            string yChar = (y + 1).ToString();

            return xChar + yChar;
        }

        /// <summary>
        /// Converts a position, such as, A1 to a numerical coordinate on a chess board.
        /// </summary>
        /// <param name="position">The string representation of the position on a chess board. Ex A1 or F7.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">The position provided is null or empty</exception>
        /// <exception cref="ArgumentOutOfRangeException">The position provided is out of range</exception>
        private static Location ConvertPositionToLocation(string? position)
        {
            if (string.IsNullOrWhiteSpace(position))
            {
                throw new ArgumentNullException(nameof(position), "The position is null or empty");
            }

            position = position.Trim().ToUpper(System.Globalization.CultureInfo.CurrentCulture);

            if (position.Length != 2)
            {
                throw new ArgumentException("The position is not valid, it must be a letter followed by a number", nameof(position));
            }

            int horizontal = position[0] switch
            {
                'A' => 0,
                'B' => 1,
                'C' => 2,
                'D' => 3,
                'E' => 4,
                'F' => 5,
                'G' => 6,
                'H' => 7,
                _ => throw new ArgumentOutOfRangeException(nameof(position), "The horizontal placement of the position is not valid, it must be a letter between A and H"),
            };

            if (!int.TryParse(position[1].ToString(), out int vertical))
            {
                throw new ArgumentOutOfRangeException(nameof(position), "The vertical placement of the position is not valid, it must be a number between 1 and 8");
            }

            if (vertical < 1 || vertical > 8)
            {
                throw new ArgumentOutOfRangeException(nameof(position), "The vertical placement of the position is not valid, it must be a number between 1 and 8");
            }

            var location = new Location(horizontal, vertical - 1);

            return location;
        }
    }
}
