using Godot;

public class TurtleState
{
    public Vector3 Position { get; set; }
    public Vector3 Direction { get; set; }

    public TurtleState(Vector3 position, Vector3 direction)
    {
        Position = position;
        Direction = direction.Normalized();
    }

    public TurtleState Clone()
    {
        return new TurtleState(Position, Direction);
    }
}
