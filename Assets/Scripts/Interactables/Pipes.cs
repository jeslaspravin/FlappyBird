using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pipes : CrashBoxes
{

    private Rigidbody2D rigidBody;


    private float velocity;
    private float boundX;
    private Vector2 cameraXExtends;// Since camera is stationary

    public float BirdVelocity
    {
        set
        {
            velocity = value;
        }
        get
        {
            return velocity;
        }
    }

    public delegate void PipeDelegate(Pipes pipe);

    public event PipeDelegate onPipeOutofScreen;

    public Vector2 camXExtends
    {
        set
        {
            cameraXExtends = value;
        }
        get
        {
            return cameraXExtends;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        boundX = GetComponent<SpriteRenderer>().sprite.bounds.extents.x;
        if (GameState.state)
            GameState.state.onDifficultyChanged += setVelocity;
    }

    private void OnDestroy()
    {
        if (GameState.state)
            GameState.state.onDifficultyChanged -= setVelocity;
    }

    public void setVelocity(int dificulty,float newVelocity)
    {
        velocity = -newVelocity;
    }

    // Update is called once per frame
    void Update()
    {
        if(this.gameObject.activeSelf)
        {
            rigidBody.velocity = new Vector3(velocity, 0, 0);

            float xPos = transform.position.x;
            if (xPos + boundX < cameraXExtends.x)// Only comparing X Max case.
            {
                onPipeOutofScreen?.Invoke(this);
            }
        }
    }
}
