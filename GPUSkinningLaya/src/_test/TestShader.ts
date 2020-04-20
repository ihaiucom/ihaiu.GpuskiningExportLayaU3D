import { TestScene } from "./TestSene";
import GPUSkinningPlayerMono from "../GPUSkinning/GPUSkinningPlayerMono";
import GPUSkinningAnimation from "../GPUSkinning/Datas/GPUSkinningAnimation";

import Mesh = Laya.Mesh;
import Material = Laya.Material;
import Texture2D = Laya.Texture2D;
import GPUSkining from "../GPUSkinning/GPUSkining";
import { GPUSkinningUnlitMaterial } from "../GPUSkinning/Material/GPUSkinningUnlitMaterial";
import { GPUSkinningWrapMode } from "../GPUSkinning/Datas/GPUSkinningWrapMode";
import GPUSkiningAvatar, { IGPUSkiningAvatarItemConfig } from "../GPUSkinning/GPUSkiningAvatar";


export default class TestShader
{
    scene: TestScene;
    constructor()
    {
        this.scene = TestScene.create();
        Laya.stage.addChild(this.scene);
        this.InitGpuskingAsync();
    }

    async InitAsync()
    {
        GPUSkining.resRoot = "res3d/GPUSKinning-30/";
        await GPUSkining.InitAsync();

        var wuqi: Laya.Sprite3D = await this.loadAsync("res3d/Conventional/zhanji_wuqi.lh");
        


        var nameList = [
            // "Hero_1004_Dongzhuo_Skin1",
            "zhanji_belt",
            "zhanji_cibang",
            "zhanji_head",
            "zhanji_lower",
            "zhanji_necklace",
            "zhanji_upper",
        ];

        var upper : GPUSkinningPlayerMono;
        var cibang : GPUSkinningPlayerMono;
        for(var j = 0; j < nameList.length; j ++)
        {
            var resId = nameList[j];
            var mono = await GPUSkining.CreateByNameAsync(nameList[j], false, GPUSkinningUnlitMaterial);
            window['mono'] = mono;
            mono.Player.Play("idle");
            // for(var i = 0; i < mono.anim.clips.length; i ++)
            // {
            //     mono.anim.clips[i].wrapMode = GPUSkinningWrapMode.Loop;
            //     mono.anim.clips[i].individualDifferenceEnabled =true;
            // }

            this.scene.addChild(mono.owner);

            if(resId == "zhanji_upper")
            {
                upper = mono;
            }
            else if(resId == "zhanji_cibang")
            {
                cibang = mono;
            }
        }

        var join =  upper.Player.FindJointGameObject("D_Chest");
        join.addChild(cibang.owner);
        cibang.gameObject.transform.localPosition = new Laya.Vector3();
        cibang.gameObject.transform.localRotationEuler = new Laya.Vector3();

        var weaponR = upper.Player.FindJointGameObject("D_R_weapon");
        var weaponL = upper.Player.FindJointGameObject("D_L_weapon");

        var wuqiR = wuqi.clone();
        var wuqiL = wuqi.clone();
        weaponR.addChild(wuqiR);
        weaponL.addChild(wuqiL);


       
    }

    async InitGpuskingAsync(pos?: Laya.Vector3)
    {
        GPUSkining.resRoot = "res3d/GPUSKinning-30/";
        await GPUSkining.InitAsync();

        for(var z = 0; z < 2; z ++)
        {
            for(var x = 0; x < 10; x ++)
            {

                var pos = new Laya.Vector3(x - 5, 0, z);
                await this.InitTestZhanjiAsync(pos, true);
            }
        }
        
    }
    
    async InitTestZhanjiAsync(pos?: Laya.Vector3, isRandome?: boolean)
    {

        

        var avatarConfigs:IGPUSkiningAvatarItemConfig[] = 
        [
            {name:"zhanji_upper", isGpusking: true, isMain: true},
            {name:"zhanji_belt", isGpusking: true},
            {name:"zhanji_head", isGpusking: true},
            {name:"zhanji_lower", isGpusking: true},
            {name:"zhanji_necklace", isGpusking: true},
            {name:"zhanji_cibang", isGpusking: true, join: "D_Chest"},
            {name:"res3d/Conventional/zhanji_wuqi.lh", isGpusking: false, join: "D_R_weapon"},
            {name:"res3d/Conventional/zhanji_wuqi.lh", isGpusking: false, join: "D_L_weapon"},
        ];

        
        var avatarConfigs2:IGPUSkiningAvatarItemConfig[] = 
        [
            {name:"zhanji_upper2", isGpusking: true, isMain: true},
            {name:"zhanji_belt2", isGpusking: true},
            {name:"zhanji_head2", isGpusking: true},
            {name:"zhanji_lower2", isGpusking: true},
            {name:"zhanji_necklace2", isGpusking: true},
            {name:"zhanji_cibang2", isGpusking: true, join: "D_Chest"},
            {name:"res3d/Conventional/zhanji_wuqi.lh", isGpusking: false, join: "D_R_weapon"},
            {name:"res3d/Conventional/zhanji_wuqi.lh", isGpusking: false, join: "D_L_weapon"},
        ];

        if(isRandome)
        {
            for(var i = 0; i < avatarConfigs2.length; i ++)
            {
                if(Math.random() < 0.5)
                {
                    avatarConfigs[i] = avatarConfigs2[i];
                }
            }
        }

        var avatar = new GPUSkiningAvatar();
        await avatar.loadAvatarAsync(avatarConfigs);
        this.scene.addChild(avatar);
        // avatar.rotaitonStart();
        avatar.startRandomPlayClip();

        if(pos)
        {
            avatar.transform.position = pos;
        }

    }

    
    // 加载资源, 异步
    async loadAsync(path: string): Promise<any>
    {
        return new Promise<any>((resolve)=>
        {
            Laya.Sprite3D.load(path, 
                Laya.Handler.create(null, (res: any) =>
                {
                    resolve(res);
                }));
         });
    }


}