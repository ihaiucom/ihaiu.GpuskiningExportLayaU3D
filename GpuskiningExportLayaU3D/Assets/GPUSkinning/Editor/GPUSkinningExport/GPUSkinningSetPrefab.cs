﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using UnityEditor;
using UnityEngine;

public class GPUSkinningSetPrefab
{

    public static bool SetGPUSkinningSampler(GameObject go, string name = null)
    {
        if (go == null)
        {
            Debug.LogError("请先选中有Animator的节点");
            return false;
        }

        Animator animator = go.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("不存在动画控制器 " + go.name);
            return false;
        }

        GPUSkinningSampler sampler = go.GetComponent<GPUSkinningSampler>();
        if (sampler == null)
        {
            sampler = go.AddComponent<GPUSkinningSampler>();
        }
        sampler.SetAnimatorList(name);
        return true;
    }

    public static string CheckSetGPUSkinningSamplerPrefab(string prefabPath)
    {
        GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        return CheckSetGPUSkinningSamplerPrefab(go);
    }

    public static string CheckSetGPUSkinningSamplerPrefab(GameObject go)
    {
        List<Animator> animatorList = new List<Animator>();
        Animator animator = go.GetComponent<Animator>();
        if (animator == null)
        {
            Animator[] animators = go.GetComponentsInChildren<Animator>();
            if (animators == null || animators.Length == 0)
            {
                Debug.LogError("不存在Animator " + go.name);
                return null;
            }
            animatorList.AddRange(animators);
        }
        else
        {
            animatorList.Add(animator);
        }

        string gpuskinningPrefabPath = null;
        for (int i = 0; i < animatorList.Count; i++)
        {
            GameObject animatorGOSrc = animatorList[i].gameObject;

            string name = null;

            if (animatorGOSrc.transform.parent != null)
            {
                name = animatorGOSrc.transform.parent.gameObject.name;
            }

            GameObject animatorGO = GameObject.Instantiate(animatorGOSrc);
            Selection.activeGameObject = animatorGO;
            if (!string.IsNullOrEmpty(name))
            {
                animatorGO.name = name;
            }
            gpuskinningPrefabPath = SetGPUSkinningSamplerPrefab(animatorGO, name);
            GameObject.DestroyImmediate(animatorGO);
        }

        return gpuskinningPrefabPath;
    }


    public static string SetGPUSkinningSamplerPrefab(GameObject go, string name = null)
    {
        SetGPUSkinningSampler(go, name);
        GPUSkinningSampler sampler = go.GetComponent<GPUSkinningSampler>();

        string dirPath = "Assets/GameResources/PrefabUnitGPUSkinning/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        string path = dirPath + go.name + ".prefab";

        PrefabUtility.SaveAsPrefabAsset(go, path);
        return path;
    }






}
