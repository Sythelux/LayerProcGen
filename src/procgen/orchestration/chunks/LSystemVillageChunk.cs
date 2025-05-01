using Runevision.Common;
using Runevision.LayerProcGen;
using Godot;
using System.Collections.Generic;
using System.Linq;
using Godot.Util;
using System;

public class LSystemVillageChunk : LayerChunk<LSystemVillageLayer, LSystemVillageChunk>
{
    List<Vector3> housePositions = new();
    Point gridOrigin;
    private Node3D? _chunkParent; 
    private readonly List<Node3D> _pendingHouses = new();

    const int GLOBAL_SEED = 12345; // TODO: make this configurable and/or random but stored in the database when finally hook this up to a backend.
    const int CHUNK_X_RANDOM = 73856093;
    const int CHUNK_Y_RANDOM = 19349663;
    const int LSYSTEM_ITERATIONS = 5;

    public override void Create(int level, bool destroy)
    {
        GD.Print("LSystemVillageChunk Create");
        if (destroy)
            housePositions.Clear();
        else
            Build();
    }

    public void DebugDraw() {
		GD.Print("LSystemVillageChunk DebugDraw");
	}

    Node3D GetChunkParent()
    {
        // Re-use it if we already made it
        if (_chunkParent != null && GodotObject.IsInstanceValid(_chunkParent))
            return _chunkParent;

        // ❶ Get the SceneTree – works from non-Node classes too
        var tree = (SceneTree)Engine.GetMainLoop();

        // ❷ Find the node you placed in the editor
        //     RootNode3D/Layers/LSystemVillage
        var villageRoot = tree.Root
            .GetNode<Node3D>("RootNode3D/Layers/LSystemVillage");

        // ❸ Make a node for *this* chunk the first time we need it
        _chunkParent = new Node3D
        {
            Name = $"Chunk_{index.x}_{index.y}"
        };
        villageRoot.AddChild(_chunkParent);

        return _chunkParent;
    }

    void Build()
    {
        gridOrigin = index * layer.chunkW;
        // Mix in chunk coords so each chunk seed varies
        int chunkSeed = GLOBAL_SEED
                    + index.x * CHUNK_X_RANDOM
                    + index.y * CHUNK_Y_RANDOM;
        
        var rnd = new Random(chunkSeed);

        // Generate your L-system string
        var rules = new StochasticRewriteTable(rnd).Build();

        var alphabet = rules.Keys.ToArray();

        // Build a 3‑char axiom with at least one 'B'
        var picks = Enumerable
            .Range(0, 3)
            .Select(_ => alphabet[rnd.Next(alphabet.Length)])
            .ToList();
        // ensure at least one 'B'
        if (!picks.Contains('B'))
            picks[rnd.Next(picks.Count)] = 'B';
        string axiom = string.Concat(picks);

        // Generate the L-system sequence
        var lSystem = new StochasticLSystem(axiom, rules, rnd);
        string lSequence = lSystem.Generate(LSYSTEM_ITERATIONS);

        GD.Print("L-System Sequence: " + lSequence);

        // Start interpreting at the chunk's origin
        var interpreter = new TurtleInterpreter(GetHeightAt);
        float spacingModifier = 3.75f; // Todo: consider using the actual cell size
        float jitterRange = 150f;
        // deterministic jitter
        float jitterX = (float)(rnd.NextDouble() * (2 * jitterRange) - jitterRange);
        float jitterZ = (float)(rnd.NextDouble() * (2 * jitterRange) - jitterRange);

        var worldOrigin = new Vector3(
            gridOrigin.x * spacingModifier + jitterX,
            0,
            gridOrigin.y * spacingModifier + jitterZ
        );
        interpreter.Interpret(
            lSequence,
            worldOrigin,
            Vector3.Forward,
            housePositions
        );

        GD.Print("House positions: " + string.Join(", ", housePositions));

        GD.Print("Layer parent node name: " + layer.layerParent.Name);

        // Now housePositions has the exact positions for houses in this chunk
        foreach (var pos in housePositions)
        {
            QueueHouseInstance(pos);
        }

        FlushHousesToScene();
    }

    float GetHeightAt(Vector3 position)
    {
        var coords2D = new Vector2(position.X, position.Z);
        return TerrainNoise.GetHeight(coords2D);
    }

    void QueueHouseInstance(Vector3 position)
    {
        position.Y = GetHeightAt(position);

        var houseScene = GD.Load<PackedScene>(
            "res://src/scenes/l_system_prefabs/house.tscn");
        var inst = houseScene.Instantiate<Node3D>();
        inst.Position = position;

        _pendingHouses.Add(inst); 
    }

    void FlushHousesToScene()
    {
        GD.Print("LSystemVillageChunk FlushHousesToScene");
        if (_pendingHouses.Count == 0) return;

        // Copy to Godot Array so it survives the lambda capture
        var batch = new Godot.Collections.Array<Node3D>(_pendingHouses.Cast<Node3D>());

        // Clear the list for the next build cycle
        _pendingHouses.Clear();

        // Schedule the whole batch to be added next frame
        SceneTree tree = (SceneTree)Engine.GetMainLoop();
        tree.CreateTimer(0)                           // 0-sec One-Shot Timer
            .Connect("timeout",
                Callable.From(() =>
                {
                    var parent = GetChunkParent();    // now definitely in tree
                    GD.Print("Adding " + batch.Count + " houses to scene for parent " + parent.Name);
                    foreach (Node3D houseNode in batch)
                    {
                        houseNode.Name = "House_" + houseNode.Position.ToString();
                        parent.AddChild(houseNode);
                    }
                }));
    }
}
