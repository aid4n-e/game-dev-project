using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatforms : MonoBehaviour {

    public ReferenceManager rm;

    [SerializeField] float respawnDelay = 3f;
    [SerializeField] float FallingPlatformsSpeed = .5f;

    [SerializeField] Rigidbody2D platformBody2D;
    Vector2 spawnPos;
    bool active = false;

    private void Start() {
        platformBody2D.velocity = Vector2.zero;
        spawnPos = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("PlayerTrigger")) {
            if(!active)
                StartCoroutine(platformFall());
        }
    }

    IEnumerator platformFall() {

        active = true;
        
        yield return new WaitForSeconds(FallingPlatformsSpeed);

        rm.audioSrc.PlayOneShot(rm.sounds[5]);
        platformBody2D.bodyType = RigidbodyType2D.Dynamic;
        platformBody2D.velocity = Vector2.zero;

        yield return new WaitForSeconds(respawnDelay);

        platformBody2D.bodyType = RigidbodyType2D.Static;
        platformBody2D.velocity = Vector2.zero;
        transform.position = spawnPos;
        active = false;
    }
}