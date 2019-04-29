using UnityEngine;

public static class LayerManager {
	public static readonly int GroundLayer = LayerMask.NameToLayer("Ground");
	public static readonly int EnemyLayer = LayerMask.NameToLayer("Enemy");
	public static readonly int PlayerLayer = LayerMask.NameToLayer("Player");
	public static readonly int WeaponLayer = LayerMask.NameToLayer("Weapon");
}

public static class AnimationManager {
	public const int IdleState = 0;
	public const int WalkState = 1;
	public const int JumpState = 2;
	public const int DodgeState = 3;
	public const int HurtState = 4;
	public const int DeathState = 5;
	public const int AttackState = 6;

	public const string State = "State";
	public const string Attack = "Attack";
	public const string WalkSpeed = "WalkSpeed";
}