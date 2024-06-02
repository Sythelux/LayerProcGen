using Godot;
using System;

// ScriptableObject which can be called as a singleton.
// Loaded on demand. If called first time from a non-main-thread, it waits for the main thread.
public abstract partial class SingletonAsset<T> : Resource where T : Resource, new()
{
    static T s_Instance = null;
    static object loadLock = new object();
    public static T instance
    {
        get
        {
            if (s_Instance == null)
            {
                // CallbackHub.ExecuteOnMainThread(() => {
                lock (loadLock)
                {
                    try
                    {
                        if (s_Instance == null)
                        {
                            s_Instance = ResourceLoader.Load<T>(ResourceLoader.Exists($"res://Resources/{typeof(T).Name}.tres") 
                                ? $"res://Resources/{typeof(T).Name}.tres" 
                                : $"res://Resources/{typeof(T).Name}.res");
                            (s_Instance as SingletonAsset<T>).OnInitialize();
                        }
                    }
                    catch (Exception)
                    {
                        GD.PushError($"couldn't find resource: res://Resources/{typeof(T).Name}");
                    }
                }
                // });
                // while (s_Instance == null)
                // 	System.Threading.Thread.Sleep(1);
            }
            return s_Instance;
        }
    }

    protected virtual void OnInitialize() { }
}
