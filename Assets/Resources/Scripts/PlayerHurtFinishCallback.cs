using UnityEngine;

public class PlayerHurtFinishCallback : StateMachineBehaviour {
	
	private bool _exiting;
	
	private void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
		// Debug.Log(Time.frameCount + " On State Enter");
		_exiting = false;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		// Debug.Log(Time.frameCount + " On State Update");
		if (!_exiting && stateInfo.normalizedTime > .9f) {
			// Debug.Log(Time.frameCount + " On State Update Exit");
			PlayerController controller = animator.GetComponent<PlayerControllerLink>().PlayerController;
			controller.OnHurtExit();
			_exiting = true;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		// Debug.Log(Time.frameCount + " On State Exit");
		_exiting = false;
	}
}