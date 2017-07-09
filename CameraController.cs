using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public Transform target;
	public Vector3 offset;

	public enum CameraState {Unattached, Follow, Cinema};
	public CameraState currentCameraState;

	// Use this for initialization
	void Start () {
		if (target != null) {
			currentCameraState = CameraState.Follow;
		}
	}

	// Update is called once per frame
	void LateUpdate () {

		if (currentCameraState == CameraState.Unattached){
			if(!target) {
				//check for possible local players
				GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
				foreach (GameObject player in players) {
					if (player.GetComponent<PlayerController> ()) {
						if (player.GetComponent<PlayerController> ().isLocalPlayer) {

							//found local player to follow
							currentCameraState = CameraState.Follow;
							target = player.transform;
						}
					}
				}
			}else{
				currentCameraState = CameraState.Follow;
			}
		}

		switch (currentCameraState) {
		case CameraState.Follow:
			if (target) {
				transform.position = target.position + offset;
				transform.LookAt (target);
			}
			break;
		default:
			break;

		}

	}
}

