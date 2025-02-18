using UnityEngine;
using System.Collections.Generic;
using System;

public class PathFindingManager : SingletonManager<PathFindingManager>, IInitializable
{
    public bool IsInitialized { get; private set; }

    #region Constants
    private const int GRID_SIZE = 50;
    public const float NODE_SIZE = 1f;
    private const float GRID_VIEW_DISTANCE = 20f;
    private const int MAX_PATH_LENGTH = 100;
    private const int MAX_SEARCH_ITERATIONS = 1000;
    private const int INITIAL_POOL_SIZE = 20;
    #endregion

    #region Fields
    private Dictionary<Vector2Int, Node> activeNodes = new Dictionary<Vector2Int, Node>();
    private Camera mainCamera;
    private Vector2 previousCameraPosition;
    private Vector2 bottomLeft;
    private Vector2 gridWorldCenter;
    private List<Node> openSet;
    private HashSet<Node> closedSet;
    private Queue<PathFindingInstance> instancePool = new Queue<PathFindingInstance>();
    #endregion

    public void Initialize()
    {
        try
        {
            Debug.Log("Initializing PathFindingManager...");
            InitializePools();
            CreateGrid();
            IsInitialized = true;
            Debug.Log("PathFindingManager initialized successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error initializing PathFindingManager: {e.Message}");
            IsInitialized = false;
        }
    }

    #region Object Pooling
    private class PathFindingInstance
    {
        public List<Node> openSet = new List<Node>(1000);
        public HashSet<Node> closedSet = new HashSet<Node>();
        public List<Vector2> path = new List<Vector2>(MAX_PATH_LENGTH);
    }

    private void InitializePools()
    {
        for (int i = 0; i < INITIAL_POOL_SIZE; i++)
        {
            instancePool.Enqueue(new PathFindingInstance());
        }
    }

    private PathFindingInstance GetPathFindingInstance()
    {
        if (instancePool.Count == 0)
        {
            return new PathFindingInstance();
        }
        var instance = instancePool.Dequeue();
        instance.openSet.Clear();
        instance.closedSet.Clear();
        instance.path.Clear();
        return instance;
    }

    private void ReturnPathFindingInstance(PathFindingInstance instance)
    {
        instancePool.Enqueue(instance);
    }
    #endregion

