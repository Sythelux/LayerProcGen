using Godot;

public partial class DebugGUI : Node {
	public static bool on { get; private set; }
	
	public Control debugOverlay;

	public override void _Process(double delta)
	{
		// Toggle debug GUI when pressing 1.
		if (Input.IsKeyPressed(Key.Key1)) {
			on = !on;
			debugOverlay.Visible = on;
			Input.SetMouseMode(Input.MouseModeEnum.Visible);
		}
		// Take screenshot when pressing 2.
		if (Input.IsKeyPressed(Key.Key2))
		{
			RenderingServer.FramePostDraw += Screenshot;
		}
	}

	private void Screenshot()
	{
		RenderingServer.FramePostDraw -= Screenshot;
		GetViewport().GetTexture().GetImage().SavePng("user://"+System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "_Screenshot.png");
	}
}
