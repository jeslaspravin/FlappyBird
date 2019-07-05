using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player's unique data
/// </summary>
/// <remarks>
/// Not used in any of implementations.
/// </remarks>
public struct PlayerUData
{
    public string playerName;
    public Guid guid;

    public PlayerUData(string name,Guid id)
    {
        playerName = name;
        guid=id;
    }
}

/// <summary>
/// Player Controller
/// </summary>
public class PlayerController : BasicController {

    /// <summary>
    /// Input manager of this controller.
    /// <para>Each player controller has one input manager</para>
    /// </summary>
    private InputManager inputMngr;
    public InputManager GetInputManager
    {
        get { return inputMngr; }
    }

    public string playerName;


    /// <summary>
    /// Unique ID of this player controller or player
    /// </summary>
    private Guid guid;

    public Guid GetID
    {
        get { return guid; }
    }

    public PlayerUData GetPlayerUniqueData
    {
        get { return new PlayerUData(playerName, guid); }
    }

    private void Awake()
    {
        guid = Guid.NewGuid();

    }

    // Use this for initialization
    public override void Start () {
        base.Start();
	}

    // Update is called once per frame
    public override void Update () {
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        inputMngr.onInputProcessed -= processMovement;
    }

    public override void controlPawn(BasicPawn pawn)
    {
        base.controlPawn(pawn);
    }

    public override void releasePawn()
    {
        if (inputMngr)
        {
            string guidStr = guid.ToString();
            inputMngr.stopListenerSet(guidStr);
            inputMngr.stopTouchSet(guidStr);
        }
        base.releasePawn();
    }

    /// <summary>
    /// Method that need to be called after taking control of pawn to start listening to controls of that pawn from input manager
    /// <para>will be useful in later stage of project</para>
    /// </summary>
    /// <param name="inputManager">Input manager to pass in</param>
    public void setupInputs(InputManager inputManager)
    {
        if (inputMngr == null)
            inputMngr = inputManager;
        inputMngr.onInputProcessed += processMovement;
        Dictionary<string, Action<float>> inputMap = new Dictionary<string, Action<float>>();
        ((Player)controlledPawn).setupInputs(ref inputMap);

        foreach (KeyValuePair<string, Action<float>> entry in inputMap)
        {
            inputMngr.addToListenerSet(guid.ToString(), entry.Key, entry.Value);
        }
        inputMap.Clear();
        ((Player)controlledPawn).setupTouches(ref inputMap);

        foreach (KeyValuePair<string, Action<float>> entry in inputMap)
        {
            inputMngr.addToTouchSet(guid.ToString(), entry.Key, entry.Value);
        }
    }

    public void setInputActive(bool isActive)
    {
        string guidStr = guid.ToString();
        if (isActive)
        {
            inputMngr.resumeListenerSet(guidStr);
            inputMngr.resumeTouchSet(guidStr);
        }
        else
        {
            inputMngr.pauseListenerSet(guidStr);
            inputMngr.pauseTouchSet(guidStr);
        }
    }


}
