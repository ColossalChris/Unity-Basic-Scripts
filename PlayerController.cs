using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {

	//overall state
	public enum PlayerState {Idle, Walking, Attacking, Shoving, Hit, Stunned, Dead};
	public PlayerState currentState;

	[SerializeField]
	Rigidbody rb;

	//movement
	public float walkSpeed = 5;
	private float horizontal;
	private float vertical;
	private float rotationDegreePerSecond = 20000;
	private bool collisionAhead;

	[SyncVar]
	private Vector3 syncPos;
	[SyncVar]
	private float syncRotY;

	//character selection
	public GameObject[] characters;
	public int currentChar = 0;

	//animator
	private Animator animator;
	private const string kState = "state";
	[SyncVar(hook = "SyncAnimStateValue")]
	private int syncAnimState = 0;

	// Use this for initialization
	void Start () {
		SelectCharacter (currentChar);
	}
	
	void Update()
	{
		if (animator && isLocalPlayer)
		{
			int speedOut = 0;

			if (currentState != PlayerState.Attacking && currentState != PlayerState.Stunned && currentState != PlayerState.Dead) {
				//check walking input
				horizontal = Input.GetAxis("Horizontal");
				vertical = Input.GetAxis("Vertical");

				Vector3 stickDirection = new Vector3(horizontal, 0, vertical);

				//normalize if needed
				if (stickDirection.sqrMagnitude > 1) stickDirection.Normalize();

				if (stickDirection.sqrMagnitude > 0.0f) {
					speedOut = 1;
				}

				if (stickDirection != Vector3.zero) {
					transform.rotation = Quaternion.RotateTowards (transform.rotation, Quaternion.LookRotation (stickDirection, Vector3.up), rotationDegreePerSecond * Time.deltaTime);
				}

				//todo check for barrier before moving forward
				if (!collisionAhead) {
					rb.velocity = transform.forward * speedOut * walkSpeed + new Vector3(0, GetComponent<Rigidbody>().velocity.y, 0);
					//transform.Translate(Vector3.forward * stickDirection.sqrMagnitude * walkSpeed * Time.deltaTime);
				}


			}

			//print ("Speed out is " + speedOut);

			//animate movement
			SetAnimationState(speedOut);


		}
	}

	/*
	 * Used to check if can move forward
	 */

	public void DetectCollision(bool newCollisionAhead){
		collisionAhead = newCollisionAhead;
	}

	/*
	 * Used when switching characters and at start of app to set default
	 */

	public void SelectCharacter(int i)
	{

		currentChar += i;

		if (currentChar > characters.Length - 1)
			currentChar = 0;
		if (currentChar < 0)
			currentChar = characters.Length - 1;

		foreach (GameObject child in characters)
		{
			if (child == characters[currentChar])
				child.SetActive(true);
			else
			{
				child.SetActive(false);

				if (child.GetComponent<triggerProjectile>())
					child.GetComponent<triggerProjectile>().clearProjectiles();
			}
		}

		animator = GetComponentInChildren<Animator>();
	}

	/*
	 * Used when controls change animation or external sources set animator like animations in the tree, enemy hits, etc
	 */

	public void SetAnimationState(int newAnimState){
		if (syncAnimState != newAnimState) {
			syncAnimState = newAnimState;

			//tell server we updated animator
			CmdUpdateAnimStateOnServer(syncAnimState);

			//update the actual animator
			animator.SetInteger (kState, newAnimState);
		}

	}

	[Command]
	void CmdUpdateAnimStateOnServer(int newAnimState){
		syncAnimState = newAnimState;
	}

	[Client]
	void SyncAnimStateValue(int newAnimState){
		if (!isLocalPlayer) {
			animator.SetInteger (kState, newAnimState);
		}
	}
}
