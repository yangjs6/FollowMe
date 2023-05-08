using System;
using System.Collections.Generic;
using UnityEngine;

namespace FollowMe.Runtime
{

    [Serializable]
    public struct BlendShapeMappingSetting
    {
        public bool localFoldout;
        public string sourceBlendShapeName;
        public List<string> targetBlendShapeNames;
        public List<float> targetBlendShapeWeights;

        public BlendShapeMappingSetting(string name)
        {
            localFoldout = false;
            sourceBlendShapeName = name;
            targetBlendShapeNames = new List<string>();
            targetBlendShapeWeights = new List<float>();
        }
    }

    
    [CreateAssetMenu(fileName = "BlendShapeMappingSettings", menuName = "FollowMe|BlendShapeMappingSettings", order = 0)]
    public class BlendShapeMappingSettings : ScriptableObject
    {
        public float scaleBlendShapeWeight = 1;
        
        public string[] sourceBlendShapeNames;
        public string[] targetBlendShapeNames;
        
        public List<BlendShapeMappingSetting> blendShapeMappings = new List<BlendShapeMappingSetting>();
    }
}