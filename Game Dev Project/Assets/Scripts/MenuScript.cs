using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{

    //janky bespoke main menu that doesn't use UI objects

    //Screen Element References
    public GameObject startBright, startDull, exitBright, exitDull; //UI elements
    public GameObject[] pathParts = new GameObject[7];
    public GameObject titleText, goldBubble;
    


    //logic references
    public bool startActive = true;
    public string nextScene;
    public float pathDelay;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //swap between active buttons
        //go from start to exit
        if (startActive && Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            startActive = false;

            startBright.SetActive(false);
            startDull.SetActive(true);
            exitBright.SetActive(true);
            exitDull.SetActive(false);
        }

        //go from exit to start
        if (!(startActive) && (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)))
        {
            startActive = true;

            startBright.SetActive(true);
            startDull.SetActive(false);
            exitBright.SetActive(false);
            exitDull.SetActive(true);
        }

        //Exiting the game if exit is active
        if (!(startActive) && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            Application.Quit();
            UnityEditor.EditorApplication.isPlaying = false;// for exiting while in editor
            
        }

        if (startActive && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            StartCoroutine(startGame());

        }

    }


    IEnumerator startGame()
    {
        titleText.SetActive(false);
        exitBright.SetActive(false);
        exitDull.SetActive(false);
        startBright.SetActive(false);
        startDull.SetActive(false);

        yield return new WaitForSeconds(pathDelay * 3);

        goldBubble.SetActive(true);

        yield return new WaitForSeconds(pathDelay * 3);

        goldBubble.SetActive(false);

        yield return new WaitForSeconds(pathDelay);

        for (int i = 0; i < pathParts.Length; i++)
        {
            pathParts[i].SetActive(true);

            yield return new WaitForSeconds(pathDelay);
        }

        SceneManager.LoadSceneAsync(nextScene);
    }
}
