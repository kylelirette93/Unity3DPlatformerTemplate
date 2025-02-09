using System.Collections;
using System.Collections.Generic;
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
        playerInput = player.GetComponent<PlayerInput>();
        if (playerRb != null)
        {
            playerRb.constraints = RigidbodyConstraints.FreezeAll;
            playerInput.enabled = false;
            StartCoroutine(ResetPlayer(resetDelay));
        }
        else
        {
            Debug.LogError("Rigidbody not found.");
        }
    }

    private IEnumerator ResetPlayer(float delay)
    {
       yield return new WaitForSeconds(delay);
       playerRb.constraints = RigidbodyConstraints.None;
       playerRb.constraints = RigidbodyConstraints.FreezeRotation;
       playerInput.enabled = true; 
    }
}
