
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace GPUSkingings
{
    public class GPUSkinningExportMenu
    {
        [MenuItem("Assets/GPUSkinning ExportMesh", false, 1)]
        [MenuItem("Assets/GPUSkinning ExportAnim", false, 1)]
        public static void ExportAnim()
        {
            Object[] objs = Selection.objects;
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i] is GPUSkinningAnimation)
                {
                    ExportAnim((GPUSkinningAnimation)objs[i]);
                }
                else if (objs[i] is Mesh)
                {
                    ExportMesh((Mesh)objs[i]);
                }
            }
        }

        public static void ExportAnim(GPUSkinningAnimation anim, string outDir = null)
        {
            GPUSkinningAnimExport export = new GPUSkinningAnimExport();
            export.SetAnim(anim);
            export.Export(outDir);
        }



        public static void ExportMesh(Mesh mesh, string outDir = null)
        {
            GPUSkinningMeshExport export = new GPUSkinningMeshExport();
            export.SetMesh(mesh);
            export.Export(outDir);
        }

        //[MenuItem("Assets/GPUSkinning ReadAnim", false, 1)]
        //public static void ReadAnim()
        //{
        //    Object[] objs = Selection.objects;
        //    for (int i = 0; i < objs.Length; i++)
        //    {

        //        if (objs[i] is TextAsset)
        //        {
        //            TextAsset asset = (TextAsset)objs[i];
        //            byte[] bytes = asset.bytes;
        //            Debug.Log(bytes);
        //            GPUSkinningAnimation anim = GPUSkinningAnimation.CreateFromBytes(bytes);
        //        }

        //    }
        //}





        [MenuItem("GPUSkinning/Set Current GPUSkinningSampler")]
        public static void SetGPUSkinningSampler()
        {
            GameObject go = Selection.activeGameObject;

            GPUSkinningSetPrefab.SetGPUSkinningSampler(go);
        }


        [MenuItem("GPUSkinning/PrefabUnit 一键导出")]
        public static void OneKeyExport()
        {
            GameObject go = Selection.activeGameObject;
            OneKeyExport(go);
        }


        public static void OneKeyExport(string prefabUnitPath)
        {
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabUnitPath);
            OneKeyExport(go);
        }

        public static void OneKeyExport(GameObject prefabUnit)
        {
            string gpuskinningPrefabPath = GPUSkinningSetPrefab.CheckSetGPUSkinningSamplerPrefab(prefabUnit);
            if (!string.IsNullOrEmpty(gpuskinningPrefabPath))
            {
                GPUSkinningExport.StartExport(gpuskinningPrefabPath, (bool isContinue) => 
                {
                    Debug.Log("生成完Gpuskin");

                    ExportGPUSkinningLayaBySelect(gpuskinningPrefabPath);


                });
            }
        }


        [MenuItem("GPUSkinning/Selection -> PrefabUnitGPUSkinningb")]
        public static void SetGPUSkinningSamplerPrefab()
        {
            GameObject[] list = Selection.gameObjects;
            for (int i = 0; i < list.Length; i++)
            {
                GPUSkinningSetPrefab.CheckSetGPUSkinningSamplerPrefab(list[i]);
            }
        }


        [MenuItem("GPUSkinning/步骤1 PrefabUnit -> PrefabUnitGPUSkinning")]
        public static void SetGPUSkinningSamplerPrefabByDirectory()
        {
            string dirPath = "Assets/GameResources/PrefabUnit";
            if (!Directory.Exists(dirPath))
            {
                Debug.Log("不存在该目录:" + dirPath);
                return;
            }

            string[] fileList = Directory.GetFiles(dirPath, "*.prefab");
            Debug.Log(fileList.Length);
            foreach (string filePath in fileList)
            {
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
                if (go == null)
                    continue;
                GPUSkinningSetPrefab.CheckSetGPUSkinningSamplerPrefab(go);
            }
        }


        [MenuItem("GPUSkinning/Selection -> GPUSkinning")]
        public static void ExportPrefabUnitGPUSkinning()
        {
            GameObject[] list = Selection.gameObjects;
            if (list == null || list.Length == 0)
            {
                Debug.Log("没有选中的预设");
                return;
            }

            GPUSkinningExport.StartExportList(list);

        }


        [MenuItem("GPUSkinning/步骤2 PrefabUnitGPUSkinning -> GPUSkinning")]
        public static void ExportPrefabUnitGPUSkinningByDirectory()
        {
            string dirPath = "Assets/GameResources/PrefabUnitGPUSkinning";
            if (!Directory.Exists(dirPath))
            {
                Debug.Log("不存在该目录:" + dirPath);
                return;
            }

            string[] fileList = Directory.GetFiles(dirPath, "*.prefab");
            List<GameObject> prefabList = new List<GameObject>();
            foreach (string filePath in fileList)
            {
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
                if (go == null)
                    continue;
                GPUSkinningSampler sampler = go.GetComponent<GPUSkinningSampler>();
                if (sampler != null)
                {
                    prefabList.Add(go);
                }
            }

            GPUSkinningExport.StartExportList(prefabList.ToArray());
        }

        public static void ExportGPUSkinningLayaBySelect(string gpuskinningPrefabPath)
        {
            string dirPath = "Assets/GameResources/GPUSkinning";
            //string outDir = "../Unity3DExport/res3d/GPUSkinning-30";
            string outDir = ExportGameResourcesSettings.Instance.res3dGpuskiningPath;
            if (!Directory.Exists(dirPath))
            {
                Debug.Log("不存在该目录:" + dirPath);
                return;
            }

            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            string gpuskinningPrefabName = Path.GetFileNameWithoutExtension(gpuskinningPrefabPath);

            string AnimName = GPUSkinningHelper.GPUSKinningAnim(gpuskinningPrefabName);
            string MeshName = GPUSkinningHelper.GPUSKinningMesh(gpuskinningPrefabName);
            string MatrixTextureName = GPUSkinningHelper.GPUSKinningMatrixTexture(gpuskinningPrefabName);
            string MaterialName = GPUSkinningHelper.GPUSKinningMaterial(gpuskinningPrefabName);

            GPUSkinningAnimation anim = AssetDatabase.LoadAssetAtPath<GPUSkinningAnimation>(dirPath + "/" + AnimName);
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(dirPath + "/" + MeshName);

            ExportAnim(anim, outDir);
            ExportMesh(mesh, outDir);
            ExportMesh(mesh, outDir);


            string src = dirPath + "/" + MatrixTextureName;
            string dest = outDir + "/" + Path.GetFileName(MatrixTextureName).Replace("_Laya_Texture_", "_").Replace(".bytes", "_MatrixTexture.bin");
            File.Copy(src, dest, true);

            GPUSkinningToGPUSkinningLayaMainTextureByPath(dirPath + "/" + MaterialName, outDir);


        }


        [MenuItem("GPUSkinning/步骤3 GPUSkinning -> GPUSkinningLaya")]
        public static void ExportGPUSkinningLayaByDirectory()
        {
            string dirPath = "Assets/GameResources/GPUSkinning";
            //string outDir = "../Unity3DExport/res3d/GPUSkinning-30";
            string outDir = ExportGameResourcesSettings.Instance.res3dGpuskiningPath;
            if (!Directory.Exists(dirPath))
            {
                Debug.Log("不存在该目录:" + dirPath);
                return;
            }

            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            GPUSkinningToGPUSkinningLayaAnimAndMesh(dirPath, outDir);
            GPUSkinningToGPUSkinningLayaMatrixTexture(dirPath, outDir);
            GPUSkinningToGPUSkinningLayaMainTexture(dirPath, outDir);
        }

        public static void GPUSkinningToGPUSkinningLayaAnimAndMesh(string dirPath, string outDir)
        {
            string[] fileList = Directory.GetFiles(dirPath, "*.asset");
            List<GPUSkinningAnimation> animList = new List<GPUSkinningAnimation>();
            List<Mesh> meshList = new List<Mesh>();
            foreach (string filePath in fileList)
            {
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(filePath);

                if (obj is GPUSkinningAnimation)
                {
                    animList.Add((GPUSkinningAnimation)obj);
                }
                else if (obj is Mesh)
                {
                    meshList.Add((Mesh)obj);
                }

            }

            for(int i = 0; i < animList.Count; i ++)
            {
                ExportAnim(animList[i], outDir);
            }

            for (int i = 0; i < meshList.Count; i++)
            {
                ExportMesh(meshList[i], outDir);
            }

            Debug.Log("animList.Count=" + animList.Count);
            Debug.Log("meshList.Count=" + meshList.Count);
        }



        public static void GPUSkinningToGPUSkinningLayaMatrixTexture(string dirPath, string outDir)
        {
            string[] fileList = Directory.GetFiles(dirPath, "*.bytes");
            foreach (string filePath in fileList)
            {
                string src = filePath;
                string dest = outDir + "/" + Path.GetFileName(filePath).Replace("_Laya_Texture_", "_").Replace(".bytes", "_MatrixTexture.bin");
                File.Copy(src, dest, true);
                Debug.Log(dest);
            }
            Debug.Log("MatrixTexture.Count=" + fileList.Length);
        }


        public static void GPUSkinningToGPUSkinningLayaMainTexture(string dirPath, string outDir)
        {
            //string outDirLaya = "../Unity3DExport/res3d/Conventional";

            string outDirLaya = ExportGameResourcesSettings.Instance.res3dConventionalPath;
            string[] fileList = Directory.GetFiles(dirPath, "*.mat");
            foreach (string filePath in fileList)
            {
                GPUSkinningToGPUSkinningLayaMainTextureByPath(filePath, outDir);
                //Material material = AssetDatabase.LoadAssetAtPath<Material>(filePath);
                //if(material == null || material.mainTexture == null)
                //{
                //    continue;
                //}

                //string texturePath =  AssetDatabase.GetAssetPath(material.mainTexture);
                //string ext = Path.GetExtension(texturePath);


                //string layaTexturePath = outDirLaya + "/" + texturePath;
                //if (!File.Exists(layaTexturePath))
                //{
                //    layaTexturePath = outDirLaya + "/" + texturePath.Replace(ext, ".jpg");
                //}
                //if (!File.Exists(layaTexturePath))
                //{
                //    Debug.LogWarning("没找到文件layaTexturePath=" + layaTexturePath);
                //    layaTexturePath = texturePath;
                //}

                //string dest = outDir + "/" + material.name.Replace("_Material_", "_") + "_MainTexture" + ext;
                //File.Copy(layaTexturePath, dest, true);
            }

            Debug.Log("MainTexture.Count=" + fileList.Length);
        }

        static void GPUSkinningToGPUSkinningLayaMainTextureByPath(string filePath, string outDir)
        {

            string outDirLaya = ExportGameResourcesSettings.Instance.res3dConventionalPath;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(filePath);
            if (material == null || material.mainTexture == null)
            {
                return;
            }

            if(material.mainTexture != null)
            {
                string mainTextureName = material.name.Replace("_Material_", "_") + "_MainTexture.png";
                CopyTexture(material.mainTexture, mainTextureName, outDir);


            }

            if(material.HasProperty("_ShadowTex"))
            {
                Texture shadowTexture = material.GetTexture("_ShadowTex");
                if (shadowTexture == null && material.mainTexture != null)
                {
                    string texturePath = AssetDatabase.GetAssetPath(material.mainTexture);
                    texturePath = Path.GetDirectoryName(texturePath) + "/shadow.png";
                    if (File.Exists(texturePath))
                    {
                        shadowTexture = AssetDatabase.LoadAssetAtPath<Texture>(texturePath);
                    }
                }
                if (shadowTexture != null)
                {
                    string shadowTextureName = material.name.Replace("_Material_", "_") + "_ShadowTexture.png";
                    CopyTexture(shadowTexture, shadowTextureName, outDir);
                }
            }

        }

        public static void CopyTexture(Texture texture, string name, string outDir)
        {
            string outDirLaya = ExportGameResourcesSettings.Instance.res3dConventionalPath;
            string texturePath = AssetDatabase.GetAssetPath(texture);
            string ext = Path.GetExtension(texturePath);


            string layaTexturePath = outDirLaya + "/" + texturePath;
            if (!File.Exists(layaTexturePath))
            {
                layaTexturePath = outDirLaya + "/" + texturePath.Replace(ext, ".jpg");
            }
            if (!File.Exists(layaTexturePath))
            {
                Debug.LogWarning("没找到文件layaTexturePath=" + layaTexturePath);
                layaTexturePath = texturePath;
            }

            string dest = outDir + "/" + name;
            File.Copy(layaTexturePath, dest, true);
        }


        [MenuItem("GPUSkinning/拷贝Laya导出的贴图替换现在的主贴图")]
        public static void CopyMainTextureUseLayaExport()
        {
            string dirPath = "Assets/GameResources/GPUSkinning";
            //string outDir = "../Unity3DExport/res3d/GPUSkinning-30";
            //string outDirLaya = "../Unity3DExport/res3d/Conventional";

            string outDir = ExportGameResourcesSettings.Instance.res3dConventionalPath;
            string outDirLaya = ExportGameResourcesSettings.Instance.res3dGpuskiningPath;
            if (!Directory.Exists(dirPath))
            {
                Debug.Log("不存在该目录:" + dirPath);
                return;
            }

            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }


            string[] fileList = Directory.GetFiles(dirPath, "*.mat");
            foreach (string filePath in fileList)
            {
                Material material = AssetDatabase.LoadAssetAtPath<Material>(filePath);
                if (material == null || material.mainTexture == null)
                {
                    continue;
                }


                string texturePath = AssetDatabase.GetAssetPath(material.mainTexture);
                string ext = Path.GetExtension(texturePath);
                

                string layaTexturePath = outDirLaya + "/"  + texturePath;
                if(!File.Exists(layaTexturePath))
                {
                    layaTexturePath = outDirLaya + "/" + texturePath.Replace(ext, ".jpg");
                }
                if (!File.Exists(layaTexturePath))
                {
                    Debug.LogWarning("没找到文件layaTexturePath=" + layaTexturePath);
                    continue;
                }
                //Debug.Log(layaTexturePath);

                string dest = outDir + "/" + material.name.Replace("_Material_", "_") + "_MainTexture" + ext;
                File.Copy(layaTexturePath, dest, true);
            }

            Debug.Log("MainTexture.Count=" + fileList.Length);

        }
    }
}
