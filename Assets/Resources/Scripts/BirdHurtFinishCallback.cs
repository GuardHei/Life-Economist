using UnityEngine;

public class BirdHurtFinishCallback : StateMachineBehaviour {
	
	private bool _exiting;
	
	private void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
		// Debug.Log(Time.frameCount + " On State Enter");
		_exiting = false;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		// Debug.Log(Time.frameCount + " On State Update");
		if (!_exiting && stateInfo.normalizedTime > .9f) {
			// Debug.Log(Time.frameCount + " On State Update Exit");
			BirdController controller = animator.GetComponentInParent<BirdController>();
			controller.OnHurtExit();
			_exiting = true;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		// Debug.Log(Time.frameCount + " On State Exit");
		_exiting = false;
	}
}