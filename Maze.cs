/**
    Michał Wójcik 2021 
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Maze2
{
    /* Enumeratory pomocne w oznaczaniu poszczególnych komórek, ścian planszy i kierunków */
    enum NodeType { WALL, PATH, START_POINT, END_POINT, GOOD_PATH, BAD_PATH }
    enum Wall { TOP, LEFT, BOTTOM, RIGHT }
    enum Direction { UP, LEFT, DOWN, RIGHT }

    /* Struktura opisująca komórkę */
    struct Node
    {
        public int x, y;
        public Node(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public Node Copy()
        {
            return new Node(x, y);
        }
    }

    /* Struktura opisująca krok w algorytmie znajdowania drogi w labiryncie */
    struct Step
    {
        public Node node;
        public Direction direction;
        public Step(Node node, Direction direction)
        {
            this.node = node;
            this.direction = direction;
        }
        public Step Copy()
        {
            return new Step(new Node(node.x, node.y), direction);
        }
    }
    class Maze
    {
        /*Kwadratowa tablica 2D przechowująca labirynt*/
        private NodeType[,] maze;
        /*Wymiar tablicy*/
        private int dimension;

        private Random rand = new Random();
        private bool generated = false;
        private int genSteps = 0;
        private int walkSteps = 0;

        public Maze(int dimension)
        {
            /*Rozmiar labiryntu podany w konstruktorze to rozmiar ścieżek: siatka składa się z pól i ścian*/
            this.dimension = (dimension*2)+1;
            maze = new NodeType[this.dimension, this.dimension];
        }

        /* Metoda wyświetlająca labirynt w konsoli */
        public void Draw()
        {
            for (int i = 0; i < dimension; i++)
            {
                for (int j = 0; j < dimension; j++)
                {
                    switch (maze[i, j])
                    {
                        case NodeType.WALL:
                            Console.Write("[#]");
                            break;
                        case NodeType.PATH:
                            Console.Write("   ");
                            break;
                        case NodeType.START_POINT:
                            Console.Write(" S ");
                            break;
                        case NodeType.END_POINT:
                            Console.Write(" E ");
                            break;
                        case NodeType.GOOD_PATH:
                            Console.Write(" * ");
                            break;
                        case NodeType.BAD_PATH:
                            Console.Write("   ");
                            break;
                        default:
                            Console.Write("[?]");
                            break;
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine("Generation Steps: " + genSteps + ", Pathfinding Steps: " + walkSteps);
        }


        /* Algorytm znajdowania ścieżki w wygenerowanym labiryncie */
        /* Algorytm bazuje na iteracyjnej implementacji algorytmu lewej ręki */
        /**
            1. Wybierz początkową komórkę w miejscu wejścia do labiryntu
            2. Gdy stos nie jest pusty i nie doszliśmy jeszcze do końca
                1. Jeśli komórka ma nieodwiedzonych sąsiadów
                    1. Oznacz bieżącą komórkę jako rozwiązanie
                    2. Odłóż bieżącą komórkę na stos
                    3. Wybierz następną komórkę według reguły lewej ręki
                2. Jeśli trafiliśmy do ślepego zaułka
                    1. Oznacz bieżącą komórkę jako złe rozwiązanie
                    2. Zdejmij komórkę ze stosu i oznacz jako bieżącą
         */
        public void FindFullPath()
        {
            if (!generated)
            {
                Console.WriteLine("Maze has not been generated");
                return;
            }
            Node startPoint;
            startPoint = FindStartPoint();

            Stack<Step> steps = new Stack<Step>();
            Step currentStep = new Step(startPoint, Direction.DOWN);


            do
            {
                /* Na wypadek błędu, gdzie ściana zostanie oznaczona jako następny krok */
                if (IsWall(currentStep.node)) 
                {
                    currentStep = steps.Pop();
                }
                else if (HasEmptySpacesAround(currentStep.node))
                {
                    markGood(currentStep);
                    steps.Push(currentStep);
                    currentStep = GetNextStep(currentStep);
                }
                else
                {
                    markBad(currentStep);
                    currentStep = steps.Pop();
                }
                // Licznik kroków do wyświetlenia w oknie wynikowym
                walkSteps++;
            } 
            while (!IsEndPoint(currentStep.node) && steps.Count > 0);
        }


        /* Algorytm generujący labirynt - randomizowane przeszukiwanie wgłąb - wersja iteracyjna*/
        /* Implementacja oparta na algorytmie ze strony https://pl.qaz.wiki/wiki/Maze_generation_algorithm */
        /**
            1. Wybierz początkową komórkę, oznacz ją jako odwiedzoną i umieść na stosie
            2. Gdy stos nie jest pusty
                1. Zdejmij komórkę ze stosu i ustaw ją jako bieżącą komórkę
                2. Jeśli bieżąca komórka ma sąsiadów, których nie odwiedzono
                    1. Odłóż bieżącą komórkę na stos
                    2. Wybierz losowo jednego z nieodwiedzonych sąsiadów
                    3. Usuń ścianę między bieżącą komórką a wybraną komórką
                    4. Oznacz wybraną komórkę jako odwiedzoną i umieść ją na stosie
        */
        public void Generate()
        {
            /*Stos do cofania się*/
            Stack<Node> nodes = new Stack<Node>();
            
            //Wejście do labiryntu
            maze[0, 1] = NodeType.START_POINT;

            // Wybór początkowej komórki do generowania
            Node currentNode = new Node(1, 1);
            maze[currentNode.y, currentNode.x] = NodeType.PATH;
            nodes.Push(currentNode);

            while (nodes.Count > 0)
            {
                //1. 
                currentNode = nodes.Pop();
                //2.
                if (HasNeighbours(currentNode))
                {
                    //1.
                    nodes.Push(currentNode);
                    //2.
                    Node nextNode = ChooseRandomNeighbour(currentNode);
                    //3.
                    RemoveWall(currentNode, nextNode);
                    //4.
                    maze[nextNode.y, nextNode.x] = NodeType.PATH;
                    nodes.Push(nextNode);
                }
                // Licznik kroków do wyświetlenia w konsoli
                genSteps++;
            }
            //Wyjście z labiryntu
            Node endPoint = GenerateEndPoint();
            maze[endPoint.y, endPoint.x] = NodeType.END_POINT;
            //Wygenerowano labirynt - można przejść do rozwiązywania go
            generated = true;
        }

        private void markGood(Step currentStep)
        {
            if (maze[currentStep.node.y, currentStep.node.x] != NodeType.START_POINT)
                maze[currentStep.node.y, currentStep.node.x] = NodeType.GOOD_PATH;
        }
        
        private void markBad(Step currentStep)
        {
            maze[currentStep.node.y, currentStep.node.x] = NodeType.BAD_PATH;
        }
        
        private Step GetNextStep(Step currentStep)
        {
            Step nextStep = currentStep.Copy();
            int count = 0;
            do
            {
                nextStep.node = currentStep.Copy().node;
                nextStep.direction = (Direction)((((int)nextStep.direction) + 1) % 4);
                if (IsEndPoint(nextStep.node)) break;
                switch (nextStep.direction)
                {
                    case Direction.DOWN:
                        if (nextStep.node.y < dimension-1)
                            nextStep.node.y += 1;
                        break;
                    case Direction.UP:
                        if(nextStep.node.y > 0)
                            nextStep.node.y -= 1;
                        break;
                    case Direction.LEFT:
                        if (nextStep.node.x > 0)
                            nextStep.node.x -= 1;
                        break;
                    case Direction.RIGHT:
                        if (nextStep.node.x < dimension - 1)
                            nextStep.node.x += 1;
                        break;
                }
                if (IsEndPoint(nextStep.node)) 
                    break;
                count++;
            } while (maze[nextStep.node.y, nextStep.node.x] != NodeType.PATH && count<4);

            return nextStep;

        }
        
        private Node FindStartPoint()
        {
            Node startPoint = new Node(0, 0);
            //TOP, BOTTOM
            for (int y = 0; y < dimension; y += dimension - 1)
            {
                for (int x = 0; x < dimension; x++)
                {
                    if (maze[y, x] == NodeType.START_POINT)
                    {
                        startPoint = new Node(x, y);
                        break;
                    }
                }
                if (!startPoint.Equals(new Node(0, 0))) break;
            }
            //LEFT, RIGHT
            if (startPoint.Equals(new Node(0, 0)))
            {
                for (int x = 0; x < dimension; x += dimension - 1)
                {
                    for (int y = 0; y < dimension; y++)
                    {
                        if (maze[y, x] == NodeType.START_POINT)
                        {
                            startPoint = new Node(x, y);
                            break;
                        }
                    }
                }
            }

            return startPoint;
        }
        
        private Node GenerateEndPoint()
        {
            Wall wall = (Wall)rand.Next(4);
            Node endPoint = new Node(0, 0);
            do
            {
                switch (wall)
                {
                    case Wall.TOP:
                        endPoint = new Node(rand.Next(dimension / 4, (dimension-1)/2)*2+1, 0);
                        if (maze[endPoint.y + 1, endPoint.x] == NodeType.WALL)
                            endPoint = new Node(0,0);
                        break;
                    case Wall.BOTTOM:
                        endPoint = new Node(rand.Next((dimension-1)/2) * 2 + 1, dimension - 1);
                        if (maze[endPoint.y - 1, endPoint.x] == NodeType.WALL)
                            endPoint = new Node(0, 0);
                        break;
                    case Wall.LEFT:
                        endPoint = new Node(0, rand.Next(dimension / 4, (dimension-1)/2) * 2 + 1);
                        if (maze[endPoint.y, endPoint.x+1] == NodeType.WALL)
                            endPoint = new Node(0, 0);
                        break;
                    case Wall.RIGHT:
                        endPoint = new Node(dimension - 1, rand.Next((dimension-1)/2) * 2+1);
                        if (maze[endPoint.y, endPoint.x-1] == NodeType.WALL)
                            endPoint = new Node(0, 0);
                        break;
                    default:
                        endPoint = new Node(rand.Next((dimension-1) / 2) * 2 + 1, dimension - 1);
                        if (maze[endPoint.y - 1, endPoint.x] == NodeType.WALL)
                            endPoint = new Node(0, 0);
                        break;
                }

            } while (endPoint.Equals(new Node(0, 0)));
            
            return endPoint;
        }
        
        private void RemoveWall(Node node, Node nextNode)
        {
            int x = (node.x + nextNode.x) / 2;
            int y = (node.y + nextNode.y) / 2;
            maze[y, x] = NodeType.PATH;
        }
        
        private Node ChooseRandomNeighbour(Node node)
        {
            List<Node> neighbours = new List<Node>();
            for (int y = node.y - 2; y <= node.y + 2; y += 2)
            {
                for (int x = node.x - 2; x <= node.x + 2; x += 2)
                {
                    if (IsInBounds(x,y) && IsNotCorner(node, x, y) && maze[y, x] == NodeType.WALL)
                    {
                        neighbours.Add(new Node(x, y));
                    }
                }
            }

            int index = rand.Next(neighbours.Count);
            return neighbours[index];

        }
        
        private bool IsWall(Node node)
        {
            return maze[node.y, node.x] == NodeType.WALL;
        }
        
        private bool IsEndPoint(Node node)
        {
            return maze[node.y, node.x] == NodeType.END_POINT;
        }
        
        private bool HasEmptySpacesAround(Node node)
        {
            int spaces = 0;
            for (int y = node.y - 1; y <= node.y + 1; y++)
            {
                for (int x = node.x - 1; x <= node.x + 1; x++)
                {

                    if (IsInBounds(x, y) && IsNotCorner(node, x, y) && maze[y, x] == NodeType.PATH)
                    {
                        spaces++;
                    }
                    if (IsInBounds(x, y) && IsNotCorner(node, x, y) && maze[y, x] == NodeType.END_POINT)
                    {
                        spaces++;
                    }
                }
            }
            return spaces > 0;
        }
        
        private bool HasNeighbours(Node node)
        {
            int neighbours = 0;
            for(int y = node.y - 2; y <= node.y+2; y += 2)
            {
                for (int x = node.x - 2; x <= node.x + 2; x += 2)
                {
                    if (IsInBounds(x,y) && IsNotCorner(node, x, y) && maze[y, x] == NodeType.WALL)
                    {
                        neighbours++;
                    }
                }
            }
            return neighbours > 0;
        }
        
        private bool IsNotCorner(Node node, int x, int y)
        {
            return (x == node.x || y == node.y);
        }
        
        private bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < dimension && y >= 0 && y < dimension;
        }
    }
}
