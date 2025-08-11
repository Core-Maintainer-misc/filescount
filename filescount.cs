using Godot;
using System;

[Tool]
public class FileCountPlugin : EditorPlugin
{
	private Control fileCountDock;
	private Label fileCountLabel;

	private Control fileCountDock2;
	private Label fileCountLabel2;

	public override void _EnterTree()
	{
		// Check if the first dock already exists
		if (fileCountDock == null)
		{
			fileCountDock = new Control();
			fileCountDock.Name = "File Count - RES://";
			fileCountDock.RectSize = new Vector2(200, 100);

			var panelStyle = new StyleBoxFlat();
			panelStyle.BgColor = new Color(0.2f, 0.2f, 0.2f, 1);
			fileCountDock.AddThemeStyleboxOverride("panel", panelStyle);

			fileCountLabel = new Label();
			fileCountLabel.Text = "File Count in RES://: 0";
			fileCountLabel.RectSize = new Vector2(200, 50);
			fileCountDock.AddChild(fileCountLabel);

			AddControlToBottomPanel(fileCountDock, "File Count - RES://");
		}

		// Check if the second dock already exists
		if (fileCountDock2 == null)
		{
			fileCountDock2 = new Control();
			fileCountDock2.Name = "File Count - USER://";
			fileCountDock2.RectSize = new Vector2(200, 100);

			var panelStyle2 = new StyleBoxFlat();
			panelStyle2.BgColor = new Color(0.2f, 0.2f, 0.2f, 1);
			fileCountDock2.AddThemeStyleboxOverride("panel2", panelStyle2);

			fileCountLabel2 = new Label();
			fileCountLabel2.Text = "File Count in USER://: 0";
			fileCountLabel2.RectSize = new Vector2(200, 50);
			fileCountDock2.AddChild(fileCountLabel2);

			AddControlToBottomPanel(fileCountDock2, "File Count - USER://");
		}

		UpdateFileCount();
	}

	public override void _ExitTree()
	{
		fileCountDock?.QueueFree();
		fileCountDock2?.QueueFree();
	}

	private void UpdateFileCount()
	{
		var counts = CountFilesInProject();
		fileCountLabel.Text = $"File Count in RES://: {counts[0]}";
		fileCountLabel2.Text = $"File Count in USER://: {counts[1]}";
	}

	private int[] CountFilesInProject()
	{
		const string path = "res://";
		const string path2 = "user://";
		int count = 0;
		int count2 = 0;

		var dir = DirAccess.Open(path);
		var dir2 = DirAccess.Open(path2);

		if (dir == null)
		{
			GD.Print("Error: Could not open directory 'res://'.");
			return new int[] { count, count2 };
		}

		GD.Print("Successfully opened directory 'res://'.");

		if (dir2 == null)
		{
			GD.Print("Error: Could not open user directory 'user://'.");
			return new int[] { count, count2 };
		}

		GD.Print("Successfully opened user directory 'user://'.");

		// Count files in RES://
		dir.ListDirBegin();
		string fileName = dir.GetNext();
		while (fileName != "")
		{
			if (!dir.CurrentIsDir())
			{
				count++;
				GD.Print($"Counting file in RES://: {fileName}");
			}
			else
			{
				GD.Print($"Skipping directory in RES://: {fileName}");
			}
			fileName = dir.GetNext();
		}
		dir.ListDirEnd();
		GD.Print($"Total files counted in RES://: {count}");

		// Count files in USER://
		dir2.ListDirBegin();
		string fileName2 = dir2.GetNext();
		while (fileName2 != "")
		{
			if (!dir2.CurrentIsDir())
			{
				count2++;
				GD.Print($"Counting file in USER://: {fileName2}");
			}
			else
			{
				GD.Print($"Skipping directory in USER://: {fileName2}");
			}
			fileName2 = dir2.GetNext();
		}
		dir2.ListDirEnd();
		GD.Print($"Total files counted in USER://: {count2}");

		return new int[] { count, count2 }; // Return both counts as an array
	}
}
