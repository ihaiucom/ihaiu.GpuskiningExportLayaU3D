
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Animations;
using GPUSkingings;
using System;

namespace ExportTool
{
    public class AnimatorCreater : EditorWindow
    {
        public static AnimatorCreater currentWindow;

        // 动画资源，材质资源存的位置。
        private static string defaultOutPath = "GameArt/Unit/";

        private static string prefabSavePath = "GameResources/PrefabUnit/";

        private static string outPath = "";
        private static bool mIsCreateAnimatorController = true;

        private static string m_prefabName = "Hero_1002_Fengyunzhanji";

        private static string createFolderName = "";
        // 不知道为啥一定会生成这个东西，所以移除
        private static string m_NeedRemove = "__preview__Take 001";

        //创建视图
        //[MenuItem("MyLayaExport/打开动画创建面板")]
        public static void CreateWindow()
        {
            currentWindow =(AnimatorCreater) EditorWindow.GetWindow(typeof(AnimatorCreater), true, "动画创建", false);
        }

        private static void AddMatrial(ref GameObject _gameObj)
        {
            string _assetsPath = "Assets/" + defaultOutPath + createFolderName + "/" + createFolderName + "_Skin1" + ".prefab";
            
            GameObject _prefab = _gameObj;
            if(_prefab == null)
            {
                Debug.LogError(string.Format("没有对应的prefab预制体，请检查，路径为{0}", _assetsPath));
                return;
            }
            Renderer _render = _prefab.GetComponentInChildren<Renderer>();
            if(!_render)
            {
                Debug.LogError(string.Format("预制体中没有对应的SkinnedMeshRenderer请检查这个路径下的预制体:{0}", _assetsPath));
                return;
            }
            string _texturePath = "Assets/" + defaultOutPath + createFolderName + "/" + "texture" + ".png";
            // _texturePath = "Assets/yypTest/Hero_1002_Fengyunzhanji/texture.png";
            Texture2D _texture = AssetDatabase.LoadAssetAtPath(_texturePath, typeof(Texture2D)) as Texture2D;
            if(!_texture)
            {
                Debug.LogError(string.Format("该路径下不存在这个贴图:{0}", _texturePath));
                return;
            }
            
            Shader _shader = Shader.Find("LayaAir3D/Mesh/Unlit");
            if(_shader == null)
            {
                Debug.LogError("找不到这个路径的shader ： LayaAir3D/Mesh/Unlit");
            }
            
            Material _mat = new Material(_shader);
            _mat.mainTexture = _texture;
            _mat.SetColor("_Color", new Color(1, 1, 1, 255));
            string _matSavePath =  "Assets/" + defaultOutPath + createFolderName + "/" + createFolderName + "_Mat.mat";
            AssetDatabase.CreateAsset(_mat, _matSavePath);
            AssetDatabase.Refresh();
            _render.material = _mat;
        }


        // 创建预制体 
        private static string CreatePrefab()
        {
            UnityEngine.Object[] _objs = Selection.GetFiltered(typeof(GameObject), SelectionMode.Unfiltered);
            m_prefabName = createFolderName + "_Skin1";
            if(_objs.Length == 0)
            {
                Debug.Log("你没有选择任何物体");
                return null;
            }
            if(m_prefabName == "")
            {
                Debug.Log("预制体不要填写空的名字");
                return null;
            }
            string _path = Application.dataPath + "/" + defaultOutPath + createFolderName;
            string _saveFloderPath = Application.dataPath + "/" + prefabSavePath;


            if(!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }
            if(!Directory.Exists(_saveFloderPath))
            {
                Directory.CreateDirectory(_saveFloderPath);
            }

            string _animCtrlName = createFolderName + "_Anim.controller";
            string _assetsPath = "Assets/" + defaultOutPath + createFolderName;
            AnimatorController _animCtrl = AnimatorCreater.GetAnimatorController(_animCtrlName, _assetsPath);
            AnimatorControllerLayer _animLayer = _animCtrl.layers[0];

            GameObject skin = null;
            for(int i = 0 ; i < _objs.Length ; i++)
            {
                string name = _objs[i].name;
                if(name.ToLower().EndsWith("@skin"))
                {
                    skin = (GameObject) _objs[i];
                    continue;
                }

                string _objPath = AssetDatabase.GetAssetPath(_objs[i]);
                AnimatorCreater.AddState(_objPath, ref _animLayer);
            }

            if(skin == null)
            {
                skin = (GameObject)  _objs[0];
            }
            
            GameObject _prefab = new GameObject(m_prefabName);
            GameObject _obj = skin as GameObject;
            GameObject _model = Instantiate(_obj);
            _model.name = "model";

            GameObject AnchorFixed = new GameObject("AnchorFixed");
            GameObject AnchorShadow = new GameObject("AnchorShadow");
            _model.transform.SetParent(_prefab.transform);
            AnchorFixed.transform.SetParent(_prefab.transform);
            
            Animator animator = _model.GetComponentInChildren<Animator>();
            if(!animator)
            {
                animator = _model.AddComponent<Animator>();
            }
            animator.runtimeAnimatorController = _animCtrl;

            AnchorShadow.transform.SetParent(_prefab.transform);
            string _savePath = Application.dataPath + "/" + prefabSavePath + _prefab.name + ".prefab";

            AddMatrial(ref _prefab);

            PrefabUtility.SaveAsPrefabAsset(_prefab, _savePath);
            DestroyImmediate(_prefab);
            return _savePath;
        }
        
