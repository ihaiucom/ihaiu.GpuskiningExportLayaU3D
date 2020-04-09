using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
public class LayaParticleSystemExt
{
    public static void ModifyJson(GameObject obj, object jsonx, int level)
    {
        JSONObject json = null;
        JSONObject root = null;
        //var path = Application.streamingAssetsPath + "/../../../Unity3DExport/res3d/Conventional/";
        var path = ExportGameResourcesSettings.Instance.res3dConventionalPath;

        if (level == 0)
        {
            Debug.Log(path + "/" + obj.name + ".lh");
            FileStream aFile = new FileStream(path + "/" + obj.name + ".lh", FileMode.OpenOrCreate);
            string fileContents;
            using (StreamReader file = new StreamReader(aFile))
            {
                fileContents = file.ReadToEnd();
                file.Close();

            }
            root = JSONObject.Create(fileContents);
            json = root.GetField("data");
        }
        else
        {
            json = jsonx as JSONObject;
        }

        for (var j = 0; j < obj.transform.childCount; j++)
        {
            if (json != null)
                ModifyJson(obj.transform.GetChild(j).gameObject, json.GetField("child")[j], level + 1);
        }

        var part = obj.GetComponent<ParticleSystem>();
        if (part)
        {
            JSONObject props = null;
            if (json != null)
            {
                props = json.GetField("props");
            }
            if (props != null)
            {
                props.AddField("speedLimitEnable", part.limitVelocityOverLifetime.enabled);
                props.AddField("speedDampen", part.limitVelocityOverLifetime.dampen);
                props.AddField("speedLimit", part.limitVelocityOverLifetime.limit.constant);

                Renderer r = part.GetComponent<Renderer>();
                props.SetField("sortingFudge", -r.sortingOrder);
            }
        }
        if (level == 0)
        {
            FileStream wFile = new FileStream(path + "/" + obj.name + ".lh", FileMode.Truncate);
            StreamWriter sw = new StreamWriter(wFile);

            sw.Write(root.Print());
            sw.Close();
        }
    }

    [MenuItem("LayaExt/修复导出粒子")]
    public static void GenParticleSystem()
    {
        GameObject[] all = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
        for (int i = 0; i < all.Length; i++)
        {
            var item = all[i];
            ParticleSystem[] partis = item.GetComponentsInChildren<ParticleSystem>();
            if (item.gameObject.scene.isLoaded && item.transform.parent == null && partis.Length > 0)
            {
                Debug.LogFormat("name:{0} {1}", item.name, partis.Length);
                ModifyJson(item, null, 0);
            }
        }
    }


    public static void ModifyJson2(GameObject obj, object jsonx, int level, int childIndex)
    {
        JSONObject json = null;
        json = jsonx as JSONObject;
        for (var j = 0; j < obj.transform.childCount; j++)
        {
            ModifyJson2(obj.transform.GetChild(j).gameObject, json.GetField("child")[j], level + 1, j);
        }

        var part = obj.GetComponent<ParticleSystem>();
        if (part)
        {
            JSONObject props = json.GetField("props");
            props.AddField("speedLimitEnable", part.limitVelocityOverLifetime.enabled);
            props.AddField("speedDampen", part.limitVelocityOverLifetime.dampen);
            props.AddField("speedLimit", part.limitVelocityOverLifetime.limit.constant);

            Renderer r = part.GetComponent<Renderer>();
            props.SetField("sortingFudge", -r.sortingOrder);

        }
    }

    [MenuItem("LayaExt/修复预览粒子")]
    public static void ShowParticleSystem()
    {
        var s = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var path = Application.streamingAssetsPath + "/LayaDemo/res/LayaScene_" + s.name + "/Conventional/";

        FileStream aFile = new FileStream(path + "/" + s.name + ".ls", FileMode.OpenOrCreate);
        string fileContents;
        using (StreamReader file = new StreamReader(aFile))
        {
            fileContents = file.ReadToEnd();
            file.Close();

        }
        var root = JSONObject.Create(fileContents);
        var json = root.GetField("data");

        for (int i = 0; i < s.rootCount; i++)
        {
            var item = s.GetRootGameObjects()[i];
            ParticleSystem[] partis = item.GetComponentsInChildren<ParticleSystem>();
            if (item.gameObject.scene.isLoaded && item.transform.parent == null && partis.Length > 0)
            {
                Debug.LogFormat("name:{0} {1} {2}", item.name, partis.Length, i);
                ModifyJson2(item, json.GetField("child")[i], 0, i);
            }
        }

        FileStream wFile = new FileStream(path + "/" + s.name + ".ls", FileMode.Truncate);
        StreamWriter sw = new StreamWriter(wFile);

        sw.Write(root.Print());
        sw.Close();
    }
}
