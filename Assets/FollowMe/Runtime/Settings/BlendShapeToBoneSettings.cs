using System;
using System.Collections.Generic;
using UnityEngine;

namespace FollowMe.Runtime
{

    public enum BoneRotationEulerDir
    {
        // positive
        X,
        Y,
        Z,
        // negative
        Xn,
        Yn,
        Zn
    };
    
    [Serializable]
    public struct BlendShapeToBoneSetting
    {
        public string blendShapeName;
        public float sensitivity;
        public BoneRotationEulerDir eulerDir;
    }
    
    [CreateAssetMenu(fileName = "BlendShapeToBoneSettings", menuName = "FollowMe|BlendShapeToBoneSettings", order = 0)]
    public class BlendShapeToBoneSettings : ScriptableObject
    {
        public string name;
        public bool enable;
        public string boneName;
        public Vector3 boneEulerRef;
        public List<BlendShapeToBoneSetting> settingList;
    }

}