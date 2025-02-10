using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class FreezePlayer : MonoBehaviour
{
    GameObject player;
    Rigidbody playerRb;
    PlayerInput playerInput;
    float resetDelay = 2f;

    private void OnEnable()
    {
        player = GameObject.FindWithTag("Player");
        transform.SetParent(player.transform);
        // Stop the player from moving.
        playerRb = player.GetComponent<Rigidbody>();
        
        if (playerRb != null)
        {
            playerRb.constraints = RigidbodyConstraints.FreezeAll;
            Time.timeScale = 0;
            StartCoroutine(ResetPlayer(resetDelay));
        }
        else
        {
            Debug.LogError("Rigidbody not found.");
        }
    }

    private IEnumerator ResetPlayer(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Time.timeScale = 1;
        playerRb.constraints = RigidbodyConstraints.None;
       playerRb.constraints = RigidbodyConstraints.FreezeRotation;
    }
}
