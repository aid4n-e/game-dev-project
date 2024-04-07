using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinCutscene : MonoBehaviour
{

    public GameObject winScreen;
    public ReferenceManager rm;

    void Win() {

        winScreen.SetActive(true);
        rm.playerState.player.gameObject.SetActive(false);

    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("PlayerTrigger"))
            Win();

    }
}
