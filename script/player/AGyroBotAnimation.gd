extends Node

var is_animating: bool

func _ready():
	is_animating = false
	
func punch_hook():
	is_animating = true
	#print("punch")

	get_parent().start_limit_SL(0.5)
	get_parent().start_limit_SR(0.5)
	get_parent().start_limit_UL(0.99)
	get_parent().start_limit_UR(0.99)
	get_parent().start_limit_DL(0.99)
	get_parent().start_limit_DR(0.99)
	
	await get_tree().create_timer(0.3).timeout
	
	get_parent().start_limit_SL(0.25)
	
	await get_tree().create_timer(0.3).timeout
	
	get_parent().start_limit_UL(0.00)
	get_parent().start_limit_DL(0.00)
	
	#get_parent().set_add_rotate(0.00)
	
	
	await get_tree().create_timer(0.7).timeout
	
	#print("punch END")
	is_animating = false

func punch_jab():
	is_animating = true
	#print("punch")

	get_parent().start_limit_SL(0.70)
	get_parent().start_limit_SR(0.40)
	get_parent().start_limit_UL(0.20)
	get_parent().start_limit_UR(0.20)
	get_parent().start_limit_DL(0.99)
	get_parent().start_limit_DR(0.99)
	
	await get_tree().create_timer(0.30).timeout
	
	get_parent().start_limit_SL(0.25)
	get_parent().start_limit_SR(0.70)
	get_parent().start_limit_UL(0.00)
	
	await get_tree().create_timer(0.25).timeout
	
	get_parent().start_limit_DL(0.09)
	
	await get_tree().create_timer(0.7).timeout
	
	#print("punch END")
	is_animating = false

func aim():
	get_parent().start_limit_SL(0.40)
	get_parent().start_limit_SR(0.40)
	get_parent().start_limit_UL(0.20)
	get_parent().start_limit_UR(0.20)
	get_parent().start_limit_DL(0.99)
	get_parent().start_limit_DR(0.99)
