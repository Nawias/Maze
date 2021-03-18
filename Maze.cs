using System;
using System.Collections.Generic;
using System.Text;

namespace Maze2
{
    enum NodeType { WALL, PATH, START_POINT, END_POINT, GOOD_PATH, BAD_PATH }
    enum Wall { TOP, LEFT, BOTTOM, RIGHT }
    enum Direction { UP, LEFT, DOWN, RIGHT }

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
        private NodeType[,] maze;
        private int dimension;
        private Stack<Node> nodes = new Stack<Node>();
        private Random rand = new Random();
        private bool generated = false;

        public Maze(int dimension)
        {
            this.dimension = (dimension*2)+1;
            maze = new NodeType[this.dimension, this.dimension];
        }

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
                            Console.Write(" O ");
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
        }

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

                if (hasEmptySpacesAround(currentStep.node))
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
                
            } 
            while (!IsEndPoint(currentStep.node) && steps.Count > 0);


        }

        private void markGood(Step currentStep)
        {
            maze[currentStep.node.y, currentStep.node.x] = NodeType.GOOD_PATH;
        }
        private void markBad(Step currentStep)
        {
            maze[currentStep.node.y, currentStep.node.x] = NodeType.BAD_PATH;
        }

        private bool hasEmptySpacesAround(Node node)
        {
            int spaces = 0;
            for (int y = node.y - 1; y <= node.y + 1; y++)
            {
                for (int x = node.x - 1; x <= node.x + 1; x++)
                {
                    if (isPointInBounds(x, y) && pointIsNotCorner(node, x, y) && ( maze[y, x] == NodeType.PATH || maze[y, x] == NodeType.END_POINT ))
                    {
                        spaces++;
                    }
                }
            }
            return spaces > 0;
        }

        Step GetNextStep(Step currentStep)
        {
            Step nextStep = currentStep.Copy();
            int count = 0;
            do
            {
                nextStep.node = currentStep.Copy().node;
                nextStep.direction = (Direction)((((int)nextStep.direction) + 1) % 4);
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
                if (IsEndPoint(nextStep.node)) break;
                count++;
            } while (maze[nextStep.node.y, nextStep.node.x] != NodeType.PATH && count<4);

            return nextStep;

        }
        

        private bool IsEndPoint(Node node)
        {
            return maze[node.y, node.x] == NodeType.END_POINT;
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

        public void Generate()
        {
            //Start Point

            maze[0, 1] = NodeType.START_POINT;
            // 1.
            Node node = new Node(1, 1);
            maze[node.y, node.x] = NodeType.PATH;
            nodes.Push(node);
            // 2.
            while(nodes.Count > 0) 
            {
                //1.
                node = nodes.Pop();
                //2.
                if (hasNeighbours(node))
                {
                    //1.
                    nodes.Push(node);
                    //2.
                    Node nextNode = chooseRandomNeighbour(node);
                    //3.
                    RemoveWall(node, nextNode);
                    //4.
                    maze[nextNode.y, nextNode.x] = NodeType.PATH;
                    nodes.Push(nextNode);
                }
            }
            //End Point
            Node endPoint = GenerateEndPoint();
            maze[endPoint.y, endPoint.x] = NodeType.END_POINT;
            //Finished
            generated = true;
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
                        endPoint = new Node(rand.Next(dimension / 4, dimension/2)*2+1, 0);
                        if (maze[endPoint.y + 1, endPoint.x] == NodeType.WALL)
                            endPoint = new Node(0,0);
                        break;
                    case Wall.BOTTOM:
                        endPoint = new Node(rand.Next(dimension/2) * 2 - 1, dimension - 1);
                        if (maze[endPoint.y - 1, endPoint.x] == NodeType.WALL)
                            endPoint = new Node(0, 0);
                        break;
                    case Wall.LEFT:
                        endPoint = new Node(0, rand.Next(dimension / 4, dimension/2) * 2 - 1);
                        if (maze[endPoint.y, endPoint.x+1] == NodeType.WALL)
                            endPoint = new Node(0, 0);
                        break;
                    case Wall.RIGHT:
                        endPoint = new Node(dimension - 1, rand.Next(dimension) * 2-1);
                        if (maze[endPoint.y, endPoint.x-1] == NodeType.WALL)
                            endPoint = new Node(0, 0);
                        break;
                    default:
                        endPoint = new Node(rand.Next(dimension * 2 - 1), 0);
                        if (maze[endPoint.y + 1, endPoint.x] == NodeType.WALL)
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

        private Node chooseRandomNeighbour(Node node)
        {
            List<Node> neighbours = new List<Node>();
            for (int y = node.y - 2; y <= node.y + 2; y += 2)
            {
                for (int x = node.x - 2; x <= node.x + 2; x += 2)
                {
                    if (isPointInBounds(x,y) && pointIsNotCorner(node, x, y) && maze[y, x] == NodeType.WALL)
                    {
                        neighbours.Add(new Node(x, y));
                    }
                }
            }

            int index = rand.Next(neighbours.Count);
            return neighbours[index];

        }

        private bool hasNeighbours(Node node)
        {
            int neighbours = 0;
            for(int y = node.y - 2; y <= node.y+2; y += 2)
            {
                for (int x = node.x - 2; x <= node.x + 2; x += 2)
                {
                    if (isPointInBounds(x,y) && pointIsNotCorner(node, x, y) && maze[y, x] == NodeType.WALL)
                    {
                        neighbours++;
                    }
                }
            }
            return neighbours > 0;
        }

        private bool pointIsNotCorner(Node node, int x, int y)
        {
            return (x == node.x || y == node.y);
        }
        private bool isPointInBounds(int x, int y)
        {
            return x > 0 && x < dimension && y > 0 && y < dimension;
        }
    }
}
