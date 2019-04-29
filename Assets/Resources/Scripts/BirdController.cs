using UnityEngine;

public class BirdController : MonoBehaviour {

	public LevelGenerator levelGenerator;
	public bool seesThroughWalls;
	public float speed = 10f;
	public float maxAlertRange;
	public float minAttackRange;
	public float attackWaitInterval = 3f;
	public float attackDecisionOffset = 1f;
	public float attackDuration = 1f;
	public float attackDistance = 20f;
	
	public int State {
		get => _state;
		set {
			_state = value;
			_animator.SetInteger(AnimationManager.State, _state);
		}
	}

	private Animator _animator;
	private EnemyDamageTrigger _damageTrigger;

	[SerializeField]
	private int _state;
	[SerializeField]
	private bool _hasTarget;
	[SerializeField]
	private bool _waiting;
	[SerializeField]
	private bool _decided;
	[SerializeField]
	private bool _attacking;
	[SerializeField]
	private bool _hurt;
	[SerializeField]
	private bool _died;
	private float _attackWaitFinishTime;
	private float _attackStartTime;
	private Vector3 _attackOriginalPosition;
	private Vector3 _attackPosition;

	private void Awake() {
		_animator = GetComponentInChildren<Animator>();
		_damageTrigger = GetComponentInChildren<EnemyDamageTrigger>();

		foreach (var renderer in GetComponentsInChildren<Renderer>()) renderer.sortingLayerName = "Enemy";

		State = 0;
	}

	private void Update() {
		if (_hurt || _died) return;
		PlayerController playerController = levelGenerator.playerController;
		Vector3 diff = playerController.transform.position - transform.position;
		float distance = diff.magnitude;
		if (!_hasTarget)
			if (distance < maxAlertRange) 
				if (seesThroughWalls || !Physics2D.Linecast(playerController.transform.position, transform.position, 1 << LayerManager.GroundLayer)) _hasTarget = true;

		if (_hasTarget) {
			Vector3 scale = transform.localScale;
			if (diff.x > 0f && scale.x < 0f) {
				scale.x = 1f;
				transform.localScale = scale;
			} else if (diff.x < 0f && scale.x > 0f) {
				scale.x = -1f;
				transform.localScale = scale;
			}
			
			if (!_waiting && !_attacking) {
				if (distance > minAttackRange) {
					State = AnimationManager.IdleState;
					transform.position = Vector3.MoveTowards(transform.position, playerController.transform.position, speed * Time.deltaTime);
				} else {
					_waiting = true;
					_attackWaitFinishTime = Time.time + attackWaitInterval;
					State = AnimationManager.IdleState;
				}
			} else {
				if (_waiting) {
					if (!_decided && Time.time >= _attackWaitFinishTime - attackDecisionOffset) {
						_decided = true;
						_attackPosition = transform.position + diff.normalized * attackDistance;
					}
						
					if (Time.time >= _attackWaitFinishTime) {
						_waiting = false;
						_decided = false;
						_attacking = true;
						_attackStartTime = Time.time;
						_attackOriginalPosition = transform.position;
						_damageTrigger.Enable();
						State = AnimationManager.AttackState;
					}
				} else if (_attacking) {
					transform.position = Vector3.Lerp(_attackOriginalPosition, _attackPosition, (Time.time - _attackStartTime) / attackDuration);
					if (Time.time >= _attackStartTime + attackDuration) {
						_attacking = false;
						_hasTarget = false;
						State = AnimationManager.IdleState;
						_damageTrigger.Disable();
					}
				} else State = AnimationManager.IdleState;
			}
		}
	}

	public void GetHurt(int damage) {
		if (!_died && !_hurt) OnHurtEnter(damage);
	}
	
	private void OnHurtEnter(int damage) {
		_hasTarget = false;
		_waiting = false;
		_decided = false;
		_attacking = false;
		_damageTrigger.Disable();
		
		_hurt = true;
		State = AnimationManager.HurtState;
	}

	public void OnHurtExit() {
		_hurt = false;
		State = AnimationManager.IdleState;
		_animator.Update(.05f);
	}
}