using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedometerScript : MonoBehaviour {

	public float loLim = 0.005f; // level to fall to the low state 
	public float hiLim = 0.1f; // level to go to high state (and detect step) 
	public int steps = 0; // step counter - counts when comp state goes high private 
	bool stateH = false; // comparator state

	public float fHigh = 10.0f; // noise filter control - reduces frequencies above fHigh private 
	public float curAcc = 0f; // noise filter 
	public float fLow = 0.1f; // average gravity filter control - time constant about 1/fLow 
	float avgAcc = 0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void FixedUpdate(){ // filter input.acceleration using Lerp
		curAcc = Mathf.Lerp(curAcc, Input.acceleration.magnitude, Time.deltaTime * fHigh);
		avgAcc = Mathf.Lerp(avgAcc, Input.acceleration.magnitude, Time.deltaTime * fLow);
		float delta = curAcc-avgAcc; // gets the acceleration pulses
		if (!stateH){ // if state == low...
			if (delta>hiLim){ // only goes high if input > hiLim
				stateH = true; 
				steps++; // count step with each step up
				NewStepDetected ();
			} 
		} else { 
			if (delta<loLim){ // only goes low if input < loLim 
				stateH = false; 
			} 
		} 
	}

	void NewStepDetected(){
		//Do whatever you want with each step
	}
}
