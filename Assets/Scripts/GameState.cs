using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public delegate void DifficultyIncreasedDelegate(int newDfficulty,float newVelocity);
    public static GameState state=null;

    // Unity Units
    public float playerInitVelocity=2;
    // In seconds
    public float diffStepFreq = 50;
    public float velocityStep = 1.5f;
    public float maxVelocity = 8.0f;

    private float timeFlew = 0;
    private int levelDifficulty = 0;
    private float distanceTravelled=0;
    private float currentVelocity;

    public float CurrentVelocity
    {
        get
        {
            return currentVelocity;
        }
    }
    public float TimeFlew
    {
        get
        {
            return timeFlew;
        }
    }

    public float DistanceTravelled
    {
        get
        {
            return distanceTravelled;
        }
    }

    // Prefab that will be used to spawn player
    public GameObject playerPrefab;
    // Prefab used to spawn player controller
    public GameObject playerControllerPrefab;

    public event DifficultyIncreasedDelegate onDifficultyChanged;

    [System.Serializable]
    public struct PlayerSpawnData
    {
        public string playerName;

        public Transform spawnTransform;
    }

    public PlayerSpawnData playerToSpawn;
    private Dictionary<Guid, BasicController> playersList = new Dictionary<Guid, BasicController>();

    void Awake()
    {
        if (state != null)
        {
            Destroy(this);
            return;
        }

        state = this;

    }

    // Start is called before the first frame update
    void Start()
    {     

        currentVelocity = playerInitVelocity;

        onDifficultyChanged?.Invoke(levelDifficulty, currentVelocity);
        spawnInPlayer();
    }

    void spawnInPlayer()
    {
        BasicPawn pawn = ((GameObject)Instantiate(playerPrefab, playerToSpawn.spawnTransform.position, Quaternion.identity)).GetComponent<BasicPawn>();
        if (!pawn)
            throw new Exception("Add proper Pawn Prefab in game manager");
        pawn.name = playerToSpawn.playerName;

        PlayerController controller = ((GameObject)Instantiate(playerControllerPrefab)).GetComponent<PlayerController>();
        if (!controller)
            throw new Exception("Add proper Pawn Controller Prefab in game manager");
        controller.name = playerToSpawn.playerName + "Controller";

        InputManager inputManager = new GameObject(controller.name + "InputManager").AddComponent<InputManager>();

        playersList.Add(controller.GetID, controller);

        controller.playerName = playerToSpawn.playerName;
        controller.controlPawn(pawn);
        controller.setupInputs(inputManager);
    }

    // Update is called once per frame
    void Update()
    {
        distanceTravelled += currentVelocity * Time.deltaTime;
        timeFlew += Time.deltaTime;
        if(Mathf.FloorToInt(timeFlew / diffStepFreq)>levelDifficulty)
        {
            levelDifficulty++;
            currentVelocity += velocityStep;
            currentVelocity = Mathf.Clamp(currentVelocity, playerInitVelocity, maxVelocity);
#if DBG
            Debug.Log("Increasing the difficulty new velocity : " + currentVelocity + " new difficulty level : " + levelDifficulty);
#endif
            onDifficultyChanged?.Invoke(levelDifficulty, currentVelocity);
        }
    }

    void OnDestroy()
    {
        state = null;
    }
}
