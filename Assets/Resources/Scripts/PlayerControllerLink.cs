using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerLink : MonoBehaviour {

	public PlayerController PlayerController { get; private set; }

	private void Awake() => PlayerController = GetComponentInParent<PlayerController>();
}
