#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Domain;

namespace BattleShipBrain
{
    public class BsBrain
    {
        public const int AmountOfBoardGenerationDuringSettingsValidation = 50;
        
        // Main gameplay data
        private bool _againstAi;
        private readonly GameBoard[] _gameBoards = new GameBoard[2];
        private int _currentPlayerId = 1;
        
        // Ship placement data
        private readonly List<ShipConfig> _shipConfigs = new();
        private readonly EShipTouchRule _touchRule;
        private List<Ship> _previouslyPlacedShips = new();
        private int _rotation;
        private int _shipConfigIndex;
        private int _currentShipQuantity;
        private Coordinate _shipPlacementPreviewCoordinate;

        public BsBrain()
        {
        }

        public BsBrain(GameConfig config)
        {
            _gameBoards[0] = new GameBoard();
            _gameBoards[1] = new GameBoard();
            
            _gameBoards[0].Board = GenerateEmptyBoard(config.BoardLength, config.BoardWidth);
            _gameBoards[1].Board = GenerateEmptyBoard(config.BoardLength, config.BoardWidth);

            _shipConfigs = config.ShipConfigs;
            _touchRule = config.EShipTouchRule;
            _currentShipQuantity = _shipConfigs[0].Quantity;
        }
        
        public int GetCurrentPlayerId()
        {
            return _currentPlayerId;
        }

        public int GetOpponentPlayerId()
        {
            return _currentPlayerId == 1 ? 2 : 1;
        }

        public void SwapCurrentPlayerId()
        {
            _currentPlayerId = GetOpponentPlayerId();
        }

        public Coordinate GetCurrentPlayerLastMove()
        {
            var prevMoves = _gameBoards[GetCurrentPlayerId() - 1].PreviousMoves;
            return prevMoves.Count > 0 ? prevMoves.Peek() : new Coordinate(0, 0);
        }

        public bool AreShipsPlaced()
        {
            var shipsCount = CountShips();
            return _gameBoards[0].Ships.Count == shipsCount && _gameBoards[1].Ships.Count == shipsCount;
        }

        private int CountShips()
        {
            return _shipConfigs.Sum(config => config.Quantity);
        }

        public bool AreShipsPlacedOnCurrentBoard()
        {
            return _gameBoards[GetCurrentPlayerId() - 1].Ships.Count == CountShips();
        }

        public List<ShipConfig> GetShipConfigs()
        {
            return _shipConfigs;
        }

        public bool CurrentPlayerHasShipsOnBoard()
        {
            return _gameBoards[GetCurrentPlayerId() - 1].Ships.Count > 0;
        }

        public List<Coordinate> GetListOfSunkenShipsCoordinatesByPlayerId(int playerId)
        {
            var sunkenShipsCords = new List<Coordinate>();
            var playerGameBoard = _gameBoards[playerId - 1];

            foreach (var ship in playerGameBoard.Ships.Where(ship => ship.IsShipSunken(playerGameBoard.Board)))
            {
                sunkenShipsCords.AddRange(ship.Coordinates);
            }

            return sunkenShipsCords;
        }

        private void ClearCurrentPlayerBoard()
        {
            var currentBoard = _gameBoards[GetCurrentPlayerId() - 1];
            currentBoard.Board = GenerateEmptyBoard(
                _gameBoards[0].Board.GetLength(0),
                _gameBoards[0].Board.GetLength(1));
            currentBoard.Ships = new List<Ship>();
        }

        private static BoardSquareState[,] GenerateEmptyBoard(int xSize, int ySize)
        {
            var generatedBoard = new BoardSquareState[xSize, ySize];
            
            for (var x = 0; x < xSize; x++)
            {
                for (var y = 0; y < ySize; y++)
                {
                    var squareState = new BoardSquareState
                    {
                        IsBomb = false,
                        IsShip = false
                    };

                    generatedBoard[x, y] = squareState;
                }
            }

            return generatedBoard;
        }

        public void GenerateShipPlacementOnCurrentBoard()
        {
            var generated = false;
            while (!generated)
            {
                generated = GenerateShipPlacementOnCurrentBoardOnce();
            }
        }

