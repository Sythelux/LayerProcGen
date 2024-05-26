using System.Linq;
using Godot;
using Godot.Collections;

public class Array3D<[MustBeVariant] T>
{
    public readonly Array<T> Array;
    public readonly int Width;
    public readonly int Height;
    public readonly int Depth;
    public readonly int Length; // Width * Height * Depth

    public T this[uint index]
    {
        get => Array[(int)index];
        set => Array[(int)index] = value;
    }

    public T this[int index]
    {
        get => Array[index];
        set => Array[index] = value;
    }

    public T this[uint z, uint y, uint x]
    {
        get => Array[(int)(z * Width * Height + y * Width + x)];
        set => Array[(int)(z * Width * Height + y * Width + x)] = value;
    }

    public T this[int z, int y, int x]
    {
        get => Array[z * Width * Height + y * Width + x];
        set => Array[z * Width * Height + y * Width + x] = value;
    }

    public Array3D(int depth, int height, int width)
    {
        Width = width;
        Height = height;
        Depth = depth;
        Length = Width * Height * Depth;
        Array = new Array<T>(new T[Length]);
    }

    public Array3D(T[,,] source)
    {
        Array = new Array<T>(source.Cast<T>());
        Depth = source.GetLength(0);
        Height = source.GetLength(1);
        Width = source.GetLength(2);
        Length = Width * Height * Depth;
    }

    public Array3D(int depth, int height, int width, out Array<T> outputNativeArray)
    {
        Depth = depth;
        Height = height;
        Width = width;
        Length = Width * Height * Depth;
        Array = new Array<T>(new T[Length]);
        outputNativeArray = Array; //TODO check, but this should be a reference
    }

    public void Clear()
    {
        Array.Clear();
        // UnsafeUtility.MemClear(Array, Length * UnsafeUtility.SizeOf<T>());
    }
}