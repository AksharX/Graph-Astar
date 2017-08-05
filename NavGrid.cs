using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavGrid : MonoBehaviour {

    public Vector3 _GameSize;

    //Decreasing this will decrease performance but have fine points.
    public float DistanceBetweenNodes = 1;
    //Add a layer of cushion for for the Gridd to be generated.
    public float PadddingOnGameWorld = 10;

    //Determines where the center of the grid will occur.
    public Vector3 GridCenterOrigin = new Vector3(0, 0, 0);

    private Vector3 startingPoint;
    private int NodeSizeX;
    private int NodeSizeY;
    private int NodeSizeZ;
    private Node[,,] NavGraph;
    
    private int NumberofConnections;
    private Vector3 gameSize;

    void Start()
    {

        gameSize = _GameSize;
        //Game Size is multiplied by two because original game size is taking negative and positive;
        gameSize.Scale(new Vector3(2, 2, 2));
        generateDungeon = null;

        NodeSizeX = (int)((gameSize.x + PadddingOnGameWorld) / DistanceBetweenNodes);
        NodeSizeY = (int)((gameSize.y + PadddingOnGameWorld) / DistanceBetweenNodes);
        NodeSizeZ = (int)((gameSize.z + PadddingOnGameWorld) / DistanceBetweenNodes);

        float startingX = GridCenterOrigin.x - ((gameSize.x + PadddingOnGameWorld) / 2);
        float startingY = GridCenterOrigin.y - ((gameSize.y + PadddingOnGameWorld) / 2);
        float startingZ = GridCenterOrigin.z - (gameSize.z + PadddingOnGameWorld) / 2;

        startingPoint = new Vector3(startingX, startingY, startingZ);

        NavGraph = new Node[NodeSizeX, NodeSizeY, NodeSizeZ];

        NumberofConnections = Enum.GetValues(typeof(NodePosition)).Length;

        generateGraph();
    }


    //Generate a 3D grid of nodes that checks if there are any obstacles in the way and will set the node to Inactive if there is.
    void generateGraph()
    {
        Vector3 pointInWorld = startingPoint;
        float radiusCheckSphere = DistanceBetweenNodes / 2; 
        for (int x = 0; x < NodeSizeX; x++)
        {
            for (int y = 0; y < NodeSizeY; y++)
            {
                for (int z = 0; z < NodeSizeZ; z++)
                {
                    NavGraph[x, y, z] = new Node(pointInWorld, new Vector3(x, y, z));
                    NavGraph[x,y,z].isActive = !Physics.CheckSphere(pointInWorld,radiusCheckSphere);
                    pointInWorld += new Vector3(0, 0, DistanceBetweenNodes);
                }
                pointInWorld = new Vector3(pointInWorld.x, pointInWorld.y, startingPoint.z);

                pointInWorld += new Vector3(0, DistanceBetweenNodes, 0);
            }
            pointInWorld = new Vector3(pointInWorld.x, startingPoint.x, pointInWorld.z);

            pointInWorld += new Vector3(DistanceBetweenNodes, 0, 0);
        }
    }


    // Add more NodePosition if you want to add more nieghbors. Will increase Astar performance if you add more
    public enum NodePosition
    {
        up = 0,
        down = 1,
        left = 2,
        right = 3,
        forward = 4,
        back = 5,

    }

    //Get a node at a world Point. Will return a node thats closest to that world point Takes in a vector3.
    public Node GetNodeAt(Vector3 worldpoint)
    {

        int x = Mathf.RoundToInt((worldpoint.x - startingPoint.x) / DistanceBetweenNodes);
        int y = Mathf.RoundToInt((worldpoint.y - startingPoint.y) / DistanceBetweenNodes);
        int z = Mathf.RoundToInt((worldpoint.z - startingPoint.z) / DistanceBetweenNodes);

        if (IsOutsideGameArea(new Vector3(x, y, z)) == true)
        {
            print("Point is Outside of Game Area!");
            return null;
        }

        return NavGraph[x, y, z];

    }


    //Get the connected Node whether using the NodePosition or not it is Active
    public Node GetConnectedNode(Node node, NodePosition nodePos)
    {
        Vector3 gridPos = node.gridPoint;
        switch (nodePos)
        {
            case NodePosition.up:
                gridPos += new Vector3(0, 1, 0);
                break;
            case NodePosition.down:
                gridPos += new Vector3(0, -1, 0);
                break;
            case NodePosition.left:
                gridPos += new Vector3(-1, 0, 0);
                break;
            case NodePosition.right:
                gridPos += new Vector3(1, 0, 0);
                break;
            case NodePosition.forward:
                gridPos += new Vector3(0, 0, 1);
                break;
            case NodePosition.back:
                gridPos += new Vector3(0, 0, -1);
                break;
            default:
                break;
        }

       if(IsOutsideGameArea(gridPos)== true)
        {
            print("No Conneceted Object at Position:" + nodePos);
            return null;
        }
        return NavGraph[(int)gridPos.x, (int)gridPos.y,(int) gridPos.z];
        
    }


    ///Get Neighboring Nodes. Can add more neighbors if you update GetConnectedNode function and enum NodeePosition
    ///If you want to get only want to get active Nodes pass an argument as True
    public List<Node> GetNeighbors(Node node,bool insert_Active_Only = true)
    {
        List<Node> neighbors = new List<Node>();
        foreach (NodePosition np in Enum.GetValues(typeof(NodePosition)))
        {
            Node n = GetConnectedNode(node,np);
            if (n != null)
            {
                if(n.isActive == true || insert_Active_Only == false)
                {
                    neighbors.Add(n);
                }
            }
        }
        return neighbors;
    }


    //Get ClosestActive Node performs a uniform graph search until it finds an active node.
    public Node ClosestActiveNode(Vector3 start)
    {
        Node currentNode = GetNodeAt(start);
        if (currentNode.isActive == true)
        {
            return currentNode;
        }
        if (currentNode == null)
        {
            Vector3 NodeAtEdgeOfMap = getClosetNodetoEdge();
            return currentNode = GetNodeAt(NodeAtEdgeOfMap);
        }

        Queue<Node> frontier = new Queue<Node>();
        frontier.Enqueue(currentNode);

        Dictionary<Node, bool> Visited = new Dictionary<Node, bool>();
        Visited[currentNode] = true;

        while (frontier.Count > 0)
        {
            currentNode = frontier.Dequeue();

            if (currentNode.isActive == true)
            {
                return currentNode;
            }

            foreach (Node nextNode in GetNeighbors(currentNode, false))
            {
                if (!Visited.ContainsKey(nextNode))
                {
                    frontier.Enqueue(nextNode);
                    Visited[nextNode] = true;
                }
            }
        }
        // Returns Node[0,0,0] if it does not find any point that is active
        print("Something Went Wrong! Could Not Find a Point");
        return GetNodeAt(new Vector3(0, 0, 0));

    }


    //If Object goes beyond our Grid. Find a point closest inside grid. Not implemented yet
    private Vector3 getClosetNodetoEdge()
    {
        throw new NotImplementedException("Did Not Implement the function to Return a node when Target Goes outside of GameArea");
    }

    //Checks if Node is Outside the GameArea. Does not check if it is active or not!
    public bool IsOutsideGameArea(Vector3 point)
    {
        // Point must be GridPoint;
        int x = (int) point.x;
        int y = (int) point.y;
        int z = (int) point.z;
        if (x < 0 || y < 0 || z < 0 || x > NodeSizeX - 1  || y > NodeSizeY - 1  || z > NodeSizeZ - 1 )
        {
            print("Point is Outside of Game Area!");
            return true;
        }

        //else if (navgraph[x, y, z].isactive == false && includeactivenodes == true)
        //{
        //    return true;
        //}
        else
        {
            return false;
        }
            
    }


    //Heuristic is using the manhattan distance. Can be change for whatever you want
    public float Heuristic(Node a, Node b)
    {
        Vector3 aP = a.worldPoint;
        Vector3 bP = b.worldPoint;

        return Mathf.Abs(aP.x - bP.x) + Mathf.Abs(aP.y - bP.y) + Mathf.Abs(aP.z - aP.z);
    }

}