        private bool GenerateShipPlacementOnCurrentBoardOnce()
        {
            ClearCurrentPlayerBoard();
            foreach (var ship in _shipConfigs)
            {
                var quantity = ship.Quantity;
                while (quantity > 0)
                {
                    var placed = PlaceShipRandomly(ship);
                    if (!placed)
                    {
                        ClearCurrentPlayerBoard();
                        return false;
                    }

                    quantity--;
                }
            }

            return true;
        }

        private List<Coordinate> GetAvailableCordsOfCurrentBoard()
        {
            var availableCords = new List<Coordinate>();
            var board = GetBoardBasedOnPlayerId(_currentPlayerId);

            for (var x = 0; x < board.GetLength(0); x++)
            {
                for (var y = 0; y < board.GetLength(0); y++)
                {
                    var cord = new Coordinate(x, y);
                    if (ValidateSquareState(cord))
                    {
                        availableCords.Add(cord);
                    }
                }
            }

            return availableCords;
        }

        private bool PlaceShipRandomly(ShipConfig ship)
        {
            var random = new Random();
            var availableCords = GetAvailableCordsOfCurrentBoard();
            var placed = false;

            while (!placed)
            {
                if (availableCords.Count == 0)
                {
                    return false;
                }

                var randomAvailableCordIndex = random.Next(0, availableCords.Count);
                var rotCycle = false;
                var rotation = random.Next(0, 4);
                var innerRotation = rotation;
                
                while (!placed)
                {
                    if (rotCycle && innerRotation == rotation)
                    {
                        break;
                    }

                    placed = PlaceShipOnCurrentBoard(availableCords[randomAvailableCordIndex], ship, innerRotation);
                    rotCycle = true;
                }

                availableCords.RemoveAt(randomAvailableCordIndex);
            }

            return true;
        }

        public void InitShipPlacement()
        {
            _rotation = 0;
            _shipConfigIndex = 0;
            _currentShipQuantity = _shipConfigs[_shipConfigIndex].Quantity;
            _shipPlacementPreviewCoordinate = new(0, 0);
        }

        public void PlaceShip()
        {
            var currentConfig = _shipConfigs[_shipConfigIndex];
            if (PlaceShipOnCurrentBoard(_shipPlacementPreviewCoordinate, currentConfig, _rotation))
            {
                UpdateCurrentShipQuantity(-1);
                _shipPlacementPreviewCoordinate = new(0, 0);
                _rotation = 0;
            }
        }

        private void UpdateCurrentShipQuantity(int quantityChange)
        {
            var currentConfigQuantity = _shipConfigs[_shipConfigIndex].Quantity;
            var newQuantity = _currentShipQuantity + quantityChange;

            if (newQuantity == 0)
            {
                if (_shipConfigIndex == _shipConfigs.Count - 1) return;

                _shipConfigIndex++;
                _currentShipQuantity = _shipConfigs[_shipConfigIndex].Quantity;
                return;
            }
        
            if (newQuantity > currentConfigQuantity)
            {
                if (_shipConfigIndex == 0) return;
                
                _shipConfigIndex--;
                _currentShipQuantity = 1;
                return;
            }

            _currentShipQuantity = newQuantity;
        }

        private bool PlaceShipOnCurrentBoard(Coordinate placementCord, ShipConfig config, int rotation)
        {
            if (!ValidateShipPlacement(placementCord, config, rotation)) return false;

            var ship = new Ship(config.Name, GetShipPlacementCordsPreview(placementCord, config, rotation));
            GetBoardShipsBasedOnPlayerId(_currentPlayerId).Add(ship);
            placeShipOnBoard(ship);
            // Clear previously placed ships, to prohibit previously placed ships redo on currently placed ships
            ClearPreviouslyPlacedShips();

            return true;
        }

        public void UndoLastShipPlacementOnCurrentBoard()
        {
            var cBoard = _gameBoards[GetCurrentPlayerId() - 1];

            if (cBoard.Ships.Count == 0) return;
            var cShip = cBoard.Ships[^1];
            foreach (var cord in cShip.Coordinates)
            {
                cBoard.Board[cord.X, cord.Y].IsShip = false;
            }
            
            _previouslyPlacedShips.Add(cShip);
            cBoard.Ships.RemoveAt(cBoard.Ships.Count - 1);
            
            UpdateCurrentShipQuantity(1);
            _shipPlacementPreviewCoordinate = new(0, 0);
        }

