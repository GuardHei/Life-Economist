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
	
	public int originalHealth;
	public int originalDamage;
	public float originalSpeed;

	public int health;
	public int damage;
	public int speed;
	
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
	private PlayerDamageTrigger _damageTrigger;
	private bool _inControlled = true;
	[SerializeField]
	private bool _inWater;
	private WaterTile _bottomWaterTile;
	private WaterTile _topWaterTile;
	private Vector3Int _topWaterCellPos;
	private Vector3Int _bottomWaterCellPos;
	[SerializeField]
	private bool _attacking;
	[SerializeField]
	private bool _grounded;
	[SerializeField]
	private bool _dodging;
	[SerializeField]
	private bool _hurt;
	[SerializeField]
	private bool _died;
	[SerializeField]
	private int _state;

	private void Awake() {
		_motor = GetComponent<PlayerMotor>();
		_motor.onDodgeExit += OnDodgeExit;
		_motor.onGroundEnter += OnGroundEnter;
		_motor.onGroundExit += OnGroundExit;

		_animator = GetComponentInChildren<Animator>();

		_damageTrigger = GetComponentInChildren<PlayerDamageTrigger>();

		foreach (var renderer in GetComponentsInChildren<Renderer>()) renderer.sortingLayerName = "Player";
	}

	private void OnEnable() {
		State = 0;
		_inWater = false;
		OnWaterRefresh();
	}

	private void Update() {
		if (!_inControlled) return;
		
		UpdateWaterEffect();

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
			if (!_inWater && !_hurt && !_died && !_dodging && !_attacking && _grounded) {
				State = AnimationManager.DodgeState;
				OnDodgeEnter();
				hasMoved = true;
			}
		}

		if (Input.GetKeyDown(attackKey)) {
			if (!_hurt && !_died && !_dodging && !_attacking) {
				State = AnimationManager.AttackState;
				OnAttackEnter();
				hasMoved = true;
			}
		}
		
		if (!hasMoved && !_hurt && !_died && !_dodging && !_attacking && _grounded) State = AnimationManager.IdleState;
	}

	private void UpdateWaterEffect() {
		Tilemap tilemap = levelGenerator.tilemap;
		Vector3Int topPos = tilemap.WorldToCell(topWaterDetectionPoint.position);
		Vector3Int bottomPos = tilemap.WorldToCell(bottomWaterDetectionPoint.position);
		WaterTile topTile = tilemap.GetTile(topPos) as WaterTile;
		WaterTile bottomTile = tilemap.GetTile(bottomPos) as WaterTile;

		if (!ReferenceEquals(bottomTile, null) || !ReferenceEquals(topTile, null)) {
			bool refresh = !_inWater || _topWaterCellPos != topPos || _bottomWaterCellPos != bottomPos;
			if (refresh) {
				_topWaterCellPos = topPos;
				_bottomWaterCellPos = bottomPos;
				_topWaterTile = topTile;
				_bottomWaterTile = bottomTile;
				OnWaterRefresh();
			}	
			
			_inWater = true;
		} else {
			bool refresh = _inWater || _topWaterCellPos != topPos || _bottomWaterCellPos != bottomPos;
			if (refresh) {
				_topWaterCellPos = topPos;
				_bottomWaterCellPos = bottomPos;
				_topWaterTile = topTile;
				_bottomWaterTile = bottomTile;
				OnWaterRefresh();
			}	
			
			_inWater = false;
		}
	}

	private void OnAttackEnter() {
		_attacking = true;
		_damageTrigger.Enable();
	}
	
	public void OnAttackExit() {
		_attacking = false;
		_damageTrigger.Disable();
		State = !_grounded ? AnimationManager.JumpState : AnimationManager.IdleState;
		_animator.Update(.05f);
	}

	private void OnDodgeEnter() {
		_motor.Dodge();
		_inControlled = false;
		_dodging = true;
	}

	private void OnDodgeExit() {
		_inControlled = true;
		_dodging = false;
		State = !_grounded ? AnimationManager.JumpState : AnimationManager.IdleState;
	}

	private void OnWaterRefresh() {
		Vector3 offset = levelGenerator.tilemap.cellSize / 2f;
		// offset.y = 0f;

		if (!ReferenceEquals(_topWaterTile, null)) {
			waterQuadA.SetActive(true);
			Vector3Int position = levelGenerator.tilemap.WorldToCell(topWaterDetectionPoint.position);
			// position.y += 1;
			waterQuadA.transform.position = levelGenerator.tilemap.CellToWorld(position) + offset;
		} else waterQuadA.SetActive(false);
		
		if (!ReferenceEquals(_bottomWaterTile, null)) {
			waterQuadB.SetActive(true);
			Vector3Int position = levelGenerator.tilemap.WorldToCell(bottomWaterDetectionPoint.position);
			// position.y += 1;
			waterQuadB.transform.position = levelGenerator.tilemap.CellToWorld(position) + offset;
		} else waterQuadB.SetActive(false);
	}

	private void OnGroundEnter() {
		_grounded = true;
	}

	private void OnGroundExit() {
		_grounded = false;
	}

	public void GetHurt(Vector2 hit, int damage) {
		if (_dodging || _died || _hurt) return;
		OnHurtEnter(hit, damage);
	}

	private void OnHurtEnter(Vector2 hit, int damage) {
		Debug.ClearDeveloperConsole();
		
		if (_attacking) {
			_attacking = false;
			_damageTrigger.Disable();
		}
		
		_hurt = true;
		State = AnimationManager.HurtState;
		_motor.GetHit(hit.x, hit.y);
	}

	public void OnHurtExit() {
		_hurt = false;
		State = !_grounded ? AnimationManager.JumpState : AnimationManager.IdleState;
		_animator.Update(.05f);
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