public class Node
{

    public static int ID;

    public float nodeNum;
    public float graphCost;
    public float Priority;
    public bool isActive;
    public Vector3 worldPoint;
    public Vector3 gridPoint;

    public Node(Vector3 _worldPoint, Vector3 _gridPoint)
    {
        worldPoint = _worldPoint;
        gridPoint = _gridPoint;
        graphCost = 0;
        nodeNum = ID;
        ID += 1;
    }
}

public class BinaryHeep: MonoBehaviour
{
    private Dictionary<Node, float> priority;
    private int count;
    private List<Node> _heap;

    public BinaryHeep()
    {
        _heap = new List<Node>();
        priority = new Dictionary<Node, float>();
        count = 0;
    }
    public Node Peek()
    {
        return _heap[0];
    }
    public bool isEmpty()
    {
        if(count == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public Node PopMin()
    {
        

        int lastNodeIndex = count - 1;
        int rootIndex = 0;

        Node temp = _heap[rootIndex];

        if (lastNodeIndex == 0)
        {
            _heap.RemoveAt(lastNodeIndex);
            count--;
            return temp;
        }
        

        swap(rootIndex, lastNodeIndex);
        _heap.RemoveAt(lastNodeIndex);
        count--;

        Heapify();

        return temp;
    }



    public void Add(Node addednode, float p)
    {
        _heap.Add(addednode);
        priority.Add(addednode, p);
        count++;

        int addednodeIndex = count - 1;
        int parentNodeIndex = getParentIndexOf(addednodeIndex);
        if (addednodeIndex < 1)
        {
            return;
        }

        Node parentNode = _heap[parentNodeIndex];

        while (true)
        {
            if (priority[addednode] < priority[parentNode])
            {
                swap(addednodeIndex, parentNodeIndex);

                addednodeIndex = parentNodeIndex;
                parentNodeIndex = getParentIndexOf(addednodeIndex);

                if (addednodeIndex == 0)
                {
                    break;
                }

                addednode = _heap[addednodeIndex];
                parentNode = _heap[parentNodeIndex];
            }
            else
            {
                break;
            }

        }
    }

    private void Heapify()
    {
        int currentIndex = 0;
        int leftChildIndex = getLeftChildIndexOf(currentIndex);
        int rightChildIndex = getRightChildIndexOf(currentIndex);

        Node parentNode = _heap[currentIndex];
        Node leftNode = _heap[leftChildIndex];
        Node rightNode = _heap[rightChildIndex];

        while (true)
        {
            if (leftNode == null)
            {
                break;
            }
            if (priority[parentNode] > priority[leftNode])
            {
                if (rightNode == null)
                {
                    swap(currentIndex, leftChildIndex);
                    break;
                }
                if (priority[leftNode] < priority[rightNode])
                {

                    swap(currentIndex, leftChildIndex);

                    currentIndex = leftChildIndex;
                    leftChildIndex = getLeftChildIndexOf(currentIndex);
                    rightChildIndex = getRightChildIndexOf(currentIndex);


                }
                else
                {
                    swap(currentIndex, rightChildIndex);

                    currentIndex = rightChildIndex;
                    leftChildIndex = getLeftChildIndexOf(currentIndex);
                    rightChildIndex = getRightChildIndexOf(currentIndex);

                }
                
            }
            else if (rightNode == null)
            {
                break;
            }
            else if (priority[parentNode] > priority[rightNode])
            {
                swap(currentIndex, rightChildIndex);

                currentIndex = rightChildIndex;
                leftChildIndex = getLeftChildIndexOf(currentIndex);
                rightChildIndex = getRightChildIndexOf(currentIndex);

            }
            else
            {
                break;
            }

            parentNode = assignNode(currentIndex);
            leftNode = assignNode(leftChildIndex);
            rightNode = assignNode(rightChildIndex);

        }

    }

    private Node assignNode(int index)
    {
        if (index <= count - 1 )
        {
            return _heap[index];
        }
        else
        {
            return null;
        }
    }

    private int getLeftChildIndexOf(int currentIndex)
    {
        return Mathf.FloorToInt(2 * currentIndex + 1);
    }
    private int getRightChildIndexOf(int currentIndex)
    {
        return Mathf.FloorToInt(2 * currentIndex + 2);
    }
    private int getParentIndexOf(int currentIndex)
    {
        return Mathf.FloorToInt(((currentIndex - 1) / 2));
    }

    private void swap(int currentNodeIndex, int parentORchildIndex)
    {
        Node temp = _heap[parentORchildIndex];
        _heap[parentORchildIndex] = _heap[currentNodeIndex];
        _heap[currentNodeIndex] = temp;

    }
}

