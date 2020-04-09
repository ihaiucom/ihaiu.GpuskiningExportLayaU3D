using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

public class ExportGameResourcesSettings : ScriptableObject
{
    const string AssetName = "Settings/ExportGameResourcesSettings";

    private static ExportGameResourcesSettings instance = null;
    public static ExportGameResourcesSettings Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load(AssetName) as ExportGameResourcesSettings;
                if (instance == null)
                {
                    UnityEngine.Debug.Log("没有找到ExportGameResourcesSettings");
                    instance = CreateInstance<ExportGameResourcesSettings>();
                    instance.name = "ExportGameResourcesSettings";

#if UNITY_EDITOR
                    string path = "Assets/Game/Resources/" + AssetName + ".asset";
                    CheckPath(path);
                    AssetDatabase.CreateAsset(instance, path);
#endif
                }
            }
            return instance;
        }
    }


    public void Save()
    {
//#if UNITY_EDITOR
//        string path = "Assets/Game/Resources/" + AssetName + ".asset";
//        CheckPath(path);
//        AssetDatabase.CreateAsset(instance, path);
//#endif
    }

    public static void CheckPath(string path, bool isFile = true)
    {
        if (isFile) path = path.Substring(0, path.LastIndexOf('/'));
        string[] dirs = path.Split('/');
        string target = "";

        bool first = true;
        foreach (string dir in dirs)
        {
            if (first)
            {
                first = false;
                target += dir;
                continue;
            }

            if (string.IsNullOrEmpty(dir)) continue;
            target += "/" + dir;
            if (!Directory.Exists(target))
            {
                Directory.CreateDirectory(target);
            }
        }
    }


#if UNITY_EDITOR

    [MenuItem("MyLayaExport/ExportGameResourcesSettings", false, 1)]
    public static void EditSettings()
    {
        Selection.activeObject = Instance;
        EditorApplication.ExecuteMenuItem("Window/Inspector");
    }



#endif


    public string tmpCachePath = "../_tmp/";
    public string gameBinPath = "../../client/client/Game/bin/";
    public string chromePath = "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe";
    public bool isUseDir = true;
    public string dirName = "res3d";
    public ExportGameResourceSettingUsePathRoot usePathRoot;


    public string exportRoot
    {
        get
        {
            switch (usePathRoot)
            {
                case ExportGameResourceSettingUsePathRoot.Tmp:
                    return tmpCachePath;
                case ExportGameResourceSettingUsePathRoot.Game:
                default:
                    return gameBinPath;
            }
        }
    }

    public string res3dTmpPath
    {
        get
        {
            if (isUseDir)
            {

                return tmpCachePath + dirName + "/";
            }
            else
            {
                return tmpCachePath;
            }
        }
    }



    public string res3dGameBinPath
    {
        get
        {
            if (isUseDir)
            {

                return gameBinPath + dirName + "/";
            }
            else
            {
                return gameBinPath;
            }
        }
    }

    public string res3dPath
    {
        get
        {
            switch (usePathRoot)
            {
                case ExportGameResourceSettingUsePathRoot.Tmp:
                    return res3dTmpPath;
                case ExportGameResourceSettingUsePathRoot.Game:
                default:
                    return res3dGameBinPath;
            }

        }
    }


    public string res3dConventionalPath
    {
        get
        {
            return res3dPath + "Conventional/";
        }
    }




    public string res3dGpuskiningPath
    {
        get
        {
            return res3dPath + "GPUSKinning-30/";
        }
    }

    public bool isExportLaya = true;
    public bool isExportGpuskining = true;

    public bool isCreateModelAndExport = true;

}

public enum ExportGameResourceSettingUsePathRoot
{
    Tmp,
    Game
}


