using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour {
	
	public KeyCode leftKey = KeyCode.A;
	public KeyCode rightKey = KeyCode.D;
	public KeyCode jumpKey = KeyCode.W;
	public KeyCode attackKey = KeyCode.J;
	public KeyCode dodgeKey = KeyCode.K;

	private PlayerMotor _motor;
	private bool _inControlled = true;
	private bool _invincible;

	private void Awake() {
		_motor = GetComponent<PlayerMotor>();
		_motor.onDodgeExit += OnDodgeExit;
	}

	private void Update() {
		if (!_inControlled) return;
		
		if (Input.GetKey(leftKey)) _motor.MoveLeft();
		else if (Input.GetKey(rightKey)) _motor.MoveRight();
		
		if (Input.GetKeyDown(jumpKey)) _motor.Jump();
		if (Input.GetKeyDown(dodgeKey)) OnDodgeEnter();
	}

	private void OnDodgeEnter() {
		_motor.Dodge();
		_inControlled = false;
		_invincible = true;
	}

	private void OnDodgeExit() {
		_inControlled = true;
		_invincible = false;
	}
}