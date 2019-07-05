using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : BasicPawn {

    public Guid GetId
    {
        get { return ((PlayerController)controller).GetID; }
    }

	// Use this for initialization
	protected override void Start () {
        base.Start();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    /// <summary>
    /// Maps the input listeners to functions ,This method gets called from controller and get further processed in controller
    /// <seealso cref="PlayerController"/>
    /// </summary>
    /// <param name="inputMap">Map that will be filled with input events to listen to with method</param>
    public virtual void setupInputs(ref Dictionary<string,Action<float>> inputMap)
    {
        inputMap.Add("Flap",onFlap);
    }
    public virtual void setupTouches(ref Dictionary<string, Action<float>> inputMap)
    {
        inputMap.Add("0", onFlap);
    }

    void onFlap(float value)
    {
        if(value>0)
            ((PlayerController)controller).addMovementInput(Vector3.up* value);
    }
}
