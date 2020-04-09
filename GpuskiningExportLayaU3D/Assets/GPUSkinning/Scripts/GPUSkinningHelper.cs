using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GPUSkinningHelper
{
    public static string GPUSKinningAnim(string name)
    {
        return "GPUSKinning_Anim_" + name + ".asset";
    }


    public static string GPUSKinningMesh(string name)
    {
        return "GPUSKinning_Mesh_" + name + ".asset";
    }

    public static string GPUSKinningMaterial(string name)
    {
        return "GPUSKinning_Material_" + name + ".mat";
    }

    public static string GPUSKinningMatrixTexture(string name)
    {
        return "GPUSKinning_Laya_Texture_" + name + ".bytes";
    }

    public static string LayaGPUSKinningAnim(string name)
    {
        return "GPUSKinning_" + name + "_Anim.bin";
    }


    public static string LayaGPUSKinningMesh(string name)
    {
        return "GPUSKinning_" + name + "_Mesh.bin";
    }
}
