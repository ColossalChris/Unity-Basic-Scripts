using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SyncRotation : NetworkBehaviour
{

	[SyncVar(hook = "SyncRotationValues")]
	private float syncRot;
	private float threshold = 2;

	private float speed;
	private float normalInterpolationRate = 18;
	private float fasterInterpolationRate = 32;

	private List<float> syncRotList = new List<float>();
	private float closeToListRot = 0.5f;

	[SerializeField]
	private bool useLagPrevention = true;

	void Start()
	{
		speed = normalInterpolationRate;
	}

	void FixedUpdate()
	{
		//make sure server has updated position
		if (isLocalPlayer) {
			if (Mathf.Abs(transform.rotation.eulerAngles.y - syncRot) > threshold)
			{
				//Debug.Log("Transmitting Rotation:" + transform.rotation.eulerAngles);
				syncRot = transform.rotation.eulerAngles.y;
				CmdUpdateMyRotationOnServer(transform.rotation.eulerAngles.y);
			}
		}
		else
		{
			UpdateRotation();
		}

	}

	/// <summary>
	/// Called every physics frame and updates player rotation based on sync var for rotation set accross whole network by server
	/// </summary>

	void UpdateRotation()
	{
		//only update rotation for players that aren't ourselves
		if (!isLocalPlayer)
		{
			if (useLagPrevention)
			{
				//store list of locations to make smooth movement, and keep non local players in the past
				if (syncRotList.Count > 0)
				{
					transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, syncRotList[0], 0), Time.deltaTime * speed);
					if (Mathf.Abs(transform.rotation.eulerAngles.y - syncRotList[0]) < closeToListRot)
					{
						syncRotList.RemoveAt(0);
					}

					if (syncRotList.Count > 10)
					{
						speed = fasterInterpolationRate;
					}
					else
					{
						speed = normalInterpolationRate;
					}
				}
			}
			else
			{
				//Debug.Log("Receiving Rotation:" + syncRot);

				transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, syncRot, 0), Time.deltaTime * speed);
			}

		}
	}

	/// <summary>
	/// Run this command on server only to update our rotation across the network
	/// </summary>

	[Command]
	void CmdUpdateMyRotationOnServer(float latestRot)
	{
		//give server most recent rotation in euler
		syncRot = latestRot;

		//server will now update syncvar for rot on all clients, using hook makes syncRot get set in SyncRotationValues
	}


	[Client]
	void SyncRotationValues(float latestRot)
	{
		syncRot = latestRot;

		if (useLagPrevention)
		{
			syncRotList.Add(syncRot);
		}

	}


}