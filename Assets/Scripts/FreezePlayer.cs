using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class FreezePlayer : MonoBehaviour
{
    GameObject player;
    Rigidbody playerRb;
    float resetDelay = 2f;

    public void WaitAndDoThing()
    {
        StartCoroutine(FreezeAndResetPlayer());
    }

    private IEnumerator FreezeAndResetPlayer()
    {
        yield return new WaitForSeconds(0.2f);
        player = GameObject.FindWithTag("Player");
        transform.SetParent(player.transform);

        // Stop the player from moving.
        playerRb = player.GetComponent<Rigidbody>();

        if (playerRb != null)
        {
            playerRb.constraints = RigidbodyConstraints.FreezeAll;
            //Time.timeScale = 0;
            DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0.0f, 0.25f);
            var dashController = player.GetComponent<DashController>();
            if (dashController)
            {
                while (!dashController.IsDashing)
                {
                    yield return 0;
                }
            }

            DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1.0f, 1.25f);

            //Time.timeScale = 1;
            playerRb.constraints = RigidbodyConstraints.FreezeRotation;
            GameManager.Instance.MenuHelper.m_MiddleScreenLabel.text = "";
        }
        else
        {
            Debug.LogError("Rigidbody not found.");
        }
    }
}