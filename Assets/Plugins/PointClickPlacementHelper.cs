using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class PointClickPlacementHelper : MonoBehaviour {
    public float placementRadius;
    public float lastTimeClicked;
    public bool MinDistance = false;
    public float minDistanceAmount;

    void Clicked()
    {
        lastTimeClicked = Time.time;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        bool recentlyClicked = (lastTimeClicked + 0.2f > Time.time);
        Gizmos.color = (Selection.activeGameObject != null && Selection.activeTransform == null) ? (recentlyClicked ? Color.yellow : Color.cyan) : Color.red;
        Gizmos.DrawWireSphere(transform.position, recentlyClicked ? placementRadius * 1.05f : this.placementRadius);
        Gizmos.color = new Color(255, 0, 0, .3f);
        Gizmos.DrawSphere(transform.position, this.placementRadius/5);

        if (MinDistance)
        {
            Gizmos.color = SetAlpha(Color.magenta,0.3f);
            Gizmos.DrawSphere(transform.position + (transform.right * (placementRadius * 0.5f)), minDistanceAmount);
        }
    }
#endif

    public Color SetAlpha(Color _c, float _newA)
    {
        return new Color(_c.r, _c.g, _c.b, _newA);
    }
}
