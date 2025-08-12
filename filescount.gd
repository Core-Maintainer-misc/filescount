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

@export var update_timer_time: float = 10.0

func _enter_tree():

	file_count_dock = create_dock("File Count - RES://", "File Count in RES://: 0")
	file_count_dock2 = create_dock("File Count - USER://", "File Count in USER://: 0")


	update_timer = Timer.new()
	update_timer.wait_time = update_timer_time
	update_timer.connect("timeout", Callable(self, "_on_update_timer_timeout"))
	add_child(update_timer)
	update_timer.start()

	update_file_count()

func _exit_tree():
	if update_timer:
		update_timer.stop()
		update_timer.queue_free()
		update_timer = null

	remove_control_from_bottom_panel(file_count_dock)
	remove_control_from_bottom_panel(file_count_dock2)
	file_count_dock.queue_free()
	file_count_dock2.queue_free()
	file_count_dock = null
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


	if name == "File Count - RES://":
		file_count_label = label
	else:
		file_count_label2 = label

	return dock

func update_file_count():
	var counts = count_files_in_project()
	file_count_label.text = "File Count in RES://: %d" % counts[0]
	file_count_label2.text = "File Count in USER://: %d" % counts[1]

func _on_update_timer_timeout():
	update_file_count()

func count_files_in_directory(path: String) -> int:
	var dir = DirAccess.open(path)
	if dir == null:
		push_error("Error: Could not open directory '%s'." % path)
		return 0

	var count = 0
	dir.list_dir_begin()
	var file_name = dir.get_next()
	while file_name != "":
		if dir.current_is_dir():
			if file_name != "." and file_name != "..":
				count += count_files_in_directory(path + file_name + "/")
		else:
			count += 1
		file_name = dir.get_next()
	dir.list_dir_end()
	return count

func count_files_in_project() -> Array:
	var res_count = count_files_in_directory("res://")
	var user_count = count_files_in_directory("user://")
	return [res_count, user_count]