        public bool RedoLastShipPlacementOnCurrentBoard()
        {
            if (_previouslyPlacedShips.Count == 0) return false;
            var recentlyPlacedShip = _previouslyPlacedShips[^1]; 
            
            placeShipOnBoard(recentlyPlacedShip);
            GetBoardShipsBasedOnPlayerId(_currentPlayerId).Add(recentlyPlacedShip);
            _previouslyPlacedShips.Remove(recentlyPlacedShip);
            
            UpdateCurrentShipQuantity(-1);
            _shipPlacementPreviewCoordinate = new(0, 0);

            return true;
        }

        public void ClearPreviouslyPlacedShips()
        {
            _previouslyPlacedShips = new List<Ship>();
        }

        public void RotateShip(int rotationChange)
        {
            var newRotation = _rotation + rotationChange;
            var updatedRotation = UpdateRotation(newRotation);
            
            if (!ValidateShipPlacementInsideGameBoardBoarders(_shipPlacementPreviewCoordinate,
                    _shipConfigs[_shipConfigIndex], updatedRotation))
            {
                newRotation -= rotationChange;
                _rotation = newRotation;
            }
            else
            {
                _rotation = updatedRotation;
            }
        }

        private int UpdateRotation(int rotation)
        {
            if (rotation == -1) {
                rotation = 3;
            } else {
                rotation %= 4;
            }

            return rotation;
        }

        public Coordinate GetShipPlacementPreviewCoordinate()
        {
            return _shipPlacementPreviewCoordinate;
        }

        public void UpdateShipPlacementPreviewCoordinate(Coordinate newCoordinate)
        {
            _shipPlacementPreviewCoordinate = newCoordinate;
        }

        private void placeShipOnBoard(Ship ship)
        {
            foreach (var cord in ship.Coordinates)
            {
                GetBoardBasedOnPlayerId(_currentPlayerId)[cord.X, cord.Y].IsShip = true;
            }
        }

        private bool ValidateShipPlacement(Coordinate placementCord, ShipConfig config, int rotation)
        {
            var cords = GetShipPlacementCordsPreview(placementCord, config, rotation);
            foreach (var cord in cords)
            {
                if (!ValidateSquareState(cord))
                {
                    return false;
                }
            }
            
            return true;
        }
    
