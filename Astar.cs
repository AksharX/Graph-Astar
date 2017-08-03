using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Astar : MonoBehaviour {


    public NavGrid graph;
    private Dictionary<Node, float> cost_so_far;
    private Dictionary<Node, Node> came_from;
    private Vector3[] Path;

    public Transform target;
    
    // Use this for initialization
    void Start()
    {
        //GameObject obj = GameObject.FindGameObjectWithTag("Graph");
        //graph = obj.GetComponent<NavGrid>();

        A_star_search(transform.position, target.position);
        print(Path[0]);

        transform.DOLocalPath(Path, 3, PathType.CatmullRom).SetEase(Ease.Linear).Play();
        
    }
    
    private void A_star_search(Vector3 start, Vector3 goal)
    {
        
        Node startingNode = graph.GetNodeAt(start);
        Node goalNode = graph.GetNodeAt(goal);
        
        BinaryHeep frontier = new BinaryHeep();
        frontier.Add(startingNode, 0f);

        Node currentNode;

        came_from = new Dictionary<Node, Node>();
        cost_so_far = new Dictionary<Node, float>();

        cost_so_far.Add(frontier.Peek(), 0);

        while (!frontier.isEmpty())
        {
            
            currentNode = frontier.PopMin();
            
            if (currentNode == goalNode)
            {
                break;
            }
            List<Node> nieghbors = graph.GetNeighbors(currentNode);
            foreach (Node nextNode in nieghbors)
            {
                
                float newCost = CostSoFar(currentNode) + nextNode.graphCost;
                if(!cost_so_far.ContainsKey(nextNode) || newCost < CostSoFar(nextNode)) // 
                {
                    cost_so_far[nextNode] = newCost;
                    float priority = graph.Heuristic(goalNode, nextNode);
                    frontier.Add(nextNode, priority);
                    came_from[nextNode] = currentNode;
                }

            }
            
        }
        
        getPath(goalNode, startingNode);
        
    }

    private float CostSoFar(Node n)
    {
        if (cost_so_far.ContainsKey(n))
        {
            return cost_so_far[n];
        }
        else
        {
            return 0f;
        }
    }

    private void getPath(Node goalNode,Node startNode)
    {
        Node current = goalNode;
        List<Vector3> pathList = new List<Vector3>();

        pathList.Add(current.worldPoint);

        while(current != startNode)
        {
            current = came_from[current];
            pathList.Add(current.worldPoint);
        }
        pathList.Add(startNode.worldPoint);
        pathList.Reverse();

        Path = pathList.ToArray();
        
    }

}
