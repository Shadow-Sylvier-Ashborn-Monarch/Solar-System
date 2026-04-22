using System;
using System.Collections.Generic;
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

    System.Random rand = new System.Random(); // ✅ FIXED placement

    int nextNodeId = 0;

    // Add node (auto ID)
    public int AddNode(NodeType type)
    {
        int id = nextNodeId++;
        nodes[id] = new NodeGene(id, type);
        return id;
    }

    // Add connection
    public void AddConnection(int from, int to, float weight, bool enabled = true)
    {
        connections.Add(new ConnectionGene(from, to, weight, enabled));
    }

    float Activate(float x)
    {
        return (float)Math.Tanh(x);
    }

    public float[] FeedForward(float[] inputs)
    {
        foreach (var node in nodes.Values)
            node.value = 0f;

        int i = 0;
        foreach (var node in nodes.Values)
        {
            if (node.type == NodeType.Input)
            {
                node.value = inputs[i];
                i++;
            }
        }

        foreach (var conn in connections)
        {
            if (!conn.enabled) continue;

            NodeGene from = nodes[conn.fromNode];
            NodeGene to = nodes[conn.toNode];

            to.value += from.value * conn.weight;
        }

        foreach (var node in nodes.Values)
        {
            if (node.type != NodeType.Input)
                node.value = Activate(node.value);
        }

        List<float> outputs = new List<float>();
        foreach (var node in nodes.Values)
        {
            if (node.type == NodeType.Output)
                outputs.Add(node.value);
        }

        return outputs.ToArray();
    }

    // ================= MUTATION =================

    public void MutateWeights(float chance = 0.8f, float strength = 0.5f)
    {
        foreach (var conn in connections)
        {
            if (UnityEngine.Random.value < chance)
            {
                conn.weight += UnityEngine.Random.Range(-strength, strength);
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

            if (a.type == NodeType.Output) continue;
            if (b.type == NodeType.Input) continue;
            if (a.id == b.id) continue;

            bool exists = connections.Exists(c => c.fromNode == a.id && c.toNode == b.id);
            if (exists) continue;

            AddConnection(a.id, b.id, UnityEngine.Random.Range(-1f, 1f));
            return;
        }
    }

    public void MutateAddNode()
    {
        if (connections.Count == 0) return;

        ConnectionGene conn = connections[rand.Next(connections.Count)];

        if (!conn.enabled) return;

        conn.enabled = false;

        int newNodeId = AddNode(NodeType.Hidden);

        AddConnection(conn.fromNode, newNodeId, 1f);
        AddConnection(newNodeId, conn.toNode, conn.weight);
    }

    public void Mutate()
    {
        MutateWeights();

        if (UnityEngine.Random.value < 0.3f)
            MutateAddConnection();

        if (UnityEngine.Random.value < 0.2f)
            MutateAddNode();
    }
}