        // 添加状态
        private static void AddState(string _objPath, ref AnimatorControllerLayer _layer)
        {
            var _datas = AssetDatabase.LoadAllAssetsAtPath(_objPath);

            if (_datas.Length == 0)
            {
                Debug.Log(string.Format("Can't find clip in {0}", _objPath));
                return;
            }
            
            AnimatorStateMachine _stateMachine = _layer.stateMachine;


            foreach(var _data in _datas)
            {
                if (!(_data is AnimationClip))
                {
                    continue;
                }
                AnimationClip _newClip = _data as AnimationClip;
                bool tag = false;
                foreach(var _state in _stateMachine.states)
                {
                    if(_state.state.name == _newClip.name)
                    {
                        _stateMachine.RemoveState(_state.state);
                        break;
                    }    
                }
                if(tag)
                {
                    continue;
                }
                var _newState = _stateMachine.AddState(_newClip.name);
                _newState.motion = _newClip;

                if(_newClip.name.ToLower() == "idle")
                {
                    _stateMachine.defaultState = _newState;
                }
            }



            foreach (var _state in _stateMachine.states)
            {
                if(_state.state.name == m_NeedRemove || _state.state.name.ToLower() == "skin")
                {
                    _stateMachine.RemoveState(_state.state);
                    break;
                }
            }
        }

        // 获取动画控制器
        private static AnimatorController GetAnimatorController(string _controllerName, string _path)
        {
            AnimatorController _animCtrl = null;
            string filePath = _path + "/" + _controllerName;
            Debug.Log(filePath);
            _animCtrl = AssetDatabase.LoadAssetAtPath(filePath, typeof(AnimatorController)) as AnimatorController;

            if(_animCtrl == null)
            {
                _animCtrl = AnimatorCreater.CreateAnimatorController(_controllerName, _path);
                Debug.Log("**********");
            }

            if(_animCtrl == null)
            {
                Debug.Log("获取的Animtior controller为空");
            }
            return _animCtrl;
        }

        // 创建一个空的动画控制器
        private static AnimatorController CreateAnimatorController(string _controllerName, string _outPutPath)
        {
            if(!Directory.Exists(_outPutPath))
            {
                Directory.CreateDirectory(_outPutPath);
            }
            AnimatorController animCtrl = AnimatorController.CreateAnimatorControllerAtPath(_outPutPath + "/" + createFolderName + "_Anim" + ".controller");
            if(animCtrl == null)
            {
                Debug.Log("创建动画控制器失败，请查找原因");
            }
            return animCtrl;
        }

        private bool isWaitGpuskiningStart = false;

