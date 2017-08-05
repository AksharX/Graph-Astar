using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class Astar : MonoBehaviour {


    public NavGrid graph;
    public Transform target;

    private Dictionary<Node, float> cost_so_far;
    private Dictionary<Node, Node> came_from;
    private Vector3[] Path;

    private Vector3 lastPosition;

    private Coroutine astarCo;

    private Node updatedTarget;
    private Node updatedEnemy;


    // Use this for initialization
    void Start()
    {
        Node updatedTarget = graph.ClosestActiveNode(target.position);
        Node updatedEnemy = graph.ClosestActiveNode(transform.position);

        Coroutine astarCo = StartCoroutine(A_star_search(updatedEnemy, updatedTarget));

        lastPosition = target.position;
        
    }



    private void Update()
    {
        if (TargetPositionChanged())
        {
            FollowTarget();
        }
    }

    /// <summary>
    /// Will follow the target using A* implementaation
    /// </summary>
    private void FollowTarget()
    {

        if (astarCo != null)
        {
            StopCoroutine(astarCo);
        }
        
        updatedTarget = graph.ClosestActiveNode(target.position);
        updatedEnemy = graph.ClosestActiveNode(transform.position);

        astarCo = StartCoroutine(A_star_search(updatedEnemy, updatedTarget));

        lastPosition = target.position;
        
    }


    /// <summary>
    /// Tests weather the target Position Changed.
    /// </summary>
    private bool TargetPositionChanged()
    {
        if(target.position == lastPosition)
        {
            return false;
        }
        else
        {
            return true;
        }
    }


    /// <summary>
    /// Sets up a Courutine for A*. Courtuines continues after gothroughing each of it nieghbors
    /// After courtine is finished. Using the DoTween Libary animates the through the path.
    IEnumerator A_star_search(Node start, Node goal)
    {
        Node startingNode = start;
        Node goalNode = goal;
        
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
            yield return null;
        }
        getPath(goalNode, startingNode);
        print("Ending Co: " + Path[4]);
        transform.DOLocalPath(Path, 2, PathType.CatmullRom).SetEase(Ease.Linear).Play();
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


    //Returns a list of Vector3 that represent the path from the dictionary CameFrom
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
