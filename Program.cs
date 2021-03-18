using System;

namespace Maze2
{
    class Program
    {
        static void Main(string[] args)
        {
            Maze maze = new Maze(7);
            maze.Generate();
            maze.Draw();
            maze.FindFullPath();
            maze.Draw();
        }
    }
}
