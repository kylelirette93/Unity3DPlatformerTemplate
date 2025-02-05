using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testmatprop : MonoBehaviour
{
    public MeshFilter meshFilter;
    public MeshRenderer meshrend;

    public Texture2D desiredTexture;
    public Color desiredColor;

    [Button]
    public bool _TestButton = false;
    public void TestButton()
    {
        if (meshFilter == null) TryGetComponent<MeshFilter>(out meshFilter);
        if (meshrend == null) TryGetComponent<MeshRenderer>(out meshrend);
        meshFilter.mesh = DrawQuadSprite.QuadMesh;
        meshrend.sharedMaterial = DrawQuadSprite.DrawMaterial;
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetColor("_MainColor", desiredColor);
        propertyBlock.SetTexture("_BaseMap", desiredTexture);
        meshrend.SetPropertyBlock(propertyBlock);
        
        //meshrend.sharedMaterial.propertyblock
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
