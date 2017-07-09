using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SyncPosition : NetworkBehaviour {

	[SyncVar (hook = "SyncPositionValues")]
	private Vector3 syncPos;
	private float threshold = 0.01f;

	private float speed;
	private float normalInterpolationRate = 0.5f;
	private float fasterInterpolationRate = 2;

	private List<Vector3> syncPosList = new List<Vector3>();
	private float closeToListPos = 0.25f;

	[SerializeField]
	private bool useLagPrevention = true;

	void Start()
	{
		speed = normalInterpolationRate;
	}

	void FixedUpdate () {
		//make sure server has updated position
		if (isLocalPlayer) {
			if (Vector3.Distance(transform.position, syncPos) > threshold)
			{
				syncPos = transform.position;
				TransmitPosition();
			}
		}
		else
		{
			UpdatePosition();
		}

	}

	/// <summary>
	/// Called every physics frame and updates player position based on sync var for position set accross whole network by server
	/// </summary>

	void UpdatePosition()
	{
		//only update position for players that aren't ourselves
		if (!isLocalPlayer)
		{
			if (useLagPrevention)
			{
				//store list of locations to make smooth movement, and keep non local players in the past
				if(syncPosList.Count > 0)
				{
					transform.position = Vector3.Lerp(transform.position, syncPosList[0], Time.deltaTime * speed);
					if(Vector3.Distance(transform.position, syncPosList[0]) < closeToListPos)
					{
						syncPosList.RemoveAt(0);
					}

					if (syncPosList.Count > 10)
					{
						speed = fasterInterpolationRate;
					}else
					{
						speed = normalInterpolationRate;
					}
				}


			}else
			{
				//lerp normally as lag isn't a problem
				if (Vector3.Distance(transform.position, syncPos) > 25.0f)
				{
					transform.position = syncPos;
					return;
				}

				float adjustedSpeed = (Vector3.Distance(transform.position, syncPos)) / speed;

				transform.position = Vector3.Lerp(transform.position, syncPos, Time.deltaTime * adjustedSpeed);
			}

		}
	}

	/// <summary>
	/// Create command for server to update our position on clients only
	/// </summary>

	void TransmitPosition()
	{
		CmdUpdateMyPositionOnServer(transform.position);
	}

	/// <summary>
	/// Run this command on server only to update our position across the network
	/// </summary>

	[Command]
	void CmdUpdateMyPositionOnServer(Vector3 pos)
	{
		//give server most recent location
		syncPos = pos;

		//server will now update syncvar for pos on all clients, using hook makes syncPos get set in SyncPositionValues
	}

	[Client]
	void SyncPositionValues(Vector3 latestPos)
	{
		syncPos = latestPos;
		syncPosList.Add(syncPos);
	}



}
