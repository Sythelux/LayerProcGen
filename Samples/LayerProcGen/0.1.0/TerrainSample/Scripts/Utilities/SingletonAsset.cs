using Godot;

// ScriptableObject which can be called as a singleton.
// Loaded on demand. If called first time from a non-main-thread, it waits for the main thread.
public  abstract partial class SingletonAsset<T> : Resource where T : Resource, new() {
	static T s_Instance = null;
	static object loadLock = new object();
	public static T instance {
		get {
			if (s_Instance == null)
			{
				// CallbackHub.ExecuteOnMainThread(() => {
				lock (loadLock)
				{
					if (s_Instance == null)
					{
						s_Instance = ResourceLoader.Load<T>($"res://Resources/{typeof(T).Name}.tres") 
						             ?? ResourceLoader.Load<T>($"res://Resources/{typeof(T).Name}.res");
						(s_Instance as SingletonAsset<T>).OnInitialize();
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