    #region Grid Management
    private void CreateGrid()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found!");
            return;
        }

        UpdateGridCenter();
        InitializeNodes();
    }

    private void UpdateGridCenter()
    {
        if (mainCamera == null) return;

        gridWorldCenter = mainCamera.transform.position;
        bottomLeft = gridWorldCenter - new Vector2(GRID_SIZE * NODE_SIZE / 2, GRID_SIZE * NODE_SIZE / 2);
    }

    private void InitializeNodes()
    {
        Vector2 cameraPosition = mainCamera.transform.position;
        float height = 2f * mainCamera.orthographicSize;
        float width = height * mainCamera.aspect;

        int startX = Mathf.FloorToInt((cameraPosition.x - width / 2 - GRID_VIEW_DISTANCE - bottomLeft.x) / NODE_SIZE);
        int endX = Mathf.CeilToInt((cameraPosition.x + width / 2 + GRID_VIEW_DISTANCE - bottomLeft.x) / NODE_SIZE);
        int startY = Mathf.FloorToInt((cameraPosition.y - height / 2 - GRID_VIEW_DISTANCE - bottomLeft.y) / NODE_SIZE);
        int endY = Mathf.CeilToInt((cameraPosition.y + height / 2 + GRID_VIEW_DISTANCE - bottomLeft.y) / NODE_SIZE);

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                CreateNode(x, y);
            }
        }
    }

    private void Update()
    {
        if (mainCamera == null) return;

        Vector2 currentCameraPosition = mainCamera.transform.position;
        if (Vector2.Distance(currentCameraPosition, previousCameraPosition) > NODE_SIZE)
        {
            UpdateGrid(currentCameraPosition);
            previousCameraPosition = currentCameraPosition;
        }
    }
    #endregion

    #region Node Operations
    private void CreateNode(int x, int y)
    {
        Vector2 worldPosition = bottomLeft + new Vector2(x * NODE_SIZE, y * NODE_SIZE);
        bool isWalkable = !Physics2D.OverlapCircle(worldPosition, NODE_SIZE * 0.4f, LayerMask.GetMask("Obstacle"));
        activeNodes[new Vector2Int(x, y)] = new Node(isWalkable, worldPosition, x, y);
    }

    private void UpdateGrid(Vector2 cameraPosition)
    {
        bottomLeft = cameraPosition - new Vector2(GRID_SIZE * NODE_SIZE / 2, GRID_SIZE * NODE_SIZE / 2);

        float height = 2f * mainCamera.orthographicSize;
        float width = height * mainCamera.aspect;

        int startX = Mathf.FloorToInt((cameraPosition.x - width / 2 - GRID_VIEW_DISTANCE - bottomLeft.x) / NODE_SIZE);
        int endX = Mathf.CeilToInt((cameraPosition.x + width / 2 + GRID_VIEW_DISTANCE - bottomLeft.x) / NODE_SIZE);
        int startY = Mathf.FloorToInt((cameraPosition.y - height / 2 - GRID_VIEW_DISTANCE - bottomLeft.y) / NODE_SIZE);
        int endY = Mathf.CeilToInt((cameraPosition.y + height / 2 + GRID_VIEW_DISTANCE - bottomLeft.y) / NODE_SIZE);

        HashSet<Vector2Int> currentVisibleNodes = new HashSet<Vector2Int>();

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                currentVisibleNodes.Add(gridPos);

                if (!activeNodes.ContainsKey(gridPos))
                {
                    CreateNode(x, y);
                }
                else
                {
                    UpdateNode(x, y);
                }
            }
        }

        List<Vector2Int> nodesToRemove = new List<Vector2Int>();
        foreach (var node in activeNodes)
        {
            if (!currentVisibleNodes.Contains(node.Key))
            {
                nodesToRemove.Add(node.Key);
            }
        }

        foreach (var pos in nodesToRemove)
        {
            activeNodes.Remove(pos);
        }

        UpdateEnemyColliders(currentVisibleNodes);
    }

    private void UpdateNode(int x, int y)
    {
        Vector2Int gridPos = new Vector2Int(x, y);
        if (activeNodes.TryGetValue(gridPos, out Node node))
        {
            Vector2 worldPosition = bottomLeft + new Vector2(x * NODE_SIZE, y * NODE_SIZE);
            node.walkable = !Physics2D.OverlapCircle(worldPosition, NODE_SIZE * 0.4f, LayerMask.GetMask("Obstacle"));
            node.worldPosition = worldPosition;
        }
    }

    private void UpdateEnemyColliders(HashSet<Vector2Int> visibleNodes)
    {
        if (GameManager.Instance?.enemies == null) return;

        foreach (var enemy in GameManager.Instance.enemies)
        {
            Vector2Int enemyGridPos = WorldToGridPosition(enemy.transform.position);
            bool isInVisibleArea = visibleNodes.Contains(enemyGridPos);
            enemy.SetCollisionState(!isInVisibleArea);
        }
    }

    private Vector2Int WorldToGridPosition(Vector2 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition.x - bottomLeft.x) / NODE_SIZE);
        int y = Mathf.FloorToInt((worldPosition.y - bottomLeft.y) / NODE_SIZE);
        return new Vector2Int(x, y);
    }

    public Node GetNodeFromWorldPosition(Vector2 worldPosition)
    {
        Vector2Int gridPos = WorldToGridPosition(worldPosition);
        activeNodes.TryGetValue(gridPos, out Node node);
        return node;
    }

    private List<Node> GetNeighbours(Node node)
    {
        var neighbours = new List<Node>(8);

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                Vector2Int checkPos = new Vector2Int(node.gridX + x, node.gridY + y);
                if (activeNodes.TryGetValue(checkPos, out Node neighbour))
                {
                    neighbours.Add(neighbour);
                }
            }
        }

        return neighbours;
    }
    #endregion

    #region Pathfinding Core

    public List<Vector2> FindPath(Vector2 startPos, Vector2 targetPos)
    {
        var pathFindingInstance = GetPathFindingInstance();
        try
        {
            return ExecutePathfinding(startPos, targetPos, pathFindingInstance);
        }
        finally
        {
            ReturnPathFindingInstance(pathFindingInstance);
        }
    }

    private List<Vector2> ExecutePathfinding(Vector2 startPos, Vector2 targetPos, PathFindingInstance instance)
    {
        openSet = instance.openSet;
        closedSet = instance.closedSet;

        if (TryGetDirectPath(startPos, targetPos, out var directPath))
        {
            return directPath;
        }

        return PerformAStarPathfinding(startPos, targetPos);
    }

    private bool TryGetDirectPath(Vector2 startPos, Vector2 targetPos, out List<Vector2> directPath)
    {
        directPath = null;
        float distanceToTarget = Vector2.Distance(startPos, targetPos);

        if (distanceToTarget < NODE_SIZE * 2f && !Physics2D.Linecast(startPos, targetPos, LayerMask.GetMask("Obstacle")))
        {
            directPath = new List<Vector2> { startPos, targetPos };
            return true;
        }

        return false;
    }

    private List<Vector2> PerformAStarPathfinding(Vector2 startPos, Vector2 targetPos)
    {
        Node startNode = GetNodeFromWorldPosition(startPos);
        Node targetNode = GetNodeFromWorldPosition(targetPos);

        if (!ValidateNodes(ref startNode, ref targetNode))
        {
            return null;
        }

        InitializePathfindingNodes(startNode, targetNode);
        return ExecuteAStarAlgorithm(startNode, targetNode, startPos, targetPos);
    }

    private bool ValidateNodes(ref Node startNode, ref Node targetNode)
    {
        if (!startNode.walkable || !targetNode.walkable)
        {
            startNode = FindNearestWalkableNode(startNode);
            targetNode = FindNearestWalkableNode(targetNode);
            return startNode != null && targetNode != null;
        }
        return true;
    }

    private void InitializePathfindingNodes(Node startNode, Node targetNode)
    {
        foreach (var node in activeNodes.Values)
        {
            node.gCost = float.MaxValue;
            node.CalculateFCost();
            node.previousNode = null;
        }

        if (startNode != null)
        {
            startNode.gCost = 0;
            startNode.hCost = CalculateDistance(startNode, targetNode);
            startNode.CalculateFCost();
        }
    }

    private List<Vector2> ExecuteAStarAlgorithm(Node startNode, Node targetNode, Vector2 startPos, Vector2 targetPos)
    {
        int iterations = 0;
        openSet.Add(startNode);

        while (openSet.Count > 0 && iterations < MAX_SEARCH_ITERATIONS)
        {
            iterations++;
            Node currentNode = GetLowestFCostNode(openSet);

            if (currentNode == targetNode)
            {
                var path = CalculatePath(targetNode);
                if (path != null && path.Count > 0)
                {
                    path[0] = startPos;
                    path[path.Count - 1] = targetPos;
                    return OptimizePath(path);
                }
                return null;
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            foreach (Node neighbour in GetNeighbours(currentNode))
            {
                if (closedSet.Contains(neighbour)) continue;
                if (!neighbour.walkable)
                {
                    closedSet.Add(neighbour);
                    continue;
                }

                float tentativeGCost = currentNode.gCost + CalculateDistance(currentNode, neighbour);
                if (tentativeGCost < neighbour.gCost)
                {
                    neighbour.previousNode = currentNode;
                    neighbour.gCost = tentativeGCost;
                    neighbour.hCost = CalculateDistance(neighbour, targetNode);
                    neighbour.CalculateFCost();

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }

        return new List<Vector2> { startPos, targetPos };
    }

    #endregion

    #region Path Optimization & Smoothing
    private List<Vector2> OptimizePath(List<Vector2> path)
    {
        if (path.Count <= 2) return path;

        var optimizedPath = new List<Vector2>(50);
        optimizedPath.Add(path[0]);

        int i = 0;
        while (i < path.Count - 2)
        {
            i = FindFurthestVisibleNode(path, i, optimizedPath);
        }

        if (i != path.Count - 1)
        {
            optimizedPath.Add(path[path.Count - 1]);
        }

        path.Clear();
        return optimizedPath;
    }

    private int FindFurthestVisibleNode(List<Vector2> path, int currentIndex, List<Vector2> optimizedPath)
    {
        Vector2 current = path[currentIndex];
        int furthestVisible = currentIndex + 1;

        for (int j = currentIndex + 2; j < path.Count; j++)
        {
            if (IsNodeVisible(current, path[j]))
            {
                furthestVisible = j;
            }
            else break;
        }

        optimizedPath.Add(path[furthestVisible]);
        return furthestVisible;
    }

    private bool IsNodeVisible(Vector2 from, Vector2 to)
    {
        bool hasObstacle = Physics2D.Linecast(from, to, LayerMask.GetMask("Obstacle"));
        return !hasObstacle && CheckPathClearance(from, to);
    }
    #endregion

    #region Node Operations
    private float CalculateDistance(Node a, Node b)
    {
        float dx = Mathf.Abs(a.gridX - b.gridX);
        float dy = Mathf.Abs(a.gridY - b.gridY);

        return (dx + dy) + (1.4f - 2) * Mathf.Min(dx, dy);
    }

    private Node GetLowestFCostNode(List<Node> nodeList)
    {
        Node lowestFCostNode = nodeList[0];
        for (int i = 1; i < nodeList.Count; i++)
        {
            if (nodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = nodeList[i];
            }
        }
        return lowestFCostNode;
    }

    private List<Vector2> CalculatePath(Node endNode)
    {
        List<Vector2> path = new List<Vector2>(MAX_PATH_LENGTH);
        Node currentNode = endNode;
        int pathLength = 0;

        while (currentNode != null && pathLength < MAX_PATH_LENGTH)
        {
            path.Add(currentNode.worldPosition);
            currentNode = currentNode.previousNode;
            pathLength++;
        }

        if (pathLength >= MAX_PATH_LENGTH)
        {
            path = path.GetRange(0, MAX_PATH_LENGTH);
        }

        path.Reverse();
        return path;
    }

    private Node FindNearestWalkableNode(Node node)
    {
        if (node.walkable) return node;

        Queue<Node> openNodes = new Queue<Node>();
        HashSet<Node> visitedNodes = new HashSet<Node>();
        openNodes.Enqueue(node);
        visitedNodes.Add(node);

        while (openNodes.Count > 0)
        {
            Node currentNode = openNodes.Dequeue();

            foreach (Node neighbor in GetNeighbours(currentNode))
            {
                if (!visitedNodes.Contains(neighbor))
                {
                    if (neighbor.walkable)
                    {
                        return neighbor;
                    }
                    openNodes.Enqueue(neighbor);
                    visitedNodes.Add(neighbor);
                }
            }
        }
        return null;
    }

    private bool CheckPathClearance(Vector2 start, Vector2 end)
    {
        Vector2 direction = (end - start).normalized;
        Vector2 perpendicular = Vector2.Perpendicular(direction) * (NODE_SIZE * 0.3f);

        bool leftClear = !Physics2D.Linecast(start + perpendicular, end + perpendicular,
            LayerMask.GetMask("Obstacle"));
        bool rightClear = !Physics2D.Linecast(start - perpendicular, end - perpendicular,
            LayerMask.GetMask("Obstacle"));

        return leftClear && rightClear;
    }
    #endregion

    #region Visualization
    private void OnDrawGizmos()
    {
        if (mainCamera == null) return;

        Gizmos.color = new Color(1, 1, 0, 0.2f);
        Gizmos.DrawWireCube(gridWorldCenter, new Vector3(GRID_SIZE * NODE_SIZE, GRID_SIZE * NODE_SIZE, 1));

        foreach (var node in activeNodes.Values)
        {
            Gizmos.color = node.walkable ?
                new Color(1, 1, 1, 0.1f) :
                new Color(1, 0, 0, 0.2f);

            Gizmos.DrawCube(node.worldPosition, Vector3.one * NODE_SIZE * 0.8f);
        }

        var enemies = FindObjectsOfType<Enemy>();
        foreach (var enemy in enemies)
        {
            if (enemy?.currentPath != null && enemy.currentPath.Count > 0)
            {
                var path = enemy.currentPath;

                Gizmos.color = new Color(1, 0, 1, 1f);
                Gizmos.DrawLine(enemy.transform.position, path[0]);

                for (int i = 0; i < path.Count - 1; i++)
                {
                    Gizmos.color = new Color(0, 0, 1, 1f);
                    Gizmos.DrawLine(path[i], path[i + 1]);

                    Gizmos.color = new Color(1, 1, 0, 1f);
                    Gizmos.DrawWireSphere(path[i], NODE_SIZE * 0.3f);
                }

                if (path.Count > 0)
                {
                    Gizmos.color = new Color(0, 1, 0, 1f);
                    Gizmos.DrawWireSphere(path[path.Count - 1], NODE_SIZE * 0.4f);
                }
            }
        }
    }
    #endregion

    #region Grid Boundary
    public bool IsPositionInGrid(Vector2 position)
    {
        Vector2Int gridPos = WorldToGridPosition(position);
        return gridPos.x >= 0 && gridPos.x < GRID_SIZE &&
               gridPos.y >= 0 && gridPos.y < GRID_SIZE;
    }

    public Vector2 ClampToGrid(Vector2 position)
    {
        Vector2 gridCenter = mainCamera.transform.position;
        float halfGridSize = (GRID_SIZE * NODE_SIZE) / 2;

        float minX = gridCenter.x - halfGridSize;
        float maxX = gridCenter.x + halfGridSize;
        float minY = gridCenter.y - halfGridSize;
        float maxY = gridCenter.y + halfGridSize;

        return new Vector2(
            Mathf.Clamp(position.x, minX, maxX),
            Mathf.Clamp(position.y, minY, maxY)
        );
    }
    #endregion

    public void InitializeWithNewCamera()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found in PathFindingManager initialization!");
            return;
        }

        UpdateGridCenter();
        InitializeNodes();
        previousCameraPosition = mainCamera.transform.position;
    }
}