        private string currentPrefabPath = "";
        private bool foldoutSelectPrefab = true;
        private bool foldoutNormalSetting = true;
        private bool foldoutLayaSetting = true;
        private bool foldoutGpuskinSetting = true;
        private void OnGUI()
        {

            if (isWaitGpuskiningStart && Application.isPlaying && !string.IsNullOrEmpty(currentPrefabPath))
            {
                isWaitGpuskiningStart = false;
                GPUSkinningExportMenu.OneKeyExport(currentPrefabPath);
            }

            GUILayout.BeginVertical();

            //绘制标题
            GUILayout.Space(10);
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label("Create Prefabs");

            //绘制文本
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            outPath = EditorGUILayout.TextField("对应人物名字:", outPath);
            if(GUILayout.Button("使用选中的文件夹"))
            {
                if(Selection.activeObject)
                {
                    string path= AssetDatabase.GetAssetPath(Selection.activeObject);
                    outPath = Path.GetFileName( Path.GetDirectoryName(path) );
                }
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("只是创建预设"))
            {
                createFolderName = outPath;
                currentPrefabPath = CreatePrefab();

                currentPrefabPath = currentPrefabPath.Replace(Application.dataPath, "Assets");
            }


            GUILayout.Space(10);
            if (GUILayout.Button("一键创建和导出", GUILayout.Height(50)))
            {
                ExportGameResourcesSettings.Instance.Save();
                createFolderName = outPath;
                currentPrefabPath = CreatePrefab();
                currentPrefabPath = currentPrefabPath.Replace(Application.dataPath, "Assets");
                if (!string.IsNullOrEmpty(currentPrefabPath))
                {
                    PathHelper.CheckPath(ExportGameResourcesSettings.Instance.res3dPath);
                    if (ExportGameResourcesSettings.Instance.isExportLaya)
                    {
                        LayaExport.ExportPrefab(currentPrefabPath);
                    }


                    if (ExportGameResourcesSettings.Instance.isExportGpuskining)
                    {

                        onClickGpuskiningButton();

                    }

                }


            }


            GUILayout.Space(10);
            GUILayout.Space(10);
            foldoutSelectPrefab = EditorGUILayout.Foldout(foldoutSelectPrefab, "选中的预设");
            if (foldoutSelectPrefab)
            {

                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                GUILayout.BeginVertical();

                GUILayout.Space(10);

                if (GUILayout.Button("设置选中的预设"))
                {
                    if (Selection.activeGameObject != null)
                    {
                        currentPrefabPath = AssetDatabase.GetAssetPath(Selection.activeGameObject);
                    }
                }

                GUILayout.Space(10);
                currentPrefabPath = EditorGUILayout.TextField("当前预设路径:", currentPrefabPath);
                GUILayout.Space(10);


                if (!string.IsNullOrEmpty(currentPrefabPath))
                {

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);

                    GUILayout.Space(5);
                    if (GUILayout.Button("一键创建和导出"))
                    {
                        ExportGameResourcesSettings.Instance.Save();
                        if (!string.IsNullOrEmpty(currentPrefabPath))
                        {
                            PathHelper.CheckPath(ExportGameResourcesSettings.Instance.res3dPath);
                            if (ExportGameResourcesSettings.Instance.isExportLaya)
                            {
                                LayaExport.ExportPrefab(currentPrefabPath);
                            }


                            if (ExportGameResourcesSettings.Instance.isExportGpuskining)
                            {

                                onClickGpuskiningButton();

                            }

                        }


                    }

                    GUILayout.Space(5);
                    if (GUILayout.Button("导出Laya"))
                    {
                        ExportGameResourcesSettings.Instance.Save();
                        PathHelper.CheckPath(ExportGameResourcesSettings.Instance.res3dPath);
                        LayaExport.ExportPrefab(currentPrefabPath);
                    }

                    GUILayout.Space(5);
                    if (GUILayout.Button("导出Gpusking"))
                    {
                        ExportGameResourcesSettings.Instance.Save();
                        onClickGpuskiningButton();
                    }
                    GUILayout.EndHorizontal();
                }



                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
            GUILayout.Space(10);
            foldoutNormalSetting = EditorGUILayout.Foldout(foldoutNormalSetting, "路径设置");
            if (foldoutNormalSetting)
            {

                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                GUILayout.BeginVertical();



                GUILayout.BeginHorizontal();
                ExportGameResourcesSettings.Instance.tmpCachePath = EditorGUILayout.TextField("临时缓存目录:", ExportGameResourcesSettings.Instance.tmpCachePath);
                if (GUILayout.Button("打开"))
                {
                    Shell.RevealInFinder(ExportGameResourcesSettings.Instance.tmpCachePath);
                }

                if (GUILayout.Button("清空"))
                {
                    PathUtil.ClearDirectory(ExportGameResourcesSettings.Instance.tmpCachePath);
                }

                if (GUILayout.Button("拷贝到游戏"))
                {
                    CopyCommand.Copy(ExportGameResourcesSettings.Instance.res3dTmpPath, ExportGameResourcesSettings.Instance.res3dGameBinPath);

                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);



                GUILayout.BeginHorizontal();
                ExportGameResourcesSettings.Instance.gameBinPath = EditorGUILayout.TextField("Laya项目bin目录:", ExportGameResourcesSettings.Instance.gameBinPath);
                if (GUILayout.Button("打开"))
                {
                    Shell.RevealInFinder(ExportGameResourcesSettings.Instance.gameBinPath);
                }
                GUILayout.EndHorizontal();




                GUILayout.BeginHorizontal();
                ExportGameResourcesSettings.Instance.isUseDir = EditorGUILayout.ToggleLeft("自定义导出目录", ExportGameResourcesSettings.Instance.isUseDir);
                ExportGameResourcesSettings.Instance.dirName = EditorGUILayout.TextField("", ExportGameResourcesSettings.Instance.dirName);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
                ExportGameResourceSettingUsePathRoot usePathRoot = (ExportGameResourceSettingUsePathRoot) EditorGUILayout.EnumPopup("使用目录:", ExportGameResourcesSettings.Instance.usePathRoot);
                if(usePathRoot != ExportGameResourcesSettings.Instance.usePathRoot)
                {
                    ExportGameResourcesSettings.Instance.usePathRoot = usePathRoot;
                    LayaExportSetting.Instance.Save(true);
                }
                GUILayout.Space(20);
                ExportGameResourcesSettings.Instance.chromePath = EditorGUILayout.TextField("浏览器chrome", ExportGameResourcesSettings.Instance.chromePath);

                if (GUILayout.Button("浏览器运行"))
                {
                    ExportLayaMenu.RunGameBin();
                    //string url = ExportGameResourcesSettings.Instance.gameBinPath;
                    //url = "file:///" + Path.GetFullPath(url + "/index.html").Replace('\\', '/');
                    //string cmd = "\""+ ExportGameResourcesSettings.Instance.chromePath + "\"  --allow-file-access-from-files --allow-file-access-frome-files  --disable-web-security  --no-sandbox --user-data-dir=\"C:\\ProgramData\\JJSG_ChromeCache\" " + url;
                    //Shell.RunCode(cmd);
                    //ExportGameResourcesSettings.Instance.Save();
                }

                GUILayout.EndHorizontal();
                GUILayout.EndHorizontal();
            }



            GUILayout.Space(10);
            GUILayout.Space(10);
            foldoutLayaSetting = EditorGUILayout.Foldout(foldoutLayaSetting, "Laya设置");
            if (foldoutLayaSetting)
            {

                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("加载Laya导出设置"))
                {
                    LayaExportSetting.Instance.Load(true);
                }

                GUILayout.Space(5);
                if (GUILayout.Button("保存到Laya导出设置"))
                {
                    ExportGameResourcesSettings.Instance.Save();
                    LayaExportSetting.Instance.Save(true);
                }

                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("导出路径:", GUILayout.Width(80));
                EditorGUILayout.LabelField(ExportGameResourcesSettings.Instance.res3dConventionalPath);

                if (GUILayout.Button("打开"))
                {
                    Shell.RevealInFinder(ExportGameResourcesSettings.Instance.res3dConventionalPath);
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
                ExportGameResourcesSettings.Instance.isExportLaya = EditorGUILayout.ToggleLeft("一键 导出", ExportGameResourcesSettings.Instance.isExportLaya);

                GUILayout.EndHorizontal();
                GUILayout.EndHorizontal();
            }


            GUILayout.Space(10);
            GUILayout.Space(10);
            foldoutGpuskinSetting = EditorGUILayout.Foldout(foldoutGpuskinSetting, "Gpuskining设置");
            if (foldoutGpuskinSetting)
            {

                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("导出路径:", GUILayout.Width(80));
                EditorGUILayout.LabelField( ExportGameResourcesSettings.Instance.res3dGpuskiningPath);
                if (GUILayout.Button("打开"))
                {
                    Shell.RevealInFinder(ExportGameResourcesSettings.Instance.res3dGpuskiningPath);
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
                ExportGameResourcesSettings.Instance.isExportGpuskining = EditorGUILayout.ToggleLeft("一键 导出", ExportGameResourcesSettings.Instance.isExportGpuskining);

                GUILayout.EndHorizontal();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        private void onClickGpuskiningButton()
        {
            if (!Application.isPlaying)
            {
                EditorApplication.isPlaying = true;
                isWaitGpuskiningStart = true;
            }
            else
            {
                GPUSkinningExportMenu.OneKeyExport(currentPrefabPath);
            }
        }

    }

    
}

