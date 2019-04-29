using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyDamageTrigger : MonoBehaviour {

	public Vector2 hit;
	public int damage;

	public bool Enabled => _collider.enabled;

	private Rigidbody2D _rigidbody;
	private Collider2D _collider;
	
	private void Awake() {
		_rigidbody = gameObject.AddComponent<Rigidbody2D>();
		_rigidbody.isKinematic = true;
		_rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

		_collider = GetComponent<Collider2D>();
		
		Disable();
	}

	public void Enable() {
		_collider.enabled = true;
	}

	public void Disable() {
		_collider.enabled = false;
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.layer == LayerManager.PlayerLayer) {
			PlayerController controller = other.GetComponent<PlayerController>();
			if (controller) {
				Vector2 hit = this.hit;
				if (other.transform.position.x - transform.position.x < 0) hit.x *= -1f;
				controller.GetHurt(hit, damage);
			}
		}
	}
}