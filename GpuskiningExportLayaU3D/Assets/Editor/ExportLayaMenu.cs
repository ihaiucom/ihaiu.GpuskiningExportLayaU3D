using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;

namespace ExportTool
{
    public class ExportLayaMenu
    {
        //创建视图
        [MenuItem("MyLayaExport/打开动画创建面板")]
        public static void CreateWindow()
        {
            AnimatorCreater.CreateWindow();


        }


        [MenuItem("MyLayaExport/(选中的预设) Laya导出->tmp")]
        public static void ExportPrefabToTmp()
        {
            ExportGameResourcesSettings.Instance.usePathRoot = ExportGameResourceSettingUsePathRoot.Tmp;
            LayaExportSetting.Instance.Save(true);

            ExportSelectPrefabs();
        }

        [MenuItem("MyLayaExport/(选中的预设) Laya导出->Game bin")]
        public static void ExportPrefabToGameBin()
        {
            ExportGameResourcesSettings.Instance.usePathRoot = ExportGameResourceSettingUsePathRoot.Game;
            LayaExportSetting.Instance.Save(true);

            ExportSelectPrefabs();
        }

        public static void ExportSelectPrefabs()
        {
            GameObject[] gos = Selection.gameObjects;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            for(int i =0, len = gos.Length; i < len; i ++)
            {
                GameObject go = gos[i];
                LayaExport.ExportPrefab(go);

                string path = AssetDatabase.GetAssetPath(go);
                bool isEffect = path.IndexOf("PrefabEffect") != -1;
                Debug.Log(path);
                Debug.Log(isEffect);


                if (isEffect)
                {
                    LayaParticleSystemExt.ModifyJson(go, null, 0);
                }

            }
        }

        [MenuItem("MyLayaExport/(清理目录) tmp")]
        public static void ClearTmp()
        {
            PathUtil.ClearDirectory(ExportGameResourcesSettings.Instance.tmpCachePath);
        }

        [MenuItem("MyLayaExport/(拷贝目录) tmp->Game bin")]
        public static void CopyTmpToGameBin()
        {
            CopyCommand.Copy(ExportGameResourcesSettings.Instance.res3dTmpPath, ExportGameResourcesSettings.Instance.res3dGameBinPath);
        }


        [MenuItem("MyLayaExport/(打开目录) tmp")]
        public static void OpenDirTmp()
        {
            Shell.RevealInFinder(ExportGameResourcesSettings.Instance.tmpCachePath);
        }

        [MenuItem("MyLayaExport/(打开目录) Game bin")]
        public static void OpenDirGameBin()
        {
            Shell.RevealInFinder(ExportGameResourcesSettings.Instance.gameBinPath);
        }

        [MenuItem("MyLayaExport/运行游戏")]
        public static void RunGameBin()
        {
            string url = ExportGameResourcesSettings.Instance.gameBinPath;
            url = "file:///" + Path.GetFullPath(url + "/index.html").Replace('\\', '/');
            string cmd = "\"" + ExportGameResourcesSettings.Instance.chromePath + "\"  --allow-file-access-from-files --allow-file-access-frome-files  --disable-web-security  --no-sandbox --user-data-dir=\"C:\\ProgramData\\JJSG_ChromeCache\" " + url;
            Shell.RunCode(cmd);
        }

        //public static ExportSelectPrefab()
        //{

        //    PathHelper.CheckPath(ExportGameResourcesSettings.Instance.res3dPath);
        //    LayaExport.ExportPrefab(currentPrefabPath);
        //}
        


    }

}