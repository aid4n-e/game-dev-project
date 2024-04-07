using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutscene : MonoBehaviour
{
    public ReferenceManager rm;
    public GameObject[] slides;


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartCutscene());
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    IEnumerator StartCutscene() {
        rm.playerState.player.gameObject.SetActive(false);
        for(int i = 0; i < slides.Length; i++) {
            
            yield return new WaitForSeconds(5);
            slides[i].SetActive(false);

        }
        rm.playerState.player.gameObject.SetActive(true);

    }
}
