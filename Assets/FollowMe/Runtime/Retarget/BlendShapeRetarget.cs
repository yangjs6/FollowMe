
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FollowMe.Runtime
{
    public class BlendShapeRetarget
    {
        public void UpdateTargetBlendShape(GameObject sourceAvatarBody, GameObject targetAvatarBody, List<GameObject> targetAvatarParts,
            List<BlendShapeMappingSettings> blendShapeMappingSettings, List<BlendShapeToBoneSettings> blendShapeToBoneSettings, float blendShapeScale)
        {
            if (!sourceAvatarBody || !targetAvatarBody)
            {
                return;
            }

            SkinnedMeshRenderer sourceMesh = sourceAvatarBody.GetComponent<SkinnedMeshRenderer>();
            SkinnedMeshRenderer targetMesh = targetAvatarBody.GetComponent<SkinnedMeshRenderer>();
            if (!sourceMesh || !targetMesh)
            {
                return;
            }

            List<SkinnedMeshRenderer> targetPartMeshList = new List<SkinnedMeshRenderer>();
            targetPartMeshList.Clear();
            foreach (var part in targetAvatarParts)
            {
                if (part)
                {
                    SkinnedMeshRenderer mesh = part.GetComponent<SkinnedMeshRenderer>();
                    if (mesh)
                    {
                        targetPartMeshList.Add(mesh);
                    }
                }
            }
            
            if (!sourceMesh || !targetMesh)
            {
                return;
            }
            
            BlendShapeRetargetUtils.UpdateTargetBlendShapeMapping(sourceMesh, targetMesh, blendShapeMappingSettings, blendShapeScale);
            BlendShapeRetargetUtils.UpdateTargetBlendShapeToBone(targetMesh, blendShapeToBoneSettings);
            
            foreach (var partMesh in targetPartMeshList)
            {
                BlendShapeRetargetUtils.UpdateTargetBlendShapeMapping(sourceMesh, partMesh, blendShapeMappingSettings, blendShapeScale);
            }
        }

    }


    public static class BlendShapeRetargetUtils
    {
        
        public static string[] GetBodyBlendShapeNames(GameObject avatarBody)
        {
            if (!avatarBody)
            {
                return null;
            }
            
            SkinnedMeshRenderer meshRenderer = avatarBody.GetComponent<SkinnedMeshRenderer>();
            if (!meshRenderer)
            {
                return null;
            }

            string[] blendShapeNames = new string[meshRenderer.sharedMesh.blendShapeCount];
            for (int i = 0; i < meshRenderer.sharedMesh.blendShapeCount; i++)
            {
                blendShapeNames[i] = meshRenderer.sharedMesh.GetBlendShapeName(i);
            }

            return blendShapeNames;
        }
        
        public static void UpdateTargetBlendShapeMapping(
            SkinnedMeshRenderer sourceMesh, 
            SkinnedMeshRenderer targetMesh,
            List<BlendShapeMappingSettings> blendShapeMappingSettings,
            float blendShapeScale)
        {
            Dictionary<int, float> targetBlendShapeWeights = new Dictionary<int, float>();

            foreach (var settings in blendShapeMappingSettings)
            {
                for (int i = 0; i < settings.blendShapeMappings.Count; i++)
                {
                    BlendShapeMappingSetting setting = settings.blendShapeMappings[i];

                    int sourceBlendShapeIndex = sourceMesh.sharedMesh.GetBlendShapeIndex(setting.sourceBlendShapeName);
                    if (sourceBlendShapeIndex < 0)
                    {
                        continue;
                    }

                    float sourceBlendShapeWeight = sourceMesh.GetBlendShapeWeight(sourceBlendShapeIndex) * settings.scaleBlendShapeWeight;

                    for (int j = 0; j < setting.targetBlendShapeNames.Count; j++)
                    {
                        int targetBlendShapeIndex =
                            targetMesh.sharedMesh.GetBlendShapeIndex(setting.targetBlendShapeNames[j]);

                        if (targetBlendShapeIndex < 0)
                        {
                            continue;
                        }

                        targetBlendShapeWeights.TryAdd(targetBlendShapeIndex, 0);

                        float targetBlendShapeWeight = setting.targetBlendShapeWeights[j] * sourceBlendShapeWeight;
                        targetBlendShapeWeights[targetBlendShapeIndex] += targetBlendShapeWeight;
                    }
                }
            }

            foreach (var targetBlendShape in targetBlendShapeWeights)
            {
                targetMesh.SetBlendShapeWeight(targetBlendShape.Key, targetBlendShape.Value * blendShapeScale);
            }
            
        }
        
        public static GameObject GetRootGameObject(GameObject gameObject)
        {
            GameObject root = (GameObject)PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject);
            if (!root && gameObject.transform.parent)
            {
                root = gameObject.transform.parent.gameObject;
            }

            return root;
        }

        public static float GetBlendShapeWeight(SkinnedMeshRenderer targetMesh, string blendShapeName)
        {
            int shapeIndex = targetMesh.sharedMesh.GetBlendShapeIndex(blendShapeName);
            if (shapeIndex >= 0)
            {
                float shapeValue = targetMesh.GetBlendShapeWeight(shapeIndex);
                return shapeValue;
            }

            return 0;
        }

        public static GameObject FindAvatarBone(GameObject gameObject, string boneName)
        {
            if (gameObject)
            {                
                if (gameObject.name == boneName)
                    return gameObject;

                int children = gameObject.transform.childCount;
                for (int i = 0; i < children; i++)
                {
                    GameObject found = FindAvatarBone(gameObject.transform.GetChild(i).gameObject, boneName);
                    if (found) return found;
                }
            }

            return null;
        }

        public static void UpdateTargetBlendShapeToBone(
            SkinnedMeshRenderer targetMesh, 
            List<BlendShapeToBoneSettings> settingsList)
        {
            GameObject root = GetRootGameObject(targetMesh.gameObject);
            if (!root)
            {
                return;
            }

            foreach (var settings in settingsList)
            {
                if (!settings.enable)
                {
                    continue;
                }

                GameObject boneObject = FindAvatarBone(root, settings.boneName);
                if (!boneObject)
                {
                    continue;
                }
    
                Vector3 refEuler = settings.boneEulerRef;

                foreach (var setting in settings.settingList)
                {
                    float blendShapeWeight = GetBlendShapeWeight(targetMesh, setting.blendShapeName);
            
                    float boneWeight = blendShapeWeight * setting.sensitivity;
            
                    switch (setting.eulerDir)
                    {
                        case BoneRotationEulerDir.X:
                            refEuler.x += boneWeight;
                            break;
                        case BoneRotationEulerDir.Y:
                            refEuler.y += boneWeight;
                            break;
                        case BoneRotationEulerDir.Z:
                            refEuler.z += boneWeight;
                            break;
                        case BoneRotationEulerDir.Xn:
                            refEuler.x -= boneWeight;
                            break;
                        case BoneRotationEulerDir.Yn:
                            refEuler.y -= boneWeight;
                            break;
                        case BoneRotationEulerDir.Zn:
                            refEuler.z -= boneWeight;
                            break;
                    }
                }

                Transform boneTransform = boneObject.transform;
                Quaternion rotation = boneTransform.localRotation;
                Vector3 euler = rotation.eulerAngles;
                boneTransform.localEulerAngles = refEuler;
                
            }
        }

    }
}