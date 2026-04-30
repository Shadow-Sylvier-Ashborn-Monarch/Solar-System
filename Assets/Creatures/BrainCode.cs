using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum NodeType { Input, Hidden, Output }

[Serializable]
public class NodeGene
{
    public int id;
    public NodeType type;
    public float value;

    public NodeGene(int id, NodeType type)
    {
        this.id = id;
        this.type = type;
        this.value = 0f;
    }
}

[Serializable]
public class ConnectionGene
{
    public int fromNode;
    public int toNode;
    public float weight;
    public bool enabled;

    public ConnectionGene(int from, int to, float weight, bool enabled = true)
    {
        this.fromNode = from;
        this.toNode = to;
        this.weight = weight;
        this.enabled = enabled;
    }
}

public class NeuralNetwork
{
    public Dictionary<int, NodeGene> nodes = new Dictionary<int, NodeGene>();
    public List<ConnectionGene> connections = new List<ConnectionGene>();

    private System.Random rand = new System.Random();
    private int nextNodeId = 0;

    public int AddNode(NodeType type)
    {
        int id = nextNodeId++;
        nodes[id] = new NodeGene(id, type);
        return id;
    }

    public void AddConnection(int from, int to, float weight, bool enabled = true)
    {
        connections.Add(new ConnectionGene(from, to, weight, enabled));
    }

    float Activate(float x) => (float)Math.Tanh(x);

    // --- CRITICAL FIX: TOPOLOGICAL FEED FORWARD ---
    public float[] FeedForward(float[] inputs)
    {
        foreach (var node in nodes.Values) node.value = 0f;

    // 2. Map inputs specifically to Input-type nodes
    // We sort by ID to ensure the order is consistent every time
    var inputNodes = nodes.Values
        .Where(n => n.type == NodeType.Input)
        .OrderBy(n => n.id)
        .ToList();

    // ERROR CHECK: This prevents the Out of Bounds crash
    if (inputs.Length != inputNodes.Count)
    {
        Debug.LogError($"Input mismatch! Expected {inputNodes.Count} inputs, but got {inputs.Length}.");
        return new float[nodes.Values.Count(n => n.type == NodeType.Output)];
    }

    for (int i = 0; i < inputNodes.Count; i++)
    {
        inputNodes[i].value = inputs[i];
    }

        // 1. Assign Inputs
        int inputCounter = 0;
        //var inputNodes = nodes.Values.Where(n => n.type == NodeType.Input).ToList();
        for (int i = 0; i < inputNodes.Count && i < inputs.Length; i++)
        {
            inputNodes[i].value = inputs[i];
        }

        // 2. Get nodes in order of dependency (Topological Sort)
        List<NodeGene> sortedNodes = GetTopologicallySortedNodes();

        // 3. Process signal
        foreach (var node in sortedNodes)
        {
            if (node.type == NodeType.Input) continue;

            float sum = 0f;
            foreach (var conn in connections)
            {
                if (conn.enabled && conn.toNode == node.id)
                {
                    sum += nodes[conn.fromNode].value * conn.weight;
                }
            }
            node.value = Activate(sum);
        }

        // 4. Return Outputs
        return nodes.Values
            .Where(n => n.type == NodeType.Output)
            .OrderBy(n => n.id)
            .Select(n => n.value)
            .ToArray();
    }

    private List<NodeGene> GetTopologicallySortedNodes()
    {
        List<NodeGene> sorted = new List<NodeGene>();
        HashSet<int> visited = new HashSet<int>();
        
        void Visit(int nodeId)
        {
            if (visited.Contains(nodeId)) return;
            
            // Find all nodes that feed INTO this node
            foreach (var conn in connections)
            {
                if (conn.enabled && conn.toNode == nodeId)
                    Visit(conn.fromNode);
            }

            visited.Add(nodeId);
            sorted.Add(nodes[nodeId]);
        }

        foreach (var node in nodes.Values.Where(n => n.type == NodeType.Output))
        {
            Visit(node.id);
        }

        return sorted;
    }

    // --- MUTATION FIXES ---

