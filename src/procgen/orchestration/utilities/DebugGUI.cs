using System.IO;
using Godot;

public partial class DebugGUI : Node
{
	private Vector2I oldSize;
	private Vector2I oldPos;
	private Window.ModeEnum oldMode;
	public static bool on { get; private set; } = true;

	[Export]
	public Control DebugOverlay { get; set; }

	public override void _Input(InputEvent @event)
	{
		if (@event is not InputEventKey keyEvent) return;
		if (!keyEvent.IsPressed()) return;
		switch (keyEvent.KeyLabel)
		{
			// Toggle debug GUI when pressing 1.
			case Key.Key1:
				on = !on;
				DebugOverlay.Visible = on;
				Input.SetMouseMode(Input.MouseModeEnum.Visible);
				break;
			// Take screenshot when pressing 2.
			case Key.Key2:
				oldPos = GetWindow().Position;
				oldMode = GetWindow().Mode;
				oldSize = GetWindow().Size;
				if (keyEvent.IsShiftPressed())
				{
					GetWindow().Size = new Vector2I(3840, 2160); //4K
				}

				RenderingServer.FramePostDraw += Screenshot;
				break;
		}
	}

	private void Screenshot()
	{
		RenderingServer.FramePostDraw -= Screenshot;
		var screenshotName = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "_Screenshot.png";
		GetViewport().GetTexture().GetImage().SavePng($"user://{screenshotName}");
		GD.Print($"Screenshot saved under: {Path.Join(OS.GetUserDataDir(), screenshotName)}");
		
		if (GetWindow().Size == oldSize) return;
		GetWindow().Size = oldSize;
		GetWindow().Position = oldPos;
		GetWindow().Mode = oldMode;
	}
}
