using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotor : MonoBehaviour {

	public float speed;
	public float dampPower;
	public float freeBodyDrag;
	public float dodgeSpeed;
	public float dodgeDuration;
	public float gravity;
	public float jumpPower;
	public int jumpTimes;
	public float ceilingDetectionDistance;
	public Collider2D bodyCollider;
	public Collider2D bottomCollider;
	public Action onDodgeExit;
	public Action onGroundEnter;
	public Action onGroundExit;

	public FaceDirection FaceDirection => _faceDirection;

	private Rigidbody2D _rigidbody;
	private bool _needsUpdate;
	private bool _grounded;
	private bool _dodging;
	private bool _moved;
	private int _jumpTimes;
	private int _bottomOverlapTimes;
	private float _velocityX;
	private float _freeVelocityX;
	private float _dodgeVelocity;
	private float _velocityY;
	private float _dodgeFinishTime = -1f;
	private FaceDirection _faceDirection = FaceDirection.Right;

	private void Awake() {
		_rigidbody = gameObject.AddComponent<Rigidbody2D>();
		_rigidbody.isKinematic = false;
		_rigidbody.gravityScale = 0;
		_rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
		_rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
		_rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		_rigidbody.bodyType = RigidbodyType2D.Dynamic;
	}

	private void FixedUpdate() => _needsUpdate = true;

	private void Update() {
		if (!_needsUpdate) return;
		_needsUpdate = false;

		if (_dodging) {
			float time = Time.time;
			if (time >= _dodgeFinishTime) {
				_dodging = false;
				onDodgeExit?.Invoke();
			} else _rigidbody.velocity = new Vector2(_dodgeVelocity, 0f);
		} else {
			if (!_grounded) {
				_velocityY -= gravity;
				if (Physics2D.Raycast(transform.position, Vector2.up, ceilingDetectionDistance, 1 << LayerManager.GroundLayer)) _velocityY = -1f;
			}

			if (!_moved) {
				if (_velocityX != 0f)
					if (_velocityX * (float) _faceDirection > dampPower) _velocityX += -dampPower * (float) _faceDirection;
					else _velocityX = 0f;
			} else _moved = false;
			
			if (_freeVelocityX != 0f) {
				int direction = _freeVelocityX > 0 ? 1 : -1;
				if (_freeVelocityX * direction > freeBodyDrag) _freeVelocityX += -freeBodyDrag * direction;
				else _freeVelocityX = 0f;
			}
		
			_rigidbody.velocity = new Vector2(_velocityX, _velocityY);
		}
	}

	private void OnGroundEnter() {
		_jumpTimes = 0;
		_velocityY = 0f;
		onGroundEnter?.Invoke();
	}

	private void OnGroundExit() {
		onGroundExit?.Invoke();
	}

	public void Move(float velocity) {
		_moved = true;
		_velocityX = velocity;
	}

	public void Move() => Move(speed * (float) _faceDirection);
	
	public void MoveLeft() {
		if (_faceDirection == FaceDirection.Right) Flip();
		Move();
	}

	public void MoveRight() {
		if (_faceDirection == FaceDirection.Left) Flip();
		Move();
	}

	public void Flip() {
		_faceDirection = (FaceDirection) (-1 * (int) _faceDirection);
		Vector3 scale = transform.localScale;
		scale.x *= -1;
		transform.localScale = scale;
	}
	
	public void Jump() {
		if (_jumpTimes == jumpTimes) return;
		_velocityY = jumpPower;
		_jumpTimes++;
	}

	public void Dodge() {
		if (_dodging) return;
		_dodging = true;
		_dodgeVelocity = dodgeSpeed * (float) _faceDirection;
		_dodgeFinishTime = Time.time + dodgeDuration;
	}
	
	public void GetHit(float velocityX, float velocityY) {
		_freeVelocityX += velocityX;
		_velocityY = velocityY;
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.layer != LayerManager.GroundLayer) return;
		_bottomOverlapTimes++;
		if (_bottomOverlapTimes == 1) {
			_grounded = true;
			OnGroundEnter();
		}
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (other.gameObject.layer != LayerManager.GroundLayer) return;
		_bottomOverlapTimes--;
		if (_bottomOverlapTimes == 0) {
			_grounded = false;
			OnGroundExit();
		}
	}
}

public enum FaceDirection {
	Left = -1, 
	Right = 1
}