 public void MutateWeights(float chance = 0.8f, float strength = 0.5f)
{
    foreach (var conn in connections)
    {
        if (UnityEngine.Random.value < chance)
        {
            // 90% chance to nudge the weight, 10% to reset it entirely
            if (UnityEngine.Random.value < 0.9f)
                conn.weight += UnityEngine.Random.Range(-strength, strength);
            else
                conn.weight = UnityEngine.Random.Range(-1f, 1f);
        }
    }
}
    public void MutateAddConnection()
    {
        var nodeList = new List<NodeGene>(nodes.Values);

        for (int attempts = 0; attempts < 20; attempts++)
        {
            NodeGene a = nodeList[rand.Next(nodeList.Count)];
            NodeGene b = nodeList[rand.Next(nodeList.Count)];

            if (a.id == b.id) continue;
            if (a.type == NodeType.Output || b.type == NodeType.Input) continue;

            if (connections.Exists(c => c.fromNode == a.id && c.toNode == b.id)) continue;
            
            // Prevent infinite loops in FeedForward
            if (WouldCreateCycle(a.id, b.id)) continue;

            AddConnection(a.id, b.id, UnityEngine.Random.Range(-1f, 1f));
            return;
        }
    }

    private bool WouldCreateCycle(int startNode, int endNode)
    {
        // Can endNode reach startNode?
        Queue<int> queue = new Queue<int>();
        queue.Enqueue(endNode);
        HashSet<int> visited = new HashSet<int>();

        while (queue.Count > 0)
        {
            int curr = queue.Dequeue();
            if (curr == startNode) return true;
            
            if (!visited.Add(curr)) continue;

            foreach (var conn in connections)
            {
                if (conn.enabled && conn.fromNode == curr)
                    queue.Enqueue(conn.toNode);
            }
        }
        return false;
    }

 public void MutateAddNode()
{
    var activeConns = connections.Where(c => c.enabled).ToList();
    if (activeConns.Count == 0) return;

    ConnectionGene conn = activeConns[rand.Next(activeConns.Count)];
    conn.enabled = false;

    int newNodeId = AddNode(NodeType.Hidden);

    // Tip: Use a small weight for the second connection to avoid
    // 'shocking' the network too much if Tanh is near its limit.
    AddConnection(conn.fromNode, newNodeId, 1f);
    AddConnection(newNodeId, conn.toNode, conn.weight);
}

public void RemoveConnection(int index)
{
    if (connections.Count > 0 && index >= 0 && index < connections.Count)
    {
        connections.RemoveAt(index);
    }
}

public void RemoveNode(int nodeId)
{
      if (nodes[nodeId].type != NodeType.Hidden) return;

    // Must remove connections using this node first!
    connections.RemoveAll(c => c.fromNode == nodeId || c.toNode == nodeId);
    nodes.Remove(nodeId);
    // 1. Check if it's a hidden node (Don't remove Input or Output nodes!)
    if (!nodes.ContainsKey(nodeId) || nodes[nodeId].type != NodeType.Hidden) 
        return;

    // 2. Remove all connections associated with this node
    // We iterate backwards to safely remove from the list while looping
    for (int i = connections.Count - 1; i >= 0; i--)
    {
        if (connections[i].fromNode == nodeId || connections[i].toNode == nodeId)
        {
            connections.RemoveAt(i);
        }
    }

    // 3. Remove the node itself
    nodes.Remove(nodeId);
}


// --- NEW MUTATION LOGIC ---

public void MutateRemoveConnection()
{
    if (connections.Count == 0) return;
    
    // Choose a random connection and delete it
    int index = rand.Next(connections.Count);
    RemoveConnection(index);
}

public void MutateRemoveNode()
{
    // Get a list of all hidden nodes
    var hiddenNodes = nodes.Values.Where(n => n.type == NodeType.Hidden).ToList();
    if (hiddenNodes.Count == 0) return;

    // Pick a random hidden node to delete
    int idToRemove = hiddenNodes[rand.Next(hiddenNodes.Count)].id;
    RemoveNode(idToRemove);
}

 public void Mutate()
{
    // Weight mutation (standard)
    MutateWeights(0.05f, 0.02f);

    // Addition mutations
    if (UnityEngine.Random.value < 0.005f) 
        MutateAddConnection();

    if (UnityEngine.Random.value < 0.003f) 
        MutateAddNode();

    // --- SUBTRACTION MUTATIONS (Pruning) ---
    // These should generally be less frequent than additions 
    // to allow the network time to explore new structures.
    
    if (UnityEngine.Random.value < 0.002f) 
        MutateRemoveConnection();

    if (UnityEngine.Random.value < 0.001f) 
        MutateRemoveNode();
}
}