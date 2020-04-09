using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FXMaxParticleCount
{
    static int maxParticles = 1000;
    [MenuItem("JJSG TOOL/FXMaxParticleCount")]
    static void MaxParticleCountMenu()
    {
        GameObject[] goList = Selection.gameObjects;

        foreach (GameObject item in goList)
        {
            MaxParticleCount(item.transform);
        }
    }

    static void MaxParticleCount(Transform tran)
    {
        ParticleSystem[] list =  tran.GetComponentsInChildren<ParticleSystem>();
        foreach(ParticleSystem item in list)
        {
            item.maxParticles = maxParticles;
        }
    }
}
