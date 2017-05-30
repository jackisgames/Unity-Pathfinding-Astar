using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Game
{ //simple implementation of A* pathfinding
    public class PathFinder {

        private bool[,] grids;
        private StringBuilder stringBuilder=new StringBuilder();
        private Dictionary<string,Node[]> cachedPath=new Dictionary<string, Node[]>();

        public void Init(int width,int height)
        {
            grids=new bool[width,height];
            cachedPath.Clear();
        }

        /*public void SetGrid(Node node, bool value)
        {
            SetGrid(node.x,node.y,value);
        }*/
        public void SetGrid(int x, int y, bool value)
        {
            if(OutsideBound(x,y))
                return;
            grids[x, y] = value;
            //reset cached path because waypoints might be different
            cachedPath.Clear();
        }

        /*public bool GetGridValue(Node node)
        {
            return GetGridValue(node.x, node.y);
        }*/
        public bool GetGridValue(int x, int y)
        {
            if (OutsideBound(x, y))
                return true;
            return grids[x, y];
        }

        public Node[] GetPosition(Node start,Node end)
        {
            //attempt to load from dictionary first
            string pathKey = GetKey(start, end);
            Node[] cached;
            cachedPath.TryGetValue(pathKey, out cached);
            if (cached == null)
            {
                //try reversing the order
                cachedPath.TryGetValue(GetKey(end, start),out cached);
                if (cached != null)
                {
                    //find one, reverse it
                    Node[] reversedArray=new Node[cached.Length];
                    System.Array.Copy(cached, reversedArray,cached.Length);
                    System.Array.Reverse(reversedArray);
                    cachedPath[pathKey] = reversedArray;
                    return reversedArray;
                }
            }
            else
            {
                return cached;
                
            }

            Stack<Node> result=new Stack<Node>();
            List<Position> openList=new List<Position>();
            List<Position> closedList=new List<Position>();

            Position startPosition = new Position(start.x, start.y);
            Position endPosition = new Position(end.x, end.y);
            closedList.Add(startPosition);
            openList.Add(startPosition);
            int traversalCount = 0;
            bool foundTarget=false;
            Position current=null;

            while (openList.Count>0)
            {
                current = openList[0];
                openList.RemoveAt(0);
                closedList.Add(current);
                if (current.Equals(endPosition))
                {
                    foundTarget = true;
                    break;
                }
                else
                {
                    traversalCount++;
                    closedList.Add(current);
                
                    
                    Stack<Position> neigbours = GetNeigbours(current);

                    while (neigbours.Count>0)
                    {
                        Position neighbour = neigbours.Pop();
                        if (!Exist(neighbour, closedList))
                        {

                            if (Exist(neighbour, openList))
                            {
                                if (traversalCount + neighbour.Cost < neighbour.TotalCost)
                                {
                                    neighbour.TraversalCount = traversalCount;
                                    neighbour.Previous = current;
                                }
                            }
                            else
                            {
                                neighbour.Previous = current;
                                neighbour.TraversalCount = traversalCount;
                                neighbour.Cost = Distance(neighbour, endPosition);

                                openList.Add(neighbour);
                            }
                        }
                    }
                }
            }
            if (foundTarget)
            {
                result.Push(current.Node);
                while (current.Previous != null)
                {
                    result.Push(current.Previous.Node);
                    current = current.Previous;
                    
                }
            }
            Node[] nodes = result.ToArray();
            cachedPath[pathKey] = nodes;
            return nodes;
        }
        private bool Exist(Position position,List<Position> positions)
        {
            for(int i=0;i<positions.Count;i++)
                if (positions[i].Equals(position))
                    return true;
            return false;
        }

        public Node[] GetNeigbours(Node node)
        {
            Stack<Position> neighbours = GetNeigbours(new Position(node.x, node.y));
            
            Node[] result=new Node[neighbours.Count];

            for (int i = 0; i < result.Length; i++)
                result[i] = neighbours.Pop().Node;

            return result;
        }
        private Stack<Position> GetNeigbours(Position position)
        {
            Stack<Position> result=new Stack<Position>();
            //left
            Position left = new Position(position.PositionX - 1,position.PositionY );
            if (Walkable(left))
                result.Push(left);

            //right
            Position right = new Position(position.PositionX + 1, position.PositionY );
            if (Walkable(right))
                result.Push(right);

            //Up
            Position up = new Position(position.PositionX,position.PositionY+1 );
            if (Walkable(up))
                result.Push(up);

            //Down
            Position down = new Position(position.PositionX,position.PositionY - 1 );
            if (Walkable(down))
                result.Push(down);

            return result;
        }

        
        public bool OutsideBound(int x, int y)
        {
            return x < 0 || x >= grids.GetLength(0) || y < 0 || y >= grids.GetLength(1);
        }
        private bool Walkable(Position position)
        {
            //outside bound
            if(OutsideBound(position.PositionX,position.PositionY))
                return false;
            //hit obstacle
            if (grids[position.PositionX, position.PositionY])
                return false;

            return true;
        }

        public  int Distance(Node current, Node next)
        {
            return Distance(current.x, current.y, next.x, next.y);
        }

        private int Distance(Position current, Position next)
        {
            return Distance(current.PositionX, current.PositionY, next.PositionX, next.PositionY);
        }

        private int Distance(int x1, int y1, int x2, int y2)
        {
            return System.Math.Abs(x1 - x2) + System.Math.Abs(y1 - y2);
        }

        private string GetKey(Node start,Node end)
        {
            stringBuilder.Length = 0;
            SetKey(start);
            SetKey(end);
            return stringBuilder.ToString();
        }

        private void SetKey(Node node)
        {

            stringBuilder.Append(node.x);
            stringBuilder.Append(",");
            stringBuilder.Append(node.y);
        }
    }

    public struct Node
    {
        public int x;
        public int y;
        public Node(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(Node other)
        {
            return x == other.x && y == other.y;
        }
    }
    internal class Position
    {

        public int PositionX;
        public int PositionY;
        public int TraversalCount;
        public int Cost;
        public Position Previous;

        public Position(int x, int y)
        {
            PositionX = x;
            PositionY = y;
        }

        public int TotalCost
        {
            get { return TraversalCount+Cost; }
        }

        public bool Equals(Position position)
        {
            if (position.PositionX == PositionX && position.PositionY == PositionY)
                return true;
            return false;
        }

        public Node Node
        {
            get
            {
                return new Node(PositionX,PositionY);
            }
        }

    }
}