        private bool ValidateSquareState(Coordinate cord)
        {
            // Check, is ship block placement square inside game board
            if (!ValidateSquareStatePlacementInBoardBoarders(cord))
            {
                return false;
            }

            for (var x = cord.X - 1; x < cord.X + 2; x++)
            {
                for (var y = cord.Y - 1; y < cord.Y + 2; y++)
                {
                    if (!ValidateSquareStatePlacementInBoardBoarders(new Coordinate(x, y)))
                    {
                        continue;
                    }

                    switch (_touchRule)
                    {
                        case EShipTouchRule.CornerTouch when x != cord.X && y != cord.Y:
                        case EShipTouchRule.SideTouch when x != cord.X || y != cord.Y:
                            continue;
                    }

                    if (GetBoardBasedOnPlayerId(_currentPlayerId)[x, y].IsShip)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool ValidateSquareStatePlacementInBoardBoarders(Coordinate cord)
        {
            return cord.X >= 0 && cord.X < GetBoardBasedOnPlayerId(_currentPlayerId).GetLength(0) &&
                   cord.Y >= 0 && cord.Y < GetBoardBasedOnPlayerId(_currentPlayerId).GetLength(1);
        }

        public bool ValidateShipPlacementInsideGameBoardBoarders(Coordinate newCoordinate)
        {
            return ValidateShipPlacementInsideGameBoardBoarders(newCoordinate,
                _shipConfigs[_shipConfigIndex],
                _rotation);
        }

        private bool ValidateShipPlacementInsideGameBoardBoarders(Coordinate placementCord, ShipConfig config,
            int rotation)
        {
            var cords = GetShipPlacementCordsPreview(placementCord, config, rotation);

            return cords.All(ValidateSquareStatePlacementInBoardBoarders);
        }

        public List<Coordinate> GetShipPlacementCordsPreview()
        {
            return GetShipPlacementCordsPreview(_shipPlacementPreviewCoordinate, _shipConfigs[_shipConfigIndex],
                _rotation);
        }

        private List<Coordinate> GetShipPlacementCordsPreview(Coordinate placementCord, ShipConfig config,
            int rotation)
        {
            var cords = new List<Coordinate>();
            var rotationLogic = new ShipRotationLogic(rotation,
                placementCord, config);

            for (var y = placementCord.Y; rotationLogic.YComparisonLogic!(y);)
            {
                for (var x = placementCord.X; rotationLogic.XComparisonLogic!(x);)
                {
                    cords.Add(new Coordinate(x, y));
                    x = rotationLogic.XMovementLogic!(x);
                }
                y = rotationLogic.YMovementLogic!(y);
            }

            return cords;
        }

        public bool Fire(int x, int y)
        {
            if (GetBoardBasedOnPlayerId(GetOpponentPlayerId())[x, y].IsBomb)
            {
                return false;
            }
            GetBoardBasedOnPlayerId(GetOpponentPlayerId())[x, y].IsBomb = true;
            _gameBoards[GetCurrentPlayerId() - 1].PreviousMoves.Push(new Coordinate(x, y));
            
            // It is made to continue player move when ship gets hit
            return !GetBoardBasedOnPlayerId(GetOpponentPlayerId())[x, y].IsShip;
        }

        public bool HasCurrentPlayerWon()
        {
            var enemyBoard = _gameBoards[GetOpponentPlayerId() - 1];

            foreach (var ship in enemyBoard.Ships)
            {
                if (!ship.IsShipSunken(enemyBoard.Board))
                {
                    return false;
                }
            }

            return true;
        }

        public int ValidateCurrentGameSettings()
        {
            var rotations = AmountOfBoardGenerationDuringSettingsValidation;
            var successfulValidations = 0;
            while (rotations > 0)
            {
                if (GenerateShipPlacementOnCurrentBoardOnce())
                {
                    successfulValidations++;
                }

                rotations--;
            }

            return successfulValidations;
        }

        public BoardSquareState[,] GetBoard(int playerNo)
        {
            return CopyOfBoard(GetBoardBasedOnPlayerId(playerNo));
        }

        private BoardSquareState[,] GetBoardBasedOnPlayerId(int playerId)
        {
            return playerId == 1 ? _gameBoards[0].Board : _gameBoards[1].Board;
        }

        private List<Ship> GetBoardShipsBasedOnPlayerId(int playerId)
        {
            return playerId == 1 ? _gameBoards[0].Ships : _gameBoards[1].Ships;
        }

        private BoardSquareState[,] CopyOfBoard(BoardSquareState[,] board)
        {
            var res = new BoardSquareState[board.GetLength(0), board.GetLength(1)];
            for (var x = 0; x < board.GetLength(0); x++)
            {
                for (var y = 0; y < board.GetLength(1); y++)
                {
                    res[x, y] = board[x, y];
                }
            }
            return res;
        }

        private SaveGameDto GetBrainDto()
        {
            var dto = new SaveGameDto();

            dto.GameBoards = GetBrainGameBoardsDtos();
            dto.CurrentPlayerId = _currentPlayerId;
            dto.AgainstAi = _againstAi;

            return dto;
        }

        public string GetBrainJson()
        {
            var dto = GetBrainDto();

            var jsonStr = GetJson(dto);
            return jsonStr;
        }

        public void RestoreBrainFromDb(SavedBsBrain loadedBrain)
        {
            RestoreBrainGameBoardsFromJson(loadedBrain.GameBoards);
            _currentPlayerId = loadedBrain.CurrentPlayerId;
            _againstAi = loadedBrain.AgainstAi;
            
        }

        public void RestoreBrainFromJson(SaveGameDto dto)
        {
            RestoreBoardsFromDtos(dto.GameBoards);
            _againstAi = dto.AgainstAi;
            _currentPlayerId = dto.CurrentPlayerId;
        }
        
        private void RestoreBrainGameBoardsFromJson(string json)
        {
            var jsonOptions = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            
            var restoredBoards = JsonSerializer.Deserialize<SaveGameDto.GameBoardDto[]>(json, jsonOptions);
            RestoreBoardsFromDtos(restoredBoards!);
        }
        private void RestoreBoardsFromDtos(SaveGameDto.GameBoardDto[] boardDtos)
        {
            var boardId = 0;
            
            foreach (var board in boardDtos)
            {
                var x = 0;
                foreach (var boardRow in board.Board)
                {
                    var y = 0;
                    foreach (var squareState in boardRow)
                    {
                        _gameBoards[boardId].Board[x, y] = squareState;
                        y++;
                    }
                    x++;
                }

                _gameBoards[boardId].Ships = boardDtos[boardId].Ships;
                boardId++;
            }
        }

        private SaveGameDto.GameBoardDto[] GetBrainGameBoardsDtos()
        {
            var gameBoardsDtos = new SaveGameDto.GameBoardDto[2];
            gameBoardsDtos[0] = new SaveGameDto.GameBoardDto();
            gameBoardsDtos[1] = new SaveGameDto.GameBoardDto();

            var boardId = 0;
            foreach (var board in _gameBoards)
            {
                var boardToTransfer = new List<List<BoardSquareState>>();
                for (var x = 0; x < board.Board.GetLength(0); x++)
                {
                    var boardRow = new List<BoardSquareState>();
                    for (var y = 0; y < board.Board.GetLength(1); y++)
                    {
                        boardRow.Add(board.Board[x, y]);
                    }
                    boardToTransfer.Add(boardRow);
                }

                gameBoardsDtos[boardId].Ships = GetBoardShipsBasedOnPlayerId(boardId + 1);
                gameBoardsDtos[boardId].Board = boardToTransfer;
                boardId++;
            }

            return gameBoardsDtos;
        }

        public string GetBrainGameBoardsJson(bool writeIndented = true)
        {
            var dtos = GetBrainGameBoardsDtos();

            var jsonStr = GetJson(dtos, writeIndented);
            return jsonStr;
        }

        private string GetJson(object something, bool writeIndented = true)
        {
            var jsonOptions = new JsonSerializerOptions()
            {
                WriteIndented = writeIndented
            };
            
            var jsonStr = JsonSerializer.Serialize(something, jsonOptions);
            return jsonStr;
        }

        public void SetGameAgainstAi()
        {
            _againstAi = true;
        }

        public bool IsGameAgainstAi()
        {
            return _againstAi;
        }

        public Coordinate AiMakeMove()
        {
            var random = new Random();
            var cordsWithoutBombs = GetOpponentBoardCoordinatesWithoutBombs();
            
            var randomCoordinateWithoutBomb = cordsWithoutBombs[random.Next(0, cordsWithoutBombs.Count)];

            return randomCoordinateWithoutBomb;
        }

        private List<Coordinate> GetOpponentBoardCoordinatesWithoutBombs()
        {
            var cordsWithoutBombs = new List<Coordinate>();
            var board = GetBoardBasedOnPlayerId(GetOpponentPlayerId());
            
            for (var x = 0; x < board.GetLength(0); x++)
            {
                for (var y = 0; y < board.GetLength(1); y++)
                {
                    if (board[x, y].IsBomb == false)
                    {
                        cordsWithoutBombs.Add(new Coordinate(x, y));
                    } 
                }
            }

            return cordsWithoutBombs;
        }
        
        public bool DoesAiMoveNow()
        {
            return _currentPlayerId == 2 && IsGameAgainstAi();
        }

        public ShipPlacementState GetBrainShipPlacementState()
        {
            var placementState = new ShipPlacementState
            {
                Rotation = _rotation,
                CurrentShipQuantity = _currentShipQuantity,
                ShipConfigIndex = _shipConfigIndex,
                ShipPlacementPreviewCoordinate = _shipPlacementPreviewCoordinate.GetCoordinateJson(),
                PreviouslyPlacedShips = JsonSerializer.Serialize(_previouslyPlacedShips)
            };

            return placementState;
        }

        public void RestoreShipPlacementState(ShipPlacementState placementState)
        {
            _rotation = placementState.Rotation;
            _currentShipQuantity = placementState.CurrentShipQuantity;
            _shipConfigIndex = placementState.ShipConfigIndex;
            _shipPlacementPreviewCoordinate.RestoreCoordinateFromJson(placementState.ShipPlacementPreviewCoordinate);
            _previouslyPlacedShips = JsonSerializer.Deserialize<List<Ship>>(placementState.PreviouslyPlacedShips)!;
        }
    }
}