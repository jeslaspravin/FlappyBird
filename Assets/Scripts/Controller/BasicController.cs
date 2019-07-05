using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicController : MonoBehaviour {

    protected BasicPawn controlledPawn;

    public float initFlySpeed;
    private float currentFlySpeed;

    public float minFlySpeed=2;
    public float maxFlySpeed = 8;

    private Vector3 currentMovementVelocity = Vector3.zero;

    private Vector3 currentPendingRotation = Vector3.zero;

    private int moveRefCount = 0, rotRefCount = 0;

    public bool alwaysFaceMovingDirection;
    public bool doNotNormalizeVelocity;

    public BasicPawn GetControlledPawn
    {
        get { return controlledPawn; }
    }

    public bool IsControllingPawn
    {
        get { return controlledPawn != null; }
    }

    public virtual void controlPawn(BasicPawn pawn)
    {
        if(pawn.controller!=null)
        {
            pawn.controller.releasePawn();
        }
        pawn.controller = this;
        controlledPawn = pawn;
    }

    public virtual void releasePawn()
    {
        // Reset the last set simulation velocity before releasing
        currentMovementVelocity = Vector3.zero;
        processMovement();

        controlledPawn.controller = null;
        controlledPawn = null;
    }

    // Use this for initialization
    public virtual void Start () {
        currentFlySpeed = initFlySpeed;
        GameState.state.onDifficultyChanged += onLevelVelocityChanged;
	}
	
	// Update is called once per frame
	public virtual void Update () {
        processMovement();
    }

    public virtual void OnDestroy()
    {
        if(GameState.state)
        GameState.state.onDifficultyChanged -= onLevelVelocityChanged;
    }

    public virtual float getMovementSpeed()
    {
        return currentFlySpeed;
    }

    public virtual void onLevelVelocityChanged(int levelDifficulty,float newVelocity)
    {
        currentFlySpeed = Mathf.Clamp(initFlySpeed * Mathf.Floor(newVelocity / initFlySpeed),minFlySpeed,maxFlySpeed);
    }

    public void processMovement()
    {
        if (controlledPawn != null && currentMovementVelocity.magnitude != 0.0f )
        {
            transform.position = controlledPawn.transform.position;
            Rigidbody2D rigidBody = controlledPawn.GetComponent<Rigidbody2D>();
            if(!doNotNormalizeVelocity)
                currentMovementVelocity.Normalize();
            rigidBody.velocity = currentMovementVelocity * getMovementSpeed() + new Vector3(GameState.state.CurrentVelocity,0,0);
            moveRefCount = 0;
            if (alwaysFaceMovingDirection)
            {
                float angleToRot = rigidBody.velocity.magnitude > 0 ? -90 + Vector3.SignedAngle(controlledPawn.transform.right, rigidBody.velocity, Vector3.forward)
                    : 0;
                controlledPawn.transform.Rotate(Vector3.forward, angleToRot);
                transform.Rotate(Vector3.forward, angleToRot);
            }
            else
            {
                transform.Rotate(currentPendingRotation, Space.Self);
                controlledPawn.transform.Rotate(Vector3.forward, currentPendingRotation.z);
            }
            currentMovementVelocity = Vector3.zero;
            rotRefCount = 0;
        }
    }

    public virtual void addMovementInput(Vector3 velocity)
    {
        if (moveRefCount == 0)
            currentMovementVelocity = velocity;
        else
        {
            currentMovementVelocity += velocity;
        }
        ++moveRefCount;
    }

    public virtual void addRotation(Vector3 rotation)
    {
        if (rotRefCount == 0)
            currentPendingRotation = rotation;
        else
        {
            currentPendingRotation += rotation;
        }
        ++rotRefCount;
    }
}
