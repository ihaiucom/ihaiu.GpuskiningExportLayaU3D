import GPUSkinningPlayerJoint from "./GPUSkinningPlayerJoint";
import GPUSkinningPlayerMono from "./GPUSkinningPlayerMono";
import GPUSkining from "./GPUSkining";
import { GPUSkinningUnlitMaterial } from "./Material/GPUSkinningUnlitMaterial";

export interface IGPUSkiningAvatarItemConfig
{
    name: string;
    join?: string;
    isGpusking?: boolean;
    isMain?: boolean
}

export default class GPUSkiningAvatar extends Laya.Sprite3D
{
    main: GPUSkinningPlayerMono;
    list: GPUSkinningPlayerMono[] = [];

    get clips()
    {
        if(this.main)
        {
            return this.main.anim.clips;
        }
    }
    
    /** 查找导出的骨骼节点 */
    public FindJoint(boneName: string):GPUSkinningPlayerJoint
    {
        if(this.main)
        {
            return this.main.Player.FindJoint(boneName);
        }
    }

     /** 查找导出的骨骼节点GameObject */
     public FindJointGameObject(boneName: string):Laya.Sprite3D
     {
        if(this.main)
        {
            return this.main.Player.FindJointGameObject(boneName);
        }
     }

    /** 播放 */
    public Play(clipName:string, nomrmalizeTime : number = 0)
    {
        for(var i = 0, len = this.list.length; i < len; i ++)
        {
            this.list[i].Player.Play(clipName, nomrmalizeTime);
        }
    }

    /** 暂停 */
    public Stop()
    {
        for(var i = 0, len = this.list.length; i < len; i ++)
        {
            this.list[i].Player.Stop();
        }
    }

    /** 继续播放 */
    public Resume()
    {
        for(var i = 0, len = this.list.length; i < len; i ++)
        {
            this.list[i].Player.Resume();
        }
    }

    async loadAvatarAsync(avatarConfigs:IGPUSkiningAvatarItemConfig[])
    {
        for(var item of avatarConfigs)
        {
            await this.loadAvatarItemAsync(item);
        }
    }

    
    async loadAvatarItemAsync(avatarConfig:IGPUSkiningAvatarItemConfig)
    {
        var parent: Laya.Sprite3D = this;
        if(avatarConfig.join && avatarConfig.join != "")
        {
            var join = this.FindJointGameObject(avatarConfig.join);
            if(join) parent = join;
        }
        
        if(avatarConfig.isGpusking)
        {
            var mono = await GPUSkining.CreateByNameAsync(avatarConfig.name, false, GPUSkinningUnlitMaterial);
            if(avatarConfig.isMain)
            {
                this.main = mono;
            }
            this.list.push(mono);

            parent.addChild(mono.owner);
        }
        else
        {
            var sprite: Laya.Sprite3D = await this.loadAsync(avatarConfig.name);
            if(sprite)
            {
                sprite = <Laya.Sprite3D> sprite.clone();
                parent.addChild(sprite);
            }

            // var box = new Laya.MeshSprite3D(Laya.PrimitiveMesh.createBox(1, 1, 1));
            // box.transform.localScale = new Laya.Vector3(0.2, 0.2, 0.2);
            // parent.addChild(box);
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
                    setTimeout(() => {
                        
                        resolve(res);
                    }, 16);
                }));
         });
    }

    
    private _rotaitonSrc:Laya.Vector3;
    private _rotaitonCur:Laya.Vector3;
    rotaitonStart()
    {
        this._rotaitonCur = this.transform.localRotationEuler;
        Laya.timer.frameLoop(1, this, this.onRotaitonLoop)
    }

    
    rotaitonStop()
    {
        this.transform.localRotationEuler = this._rotaitonSrc;
        Laya.timer.clear(this, this.onRotaitonLoop)
    }
    
    private onRotaitonLoop()
    {
        this._rotaitonCur.y += 1;

        this.transform.localRotationEuler = this._rotaitonCur;
    }

    startRandomPlayClip()
    {
        if(this.clips.length > 0)
        {
            Laya.timer.once(Random.range(2000, 5000), this, this.onRandomPlayClip)
        }
    }

    
    stopRandomPlayClip()
    {
        Laya.timer.clear(this, this.onRotaitonLoop)
    }

    onRandomPlayClip()
    {
        var i = Random.range(0, this.clips.length);
        i = Math.floor(i);
        // if(i >= this.clips.length) i = this.clips.length - 1;

        this.Play(this.clips[i].name);
        this.startRandomPlayClip();
    }

}