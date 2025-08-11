# Written by Andrew Poženel - 2025

@tool
extends EditorPlugin

const DOCK_SIZE: Vector2 = Vector2(200, 100)
const LABEL_SIZE: Vector2 = Vector2(200, 50)
const PANEL_COLOR: Color = Color(0.2, 0.2, 0.2, 1)

var file_count_dock: Control
var file_count_label: Label

var file_count_dock2: Control
var file_count_label2: Label

var update_timer: Timer

func _enter_tree():
	# Only create docks if they don't already exist
	if file_count_dock == null:
		file_count_dock = create_dock("File Count - RES://", "File Count in RES://: 0")
	if file_count_dock2 == null:
		file_count_dock2 = create_dock("File Count - USER://", "File Count in USER://: 0")

	# Set up a timer for dynamic updates
	update_timer = Timer.new()
	update_timer.wait_time = 45.0
	update_timer.connect("timeout", Callable(self, "_on_update_timer_timeout"))
	add_child(update_timer)
	update_timer.start()

	update_file_count()

func _exit_tree():
	if update_timer:
		update_timer.stop()
		update_timer.queue_free()
		update_timer = null

	if file_count_dock:
		file_count_dock.queue_free()
		file_count_dock = null
	if file_count_dock2:
		file_count_dock2.queue_free()
		file_count_dock2 = null

func create_dock(name: String, label_text: String) -> Control:
	var dock = Control.new()
	dock.name = name
	dock.size = DOCK_SIZE

	var panel_style = StyleBoxFlat.new()
	panel_style.bg_color = PANEL_COLOR
	dock.add_theme_stylebox_override("panel", panel_style)

	var label = Label.new()
	label.text = label_text
	label.size = LABEL_SIZE
	dock.add_child(label)

	add_control_to_bottom_panel(dock, name)

	# Store the label reference for updating later
	if name == "File Count - RES://":
		file_count_label = label
	else:
		file_count_label2 = label

	print("Dock created: %s" % name)  # Debug print
	return dock

func update_file_count():
	var counts = count_files_in_project()
	file_count_label.text = "File Count in RES://: %d" % counts[0]
	file_count_label2.text = "File Count in USER://: %d" % counts[1]
	print("Updated file counts: RES:// = %d, USER:// = %d" % [counts[0], counts[1]])  # Debug print

func _on_update_timer_timeout():
	update_file_count()  # Update file counts dynamically

func count_files_in_project() -> Array:
	const path2: String = "user://"
	const path: String = "res://"
	var count: int = 0
	var count2: int = 0

	var dir = DirAccess.open(path)
	if dir == null:
		push_error("Error: Could not open directory 'res://'.")
		return [count, count2]

	var dir2 = DirAccess.open(path2)
	if dir2 == null:
		push_error("Error: Could not open user directory 'user://'.")
		return [count, count2]

	# Count files in RES://
	dir.list_dir_begin()
	var file_name = dir.get_next()
	while file_name != "":
		if not dir.current_is_dir():
			count += 1
			print("Counting file in RES://: %s" % file_name)  # Debug print
		file_name = dir.get_next()
	dir.list_dir_end()
	print("Total files counted in RES://: %d" % count)  # Debug print

	# Count files in USER://
	dir2.list_dir_begin()
	var file_name2 = dir2.get_next()
	while file_name2 != "":
		if not dir2.current_is_dir():
			count2 += 1
			print("Counting file in USER://: %s" % file_name2)  # Debug print
		file_name2 = dir2.get_next()
	dir2.list_dir_end()

	print("Total files counted in USER://: %d" % count2)  # Debug print

	return [count, count2]
