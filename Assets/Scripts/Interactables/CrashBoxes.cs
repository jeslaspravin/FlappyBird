using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CrashBoxes : BasicItem
{

    public override void interact(GameObject interactor)
    {
        if(interactor.GetComponent<Player>())
        {
            Debug.Log("Game over");

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
