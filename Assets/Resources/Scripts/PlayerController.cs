using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour {
	
	public KeyCode leftKey = KeyCode.A;
	public KeyCode rightKey = KeyCode.D;
	public KeyCode jumpKey = KeyCode.W;
	public KeyCode attackKey = KeyCode.J;
	public KeyCode dodgeKey = KeyCode.K;
	public Transform topWaterDetectionPoint;
	public Transform bottomWaterDetectionPoint;
	public LevelGenerator levelGenerator;

	public GameObject waterQuadA;
	public GameObject waterQuadB;
	
	public float originalSpeed;
	public float originalDamage;
	public float originalHealth;

	public float health;
	public float damage;
	
	public List<PropertyEffect> speedEffects = new List<PropertyEffect>();
	public List<PropertyEffect> damageEffects = new List<PropertyEffect>();

	public int State {
		get => _state;
		set {
			_state = value;
			_animator.SetInteger(AnimationManager.State, _state);
		}
	}

	private PlayerMotor _motor;
	private Animator _animator;
	private bool _inControlled = true;
	[SerializeField]
	private int _inWater;
	[SerializeField]
	private bool _attacking;
	[SerializeField]
	private bool _grounded;
	private bool _dodging;
	private bool _hurt;
	private bool _died;

	[SerializeField]
	private int _state;
	[SerializeField]
	private int _lastState;

	private void Awake() {
		_motor = GetComponent<PlayerMotor>();
		_motor.onDodgeExit += OnDodgeExit;
		_motor.onGroundEnter += OnGroundEnter;
		_motor.onGroundExit += OnGroundExit;

		_animator = GetComponentInChildren<Animator>();

		foreach (var renderer in GetComponentsInChildren<Renderer>()) renderer.sortingLayerName = "Player";
	}

	private void OnEnable() {
		State = 0;
		_inWater = 0;
		OnWaterRefresh();
	}

	private void Update() {
		if (!_inControlled) return;
		
		UpdateWaterEffect();

		_lastState = State;

		bool hasMoved = false;
		if (Input.GetKey(leftKey)) {
			if (!_hurt && !_died && !_dodging && !_attacking) {
				if (_grounded) {
					State = AnimationManager.WalkState;
					hasMoved = true;
				}
				
				_motor.MoveLeft();
			}
		} else if (Input.GetKey(rightKey)) {
			if (!_hurt && !_died && !_dodging && !_attacking) {
				if (_grounded) {
					State = AnimationManager.WalkState;
					hasMoved = true;
				}
				
				_motor.MoveRight();
			}
		}

		if (Input.GetKeyDown(jumpKey)) {
			if (!_hurt && !_died && !_dodging && !_attacking) {
				State = AnimationManager.JumpState;
				_motor.Jump();
				hasMoved = true;
			}
		}

		if (Input.GetKeyDown(dodgeKey)) {
			if (_inWater == 0 && !_hurt && !_died && !_dodging && !_attacking && _grounded) {
				State = AnimationManager.DodgeState;
				OnDodgeEnter();
				hasMoved = true;
			}
		}

		if (Input.GetKeyDown(attackKey)) {
			if (!_hurt && !_died && !_dodging && !_attacking) {
				State = 6;
				OnAttackEnter();
				hasMoved = true;
			}
		}
		
		if (!hasMoved && !_hurt && !_died && !_dodging && !_attacking) State = AnimationManager.IdleState;
	}

	private void UpdateWaterEffect() {
		Tilemap tilemap = levelGenerator.tilemap;
		TileBase tile = tilemap.GetTile(tilemap.WorldToCell(topWaterDetectionPoint.position));
		bool inWater = tile is WaterTile;
		if (inWater) {
			if (_inWater != 2) {
				_inWater = 2;
				OnWaterRefresh();
			}
		} else if (!inWater) {
			inWater = tilemap.GetTile(tilemap.WorldToCell(bottomWaterDetectionPoint.position)) is WaterTile;
			if (inWater) {
				if (_inWater != 1) {
					_inWater = 1;
					OnWaterRefresh();
				}
			} else {
				if (_inWater != 0) {
					_inWater = 0;
					OnWaterRefresh();
				}
			}
		}
	}

	private void OnAttackEnter() {
		_attacking = true;
	}
	
	public void OnAttackExit() {
		_attacking = false;
		State = !_grounded ? AnimationManager.JumpState : AnimationManager.IdleState;
	}

	private void OnDodgeEnter() {
		_motor.Dodge();
		_inControlled = false;
		_dodging = true;
	}

	private void OnDodgeExit() {
		_inControlled = true;
		_dodging = false;
	}

	private void OnWaterRefresh() {
		switch (_inWater) {
			case 0: {
				waterQuadA.SetActive(false);
				waterQuadB.SetActive(false);
				break;
			}
			case 1: {
				waterQuadA.SetActive(false);
				waterQuadB.SetActive(true);
				waterQuadB.transform.position = levelGenerator.tilemap.GetCellCenterWorld(levelGenerator.tilemap.WorldToCell(bottomWaterDetectionPoint.position)) + levelGenerator.tilemap.cellSize / 2f;
				break;
			}
			case 2: {
				waterQuadA.SetActive(true);
				waterQuadB.SetActive(true);
				var position = levelGenerator.tilemap.WorldToCell(topWaterDetectionPoint.position);
				waterQuadA.transform.position = levelGenerator.tilemap.GetCellCenterWorld(position) + levelGenerator.tilemap.cellSize / 2f;
				position.y -= 1;
				waterQuadB.transform.position = levelGenerator.tilemap.GetCellCenterWorld(position) + levelGenerator.tilemap.cellSize / 2f;
				break;
			}
		}
	}

	private void OnGroundEnter() {
		_grounded = true;
	}

	private void OnGroundExit() {
		_grounded = false;
	}
}

public class PropertyEffect {

	public PropertyEffectType type;
	public float value;
	
	public enum PropertyEffectType {
		Add,
		Multiply
	}
}