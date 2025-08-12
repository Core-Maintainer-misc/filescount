// Written by Andrew Poženel - 2025

using Godot;

[Tool]
public class FileCountPlugin : EditorPlugin
{
	private const string DockNameRes = "File Count - RES://";
	private const string DockNameUser = "File Count - USER://";
	private const string LabelTextRes = "File Count in RES://: 0";
	private const string LabelTextUser = "File Count in USER://: 0";
	private static readonly Vector2 DockSize = new Vector2(200, 100);
	private static readonly Vector2 LabelSize = new Vector2(200, 50);
	private static readonly Color PanelColor = new Color(0.2f, 0.2f, 0.2f, 1);

	[Export]
	public float UpdateTimerTime = 10.0f;

	private Control fileCountDock;
	private Label fileCountLabel;

	private Control fileCountDock2;
	private Label fileCountLabel2;

	private Timer updateTimer;

	public override void _EnterTree()
	{
		fileCountDock = CreateDock(DockNameRes, LabelTextRes);
		fileCountDock2 = CreateDock(DockNameUser, LabelTextUser);

		updateTimer = new Timer();
		updateTimer.WaitTime = UpdateTimerTime;
		updateTimer.Connect("timeout", new Callable(this, nameof(OnUpdateTimerTimeout)));
		AddChild(updateTimer);
		updateTimer.Start();

		UpdateFileCount();
	}

	public override void _ExitTree()
	{
		if (updateTimer != null)
		{
			updateTimer.Stop();
			updateTimer.QueueFree();
			updateTimer = null;
		}

		RemoveControlFromBottomPanel(fileCountDock);
		RemoveControlFromBottomPanel(fileCountDock2);
		fileCountDock.QueueFree();
		fileCountDock2.QueueFree();
		fileCountDock = null;
		fileCountDock2 = null;
	}

	private Control CreateDock(string name, string labelText)
	{
		var dock = new Control();
		dock.Name = name;
		dock.SetSize(DockSize);

		var panelStyle = new StyleBoxFlat();
		panelStyle.BgColor = PanelColor;
		dock.AddThemeStyleboxOverride("panel", panelStyle);

		var label = new Label();
		label.Text = labelText;
		label.SetSize(LabelSize);
		dock.AddChild(label);

		AddControlToBottomPanel(dock, name);

		if (name == DockNameRes)
		{
			fileCountLabel = label;
		}
		else
		{
			fileCountLabel2 = label;
		}

		return dock;
	}

	private void UpdateFileCount()
	{
		var counts = CountFilesInProject();
		fileCountLabel.Text = $"File Count in RES://: {counts[0]}";
		fileCountLabel2.Text = $"File Count in USER://: {counts[1]}";
	}

	private void OnUpdateTimerTimeout()
	{
		UpdateFileCount();
	}

	private int CountFilesInDirectory(string path)
	{
		var dir = DirAccess.Open(path);
		if (dir == null)
		{
			GD.PushError($"Error: Could not open directory '{path}'.");
			return 0;
		}

		var count = 0;
		dir.ListDirBegin();
		var fileName = dir.GetNext();
		while (fileName != "")
		{
			if (dir.CurrentIsDir())
			{
				if (fileName != "." && fileName != "..")
				{
					count += CountFilesInDirectory(path + fileName + "/");
				}
			}
			else
			{
				count += 1;
			}
			fileName = dir.GetNext();
		}
		dir.ListDirEnd();
		return count;
	}

	private int[] CountFilesInProject()
	{
		var resCount = CountFilesInDirectory("res://");
		var userCount = CountFilesInDirectory("user://");
		return new int[] { resCount, userCount };
	}
}
