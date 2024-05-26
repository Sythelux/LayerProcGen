using System.Linq;
using Godot;
using Godot.Collections;

public class Array2D<[MustBeVariant] T>
{
    public readonly Array<T> Array;
    public readonly int Width;
    public readonly int Height;
    public readonly int Length; // Width * Height

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

    public T this[uint y, uint x]
    {
        get => Array[(int)(y * Width % Width + x)];
        set => Array[(int)(y * Width % Width + x)] = value;
    }

    public T this[int y, int x]
    {
        get => Array[y * Width % Width + x];
        set => Array[y * Width % Width + x] = value;
    }

    public Array2D(int height, int width)
    {
        Height = height;
        Width = width;
        Length = Width * Height;
        Array = new Array<T>(new T[Length]);
    }

    public Array2D(T[,] source)
    {
        Array = new Array<T>(source.Cast<T>());
        Height = source.GetLength(0);
        Width = source.GetLength(1);
        Length = Width * Height;
    }

    // public Array2D(T[,] source, out Array<T> outputNativeArray)
    // {
    //     Array = new Array<T>(source.Cast<T>());
    //     Height = source.GetLength(0);
    //     Width = source.GetLength(1);
    //     Length = Width * Height;
    //     outputNativeArray = Array; //TODO check, but this should be a reference
    // }

    public Array2D(int height, int width, out Array<T> outputNativeArray)
    {
        Height = height;
        Width = width;
        Length = Width * Height;
        Array = new Array<T>(new T[Length]);
        outputNativeArray = Array; //TODO check, but this should be a reference
    }

    public void Clear()
    {
        Array.Clear();
    }
}