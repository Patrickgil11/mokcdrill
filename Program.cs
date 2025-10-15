
using System;
using System.Threading;

namespace RobotCleaner
{

    public class Map
    {
        private enum CellType { Empty, Dirt, Obstacle, Cleaned }
        private readonly CellType[,] _grid;

        public int Width { get; }
        public int Height { get; }

        public Map(int width, int height)
        {
            Width = width;
            Height = height;
            _grid = new CellType[width, height];

            // Initialize all cells as empty
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    _grid[x, y] = CellType.Empty;
        }

        public bool IsInBounds(int x, int y) =>
            x >= 0 && x < Width && y >= 0 && y < Height;

        public bool IsObstacle(int x, int y) =>
            IsInBounds(x, y) && _grid[x, y] == CellType.Obstacle;

        public bool IsDirt(int x, int y) =>
            IsInBounds(x, y) && _grid[x, y] == CellType.Dirt;

        public void AddDirt(int x, int y)
        {
            if (IsInBounds(x, y)) _grid[x, y] = CellType.Dirt;
        }

        public void AddObstacle(int x, int y)
        {
            if (IsInBounds(x, y)) _grid[x, y] = CellType.Obstacle;
        }

        public void Clean(int x, int y)
        {
            if (IsInBounds(x, y))
                _grid[x, y] = CellType.Cleaned;
        }

        // Display current map state
        public void Display(int robotX, int robotY)
        {
            Console.Clear();
            Console.WriteLine("🧹 Vacuum Robot Simulation");
            Console.WriteLine("---------------------------");
            Console.WriteLine("Legend: R=Robot, D=Dirt, #=Obstacle, C=Cleaned, .=Empty\n");

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (x == robotX && y == robotY)
                        Console.Write("R ");
                    else
                    {
                        switch (_grid[x, y])
                        {
                            case CellType.Empty: Console.Write(". "); break;
                            case CellType.Dirt: Console.Write("D "); break;
                            case CellType.Obstacle: Console.Write("# "); break;
                            case CellType.Cleaned: Console.Write("C "); break;
                        }
                    }
                }
                Console.WriteLine();
            }
            Thread.Sleep(150);
        }
    }

  
    public interface IStrategy
    {
        void Clean(Robot robot);
    }

 
    public class Robot
    {
        private readonly Map _map;
        private readonly IStrategy _strategy;

        public int X { get; private set; }
        public int Y { get; private set; }

        public Map Map => _map;

        public Robot(Map map, IStrategy strategy)
        {
            _map = map;
            _strategy = strategy;
        }

        public bool Move(int newX, int newY)
        {
            if (!_map.IsInBounds(newX, newY) || _map.IsObstacle(newX, newY))
                return false;

            X = newX;
            Y = newY;
            _map.Display(X, Y);
            return true;
        }

        public void CleanCurrentSpot()
        {
            if (_map.IsDirt(X, Y))
                _map.Clean(X, Y);
        }

        public void StartCleaning()
        {
            _map.Display(X, Y);
            CleanCurrentSpot();
            _strategy.Clean(this);
        }
    }

 
    public class SpiralStrategy : IStrategy
    {
        public void Clean(Robot robot)
        {
            Console.WriteLine("Starting Spiral Cleaning...\n");
            Thread.Sleep(800);

            // Directions: Right, Down, Left, Up
            int[,] dirs = { { 1, 0 }, { 0, 1 }, { -1, 0 }, { 0, -1 } };

            int dirIndex = 0;       // Start going right
            int segmentLength = 1;  // Steps per direction
            int stepsTaken = 0;     // Steps so far in current direction
            int turns = 0;          // Count of direction changes

            while (true)
            {
                bool moved = false;

                int nextX = robot.X + dirs[dirIndex, 0];
                int nextY = robot.Y + dirs[dirIndex, 1];

                // Try to move; if blocked, change direction
                if (!robot.Move(nextX, nextY))
                {
                    dirIndex = (dirIndex + 1) % 4; // turn right
                    turns++;
                    if (turns % 2 == 0)
                        segmentLength++;
                    stepsTaken = 0;
                    continue;
                }

                // Successful move
                moved = true;
                robot.CleanCurrentSpot();
                stepsTaken++;

                // Turn direction when segment length reached
                if (stepsTaken == segmentLength)
                {
                    dirIndex = (dirIndex + 1) % 4;
                    stepsTaken = 0;
                    turns++;

                    if (turns % 2 == 0)
                        segmentLength++;
                }

                // Check if trapped in all directions
                bool allBlocked = true;
                for (int i = 0; i < 4; i++)
                {
                    int checkX = robot.X + dirs[i, 0];
                    int checkY = robot.Y + dirs[i, 1];
                    if (robot.Map.IsInBounds(checkX, checkY) &&
                        !robot.Map.IsObstacle(checkX, checkY))
                    {
                        allBlocked = false;
                        break;
                    }
                }

                // Stop if no more moves available
                if (!moved && allBlocked)
                {
                    Console.WriteLine("\n✅ Cleaning complete! Robot can no longer move.");
                    break;
                }
            }
        }
    }

    
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Initialize Robot...\n");

            // Create a 20x10 map
            Map map = new Map(20, 10);

            // Add dirt
            map.AddDirt(5, 3);
            map.AddDirt(10, 8);
            map.AddDirt(1, 1);
            map.AddDirt(9, 5);
            map.AddDirt(15, 2);

            // Add obstacles
            map.AddObstacle(2, 5);
            map.AddObstacle(12, 1);
            map.AddObstacle(15, 4);
            map.AddObstacle(6, 8);
            map.AddObstacle(9, 7);

            // Use the Spiral strategy
            IStrategy strategy = new SpiralStrategy();
            Robot robot = new Robot(map, strategy);

            // Start robot at center
            int startX = map.Width / 2;
            int startY = map.Height / 2;
            robot.Move(startX, startY);

            // Begin cleaning
            robot.StartCleaning();

            Console.WriteLine("\nProcess Finished.\n");
        }
    }
}
