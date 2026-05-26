using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

public class NetworkVisualizer : MonoBehaviour
{
    public Creatures Creature; // Reference to the brain you want to see
    public NeuralNetwork network;
    public float nodeRadius = 0.3f;
    public Vector2 dimensions = new Vector2(10, 6);

    // Store positions so we don't recalculate every frame
    private Dictionary<int, Vector2> nodePositions = new Dictionary<int, Vector2>();

    void Update()
    {
        if (Creature == null)
        {
            network = CreatureSystem.Creatures[0].GetComponent<Creatures>().Brain;
            if (network == null) return;
            CalculateNodePositions();
            
        }
        else if (Creature != null)
        {
            network = Creature.Brain;
            CalculateNodePositions();
        }



    }
    public void CalculateNodePositions()
    {
        if (network == null) return;
        nodePositions.Clear();

        var nodes = network.nodes.Values.ToList();
        var inputs = nodes.Where(n => n.type == NodeType.Input).ToList();
        var outputs = nodes.Where(n => n.type == NodeType.Output).ToList();
        var hidden = nodes.Where(n => n.type == NodeType.Hidden).ToList();

        // 1. Position Inputs (Left)
        for (int i = 0; i < inputs.Count; i++)
        {
            float y = (inputs.Count > 1) ? (float)i / (inputs.Count - 1) : 0.5f;
            nodePositions[inputs[i].id] = new Vector2(0, y * dimensions.y);
        }

        // 2. Position Outputs (Right)
        for (int i = 0; i < outputs.Count; i++)
        {
            float y = (outputs.Count > 1) ? (float)i / (outputs.Count - 1) : 0.5f;
            nodePositions[outputs[i].id] = new Vector2(dimensions.x, y * dimensions.y);
        }

        // 3. Position Hidden (Middle - based on "flow" depth)
        foreach (var hNode in hidden)
        {
            float depth = GetNodeDepth(hNode.id);
            // We randomize the Y slightly or use a counter to prevent overlapping
            float randomY = UnityEngine.Random.Range(0, dimensions.y);
            nodePositions[hNode.id] = new Vector2(depth * (dimensions.x / 5f), randomY);
        }
    }

    private float GetNodeDepth(int nodeId)
    {
        // Simple heuristic: how many connections exist between this and the input
        int depth = 0;
        var currentId = nodeId;

        // This is a simplified version; for complex NEAT, 
        // you'd use the topological rank calculated during FeedForward.
        foreach (var conn in network.connections)
        {
            if (conn.toNode == nodeId && conn.enabled) depth++;
        }
        return Mathf.Clamp(depth, 1, 4);
    }

    private void OnDrawGizmos()
    {
        if (network == null || nodePositions.Count == 0) return;

        // Draw Connections
        foreach (var conn in network.connections)
        {
            if (!conn.enabled) continue;

            Gizmos.color = conn.weight > 0 ? Color.blue : Color.red;
            // Thickness can be simulated by drawing multiple lines or using a custom shader
            Gizmos.DrawLine(transform.position + (Vector3)nodePositions[conn.fromNode],
                            transform.position + (Vector3)nodePositions[conn.toNode]);
        }

        // Draw Nodes
        foreach (var node in network.nodes.Values)
        {
            Gizmos.color = Color.Lerp(Color.gray, Color.yellow, node.value);
            Vector3 pos = transform.position + (Vector3)nodePositions[node.id];
            Gizmos.DrawSphere(pos, nodeRadius);
        }
    }
}