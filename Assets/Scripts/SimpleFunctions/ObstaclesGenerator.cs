using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclesGenerator : MonoBehaviour
{


    private Bounds cameraBounds;
    public GameObject pipePrefab;

    public int poolSize=10;
    // For hole position
    public float minVariation = 0.4f;
    public float maxVariation = 1.0f;
    // Hole Y extend(so half size)
    public float holeSize = 0.3f;

    // Distance gap between each pipes.
    public float spawnBetweenDistances=1.5f;
    // next spawn happens at distance traveled reaches this value
    private float nextSpawnAt=0;

    // Initial spawn delay
    public float firstSpawnDelay=5;

    // For Perlin noise 2nd coord
    public float minYValue;
    public float maxYValue;
    private float chosenCoord;

    // Can spawn
    private bool canSpawn = false;

    private Queue<GameObject> topPipesPool= new Queue<GameObject>();
    private Queue<GameObject> bottomPipesPool = new Queue<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        float sizeX = Camera.main.orthographicSize * Camera.main.aspect;
        cameraBounds = new Bounds(Camera.main.transform.position, new Vector3(sizeX*2, Camera.main.orthographicSize*2, 0));
        chosenCoord = Random.Range(minYValue, maxYValue);
        canSpawn = false;
        fillPool();
        Invoke("startSpawn", firstSpawnDelay);
    }

    void fillPool()
    {
        Vector2 camXExt = new Vector2(cameraBounds.center.x - cameraBounds.extents.x, cameraBounds.center.x + cameraBounds.extents.x);
        for (int i=0;i<poolSize; i++)
        {
            GameObject go = Instantiate(pipePrefab);
            go.SetActive(false);
            go.GetComponent<Pipes>().camXExtends = camXExt;
            topPipesPool.Enqueue(go);
        }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = Instantiate(pipePrefab);
            go.GetComponent<SpriteRenderer>().flipY = true;
            go.GetComponent<Collider2D>().offset = new Vector2(go.GetComponent<Collider2D>().offset.x,- go.GetComponent<Collider2D>().offset.y);
            go.GetComponent<Pipes>().camXExtends = camXExt;
            go.SetActive(false);
            bottomPipesPool.Enqueue(go);
        }
    }

    void startSpawn()
    {
        spawnObstacle();
        canSpawn = true;
    }

    void spawnObstacle()
    {
        if (topPipesPool.Count == 0 || bottomPipesPool.Count == 0)
            return;
        GameObject topGo = topPipesPool.Dequeue();
        GameObject bottomGo = bottomPipesPool.Dequeue();

        BoxCollider2D topCollider = topGo.GetComponent<BoxCollider2D>();
        SpriteRenderer topRenderer = topGo.GetComponent<SpriteRenderer>();
        BoxCollider2D bottomCollider = bottomGo.GetComponent<BoxCollider2D>();
        SpriteRenderer bottomRenderer = bottomGo.GetComponent<SpriteRenderer>();

        nextSpawnAt = GameState.state.DistanceTravelled + (topCollider.size.x / 2.0f) + spawnBetweenDistances;

        Vector2 holePosition = new Vector2(cameraBounds.center.x + cameraBounds.extents.x + topCollider.bounds.extents.x, 
            cameraBounds.extents.y * getYValue()+Random.Range(minVariation,maxVariation));

        topGo.transform.position = new Vector3(holePosition.x,holePosition.y-holeSize,topGo.transform.position.z);
        topGo.GetComponent<Pipes>().setVelocity(0, GameState.state.CurrentVelocity);
        bottomGo.GetComponent<Pipes>().setVelocity(0, GameState.state.CurrentVelocity);
        bottomGo.transform.position = new Vector3(holePosition.x, holePosition.y + holeSize, bottomGo.transform.position.z);

        topGo.GetComponent<Pipes>().onPipeOutofScreen +=onTopPipeOffScreen;
        bottomGo.GetComponent<Pipes>().onPipeOutofScreen +=onBottomPipeOffScreen;
        topGo.SetActive(true);
        bottomGo.SetActive(true);
    }

    float getYValue()
    {
        return 0.9f*Mathf.Clamp((Mathf.Sin(GameState.state.TimeFlew) * 0.5f) - 0.5f + Mathf.PerlinNoise(GameState.state.TimeFlew, 
            chosenCoord),-1.0f,1.0f);
    }

    void onTopPipeOffScreen(Pipes pipe)
    {
        pipe.onPipeOutofScreen -= onTopPipeOffScreen;
        pipe.gameObject.SetActive(false);
        topPipesPool.Enqueue(pipe.gameObject);
    }

    void onBottomPipeOffScreen(Pipes pipe)
    {
        pipe.onPipeOutofScreen -= onBottomPipeOffScreen;
        pipe.gameObject.SetActive(false);
        bottomPipesPool.Enqueue(pipe.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if(canSpawn && GameState.state.DistanceTravelled >= nextSpawnAt)
        {
            spawnObstacle();
        }
    }
}
