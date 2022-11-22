using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrWind : MonoBehaviour
{
    [Header("Simple wind (hdrp only)")]
    public Material SharedMaterial;
    public float Power;

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        //hdrp lit float!
        SharedMaterial.SetFloat("_HeightAmplitude", Power * Mathf.Sin(Time.time * Mathf.PI));
    }
}
