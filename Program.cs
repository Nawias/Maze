/**
    Michał Wójcik 2021 
 */

using System;

namespace Maze2
{
    class Program
    {
        static void Main(string[] args)
        {
            /* Logika dotycząca labiryntu zawarta jest w klasie Maze w osobnym pliku */
            Maze maze = new Maze(10);
            maze.Generate();
            maze.Draw();
            maze.FindFullPath();
            maze.Draw();
        }
    }
}
