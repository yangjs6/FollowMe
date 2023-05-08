using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FollowMe.Runtime
{
    public struct SkeletonBoneTransform
    {
        public HumanBodyBones boneIndex;
        public Transform transform;
        
        public Vector3 initPosition;
        public Quaternion initRotation;

        public SkeletonBoneTransform(HumanBodyBones b, Transform t)
        {
            boneIndex = b;
            transform = t;
            
            if (transform)
            {
                initPosition = transform.position;
                initRotation = transform.rotation;
            }
            else
            {
                initPosition = Vector3.one;
                initRotation = Quaternion.identity;
            }
        }
        
        public bool IsValid => transform;
    }
    
    public class SkeletonAnimator
    {
        public AvatarDescription avatar;
        
        public SkeletonBoneTransform rootTransform;

        public List<SkeletonBoneTransform> boneTransforms = new List<SkeletonBoneTransform>();


        public SkeletonAnimator(AvatarDescription avatar, Transform root)
        {
            this.avatar = avatar;
            this.rootTransform.transform = root;
        }

        
        public bool InitBones()
        {
            if (avatar.humanBones == null || avatar.humanBones.Length == 0)
            {
                return false;
            }
            
            rootTransform.initRotation = rootTransform.transform.rotation;
            rootTransform.initPosition = rootTransform.transform.position;

            Dictionary<string, AvatarHumanBone> boneMap = avatar.humanBones.ToDictionary( x => x.humanName, x => x);
            
            for (int i = 0; i < SkeletonSettings.boneSettings.Length; i++)
            {
                HumanBodyBones boneIndex = SkeletonSettings.boneSettings[i].bone;
                string humanName = SkeletonSettings.boneSettings[i].name;

                if (boneMap.ContainsKey(humanName))
                {
                    AvatarHumanBone humanBone = boneMap[humanName];
                    string boneName = humanBone.boneName;
                    Transform boneTransform = SkeletonRetargetUtils.FindBone(rootTransform.transform, boneName);
                    SkeletonBoneTransform boneTransformStruct = new SkeletonBoneTransform(boneIndex, boneTransform);
                    
                    boneTransformStruct.initPosition = humanBone.position;
                    boneTransformStruct.initRotation = humanBone.rotation;
                    
                    boneTransforms.Add(boneTransformStruct);
                }
                else
                {
                    boneTransforms.Add(new SkeletonBoneTransform(boneIndex, null));
                }
            }

            return true;
        }

        public Transform GetBoneTransform(HumanBodyBones bone)
        {
            return boneTransforms.Find(x => x.boneIndex == bone).transform;
        }
    }

    public class SkeletonRetarget
    {
        public void UpdateTargetSkeleton(AvatarSettings sourceAvatarSettings, AvatarSettings targetAvatarSettings)
        {
            SkeletonAnimator sourceAnimator = new SkeletonAnimator(sourceAvatarSettings.avatarSetting.avatarDescription, sourceAvatarSettings.avatarRoot.transform);
            SkeletonAnimator targetAnimator = new SkeletonAnimator(targetAvatarSettings.avatarSetting.avatarDescription, targetAvatarSettings.avatarRoot.transform);
            
            bool isInit = sourceAnimator.InitBones() && targetAnimator.InitBones();
            
            if (!isInit)
            {
                return;
            }

            if (sourceAnimator != null && targetAnimator != null)
            {
                targetAnimator.rootTransform.transform.position =
                    (sourceAnimator.rootTransform.transform.position - sourceAnimator.rootTransform.initPosition) +
                    targetAnimator.rootTransform.initPosition;
                
                for (int i = 0; i < targetAnimator.boneTransforms.Count; i++)
                {
                    SkeletonBoneTransform targetBone = targetAnimator.boneTransforms[i];
                    SkeletonBoneTransform sourceBone = sourceAnimator.boneTransforms[i];

                    Quaternion sourceRotation = Quaternion.Inverse(sourceAnimator.rootTransform.transform.rotation) * sourceBone.transform.rotation;

                    // 处理 rotation
                    Quaternion rotation = targetAnimator.rootTransform.transform.rotation;
                    rotation *= (sourceRotation * Quaternion.Inverse(sourceBone.initRotation));
                    rotation *= targetBone.initRotation;
                    targetBone.transform.rotation = rotation;

                    // 处理 hips 的位置
                    if (targetBone.boneIndex == HumanBodyBones.Hips)
                    {
                        Vector3 sourcePosition = sourceAnimator.rootTransform.transform.InverseTransformPoint(sourceBone.transform.position);
                        targetBone.transform.position = targetAnimator.rootTransform.transform.TransformPoint((sourcePosition - sourceBone.initPosition) + targetBone.initPosition);
                    }
                }
            }
        }
    }

    public static class SkeletonRetargetUtils
    {
        public static void MirrorBone(SkeletonAnimator animator, HumanBodyBones bone1, HumanBodyBones bone2)
        {
            Transform bone1T = animator.GetBoneTransform(bone1);
            Transform bone2T = animator.GetBoneTransform(bone2);
            if (bone1T && bone2T)
            {
                Quaternion bone1Rotation = bone1T.localRotation;
                Quaternion bone2Rotation = bone2T.localRotation;
                bone2Rotation.x = bone1Rotation.x;
                bone2Rotation.y = -bone1Rotation.y;
                bone2Rotation.z = -bone1Rotation.z;
                bone2Rotation.w = bone1Rotation.w;
                
                Undo.RecordObject(bone2T, "MirrorSkeleton");
                bone2T.localRotation = bone2Rotation;
            }
        }
        
        public static void MirrorSkeleton(AvatarSettings avatarRoot, bool leftToRight)
        {
            SkeletonAnimator animator = new SkeletonAnimator(avatarRoot.avatarSetting.avatarDescription, avatarRoot.avatarRoot.transform);
            bool isInit = animator.InitBones();
            if (!isInit)
            {
                return;
            }

            if (leftToRight)
            {
                MirrorBone(animator, HumanBodyBones.LeftUpperLeg, HumanBodyBones.RightUpperLeg);
                MirrorBone(animator, HumanBodyBones.LeftLowerLeg, HumanBodyBones.RightLowerLeg);
                MirrorBone(animator, HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot);
                MirrorBone(animator, HumanBodyBones.LeftShoulder, HumanBodyBones.RightShoulder);
                MirrorBone(animator, HumanBodyBones.LeftUpperArm, HumanBodyBones.RightUpperArm);
                MirrorBone(animator, HumanBodyBones.LeftLowerArm, HumanBodyBones.RightLowerArm);
                MirrorBone(animator, HumanBodyBones.LeftHand, HumanBodyBones.RightHand);
                MirrorBone(animator, HumanBodyBones.LeftToes, HumanBodyBones.RightToes);
                MirrorBone(animator, HumanBodyBones.LeftEye, HumanBodyBones.RightEye);
                MirrorBone(animator, HumanBodyBones.LeftThumbProximal, HumanBodyBones.RightThumbProximal);
                MirrorBone(animator, HumanBodyBones.LeftThumbIntermediate, HumanBodyBones.RightThumbIntermediate);
                MirrorBone(animator, HumanBodyBones.LeftThumbDistal, HumanBodyBones.RightThumbDistal);
                MirrorBone(animator, HumanBodyBones.LeftIndexProximal, HumanBodyBones.RightIndexProximal);
                MirrorBone(animator, HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.RightIndexIntermediate);
                MirrorBone(animator, HumanBodyBones.LeftIndexDistal, HumanBodyBones.RightIndexDistal);
                MirrorBone(animator, HumanBodyBones.LeftMiddleProximal, HumanBodyBones.RightMiddleProximal);
                MirrorBone(animator, HumanBodyBones.LeftMiddleIntermediate, HumanBodyBones.RightMiddleIntermediate);
                MirrorBone(animator, HumanBodyBones.LeftMiddleDistal, HumanBodyBones.RightMiddleDistal);
                MirrorBone(animator, HumanBodyBones.LeftRingProximal, HumanBodyBones.RightRingProximal);
                MirrorBone(animator, HumanBodyBones.LeftRingIntermediate, HumanBodyBones.RightRingIntermediate);
                MirrorBone(animator, HumanBodyBones.LeftRingDistal, HumanBodyBones.RightRingDistal);
                MirrorBone(animator, HumanBodyBones.LeftLittleProximal, HumanBodyBones.RightLittleProximal);
                MirrorBone(animator, HumanBodyBones.LeftLittleIntermediate, HumanBodyBones.RightLittleIntermediate);
                MirrorBone(animator, HumanBodyBones.LeftLittleDistal, HumanBodyBones.RightLittleDistal);
            }
            else
            {
                MirrorBone(animator, HumanBodyBones.RightUpperLeg, HumanBodyBones.LeftUpperLeg);
                MirrorBone(animator, HumanBodyBones.RightLowerLeg, HumanBodyBones.LeftLowerLeg);
                MirrorBone(animator, HumanBodyBones.RightFoot, HumanBodyBones.LeftFoot);
                MirrorBone(animator, HumanBodyBones.RightShoulder, HumanBodyBones.LeftShoulder);
                MirrorBone(animator, HumanBodyBones.RightUpperArm, HumanBodyBones.LeftUpperArm);
                MirrorBone(animator, HumanBodyBones.RightLowerArm, HumanBodyBones.LeftLowerArm);
                MirrorBone(animator, HumanBodyBones.RightHand, HumanBodyBones.LeftHand);
                MirrorBone(animator, HumanBodyBones.RightToes, HumanBodyBones.LeftToes);
                MirrorBone(animator, HumanBodyBones.RightEye, HumanBodyBones.LeftEye);
                MirrorBone(animator, HumanBodyBones.RightThumbProximal, HumanBodyBones.LeftThumbProximal);
                MirrorBone(animator, HumanBodyBones.RightThumbIntermediate, HumanBodyBones.LeftThumbIntermediate);
                MirrorBone(animator, HumanBodyBones.RightThumbDistal, HumanBodyBones.LeftThumbDistal);
                MirrorBone(animator, HumanBodyBones.RightIndexProximal, HumanBodyBones.LeftIndexProximal);
                MirrorBone(animator, HumanBodyBones.RightIndexIntermediate, HumanBodyBones.LeftIndexIntermediate);
                MirrorBone(animator, HumanBodyBones.RightIndexDistal, HumanBodyBones.LeftIndexDistal);
                MirrorBone(animator, HumanBodyBones.RightMiddleProximal, HumanBodyBones.LeftMiddleProximal);
                MirrorBone(animator, HumanBodyBones.RightMiddleIntermediate, HumanBodyBones.LeftMiddleIntermediate);
                MirrorBone(animator, HumanBodyBones.RightMiddleDistal, HumanBodyBones.LeftMiddleDistal);
                MirrorBone(animator, HumanBodyBones.RightRingProximal, HumanBodyBones.LeftRingProximal);
                MirrorBone(animator, HumanBodyBones.RightRingIntermediate, HumanBodyBones.LeftRingIntermediate);
                MirrorBone(animator, HumanBodyBones.RightRingDistal, HumanBodyBones.LeftRingDistal);
                MirrorBone(animator, HumanBodyBones.RightLittleProximal, HumanBodyBones.LeftLittleProximal);
                MirrorBone(animator, HumanBodyBones.RightLittleIntermediate, HumanBodyBones.LeftLittleIntermediate);
                MirrorBone(animator, HumanBodyBones.RightLittleDistal, HumanBodyBones.LeftLittleDistal);
            }
            
        }
    
        public static void AlignSkeleton(AvatarSettings sourceAvatarSettings, AvatarSettings targetAvatarSettings)
        {
            SkeletonAnimator sourceAnimator = new SkeletonAnimator(sourceAvatarSettings.avatarSetting.avatarDescription, sourceAvatarSettings.avatarRoot.transform);
            SkeletonAnimator targetAnimator = new SkeletonAnimator(targetAvatarSettings.avatarSetting.avatarDescription, targetAvatarSettings.avatarRoot.transform);
            
            bool isInit = sourceAnimator.InitBones() && targetAnimator.InitBones();
            
            if (!isInit)
            {
                return;
            }
            
            
            // AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.Spine, HumanBodyBones.Chest);
            // AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.Chest, HumanBodyBones.UpperChest);
            // AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.UpperChest, HumanBodyBones.Neck);
            // AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.Neck, HumanBodyBones.Head);
            
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.LeftShoulder, HumanBodyBones.LeftUpperArm);
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm);
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand);
            
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg);
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot);
            //AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.LeftFoot, HumanBodyBones.LeftToes);
            
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.LeftHand, new HumanBodyBones[]{
                HumanBodyBones.LeftThumbProximal,
                HumanBodyBones.LeftIndexProximal,
                HumanBodyBones.LeftMiddleProximal,
                HumanBodyBones.LeftRingProximal,
                HumanBodyBones.LeftLittleProximal,
            });
            
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.LeftThumbProximal, HumanBodyBones.LeftThumbIntermediate);
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.LeftThumbIntermediate, HumanBodyBones.LeftThumbDistal);
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftIndexIntermediate);
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.LeftIndexDistal);
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.LeftMiddleProximal, HumanBodyBones.LeftMiddleIntermediate);
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.LeftMiddleIntermediate, HumanBodyBones.LeftMiddleDistal);
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.LeftRingProximal, HumanBodyBones.LeftRingIntermediate);
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.LeftRingIntermediate, HumanBodyBones.LeftRingDistal);
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.LeftLittleProximal, HumanBodyBones.LeftLittleIntermediate);
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.LeftLittleIntermediate, HumanBodyBones.LeftLittleDistal);
            
            
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.RightShoulder, HumanBodyBones.RightUpperArm);
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm);
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand);
            
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg);
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot);
            //AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.RightFoot, HumanBodyBones.RightToes);
            
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.RightHand, new HumanBodyBones[]{
                HumanBodyBones.RightThumbProximal,
                HumanBodyBones.RightIndexProximal,
                HumanBodyBones.RightMiddleProximal,
                HumanBodyBones.RightRingProximal,
                HumanBodyBones.RightLittleProximal,
            });
            
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.RightThumbProximal, HumanBodyBones.RightThumbIntermediate);
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.RightThumbIntermediate, HumanBodyBones.RightThumbDistal);
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.RightIndexProximal, HumanBodyBones.RightIndexIntermediate);
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightIndexDistal);
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightMiddleIntermediate);
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.RightMiddleIntermediate, HumanBodyBones.RightMiddleDistal);
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.RightRingProximal, HumanBodyBones.RightRingIntermediate);
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.RightRingIntermediate, HumanBodyBones.RightRingDistal);
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.RightLittleProximal, HumanBodyBones.RightLittleIntermediate);
            AlignBone(sourceAnimator, targetAnimator, HumanBodyBones.RightLittleIntermediate, HumanBodyBones.RightLittleDistal);
        }

        private static void AlignBone(SkeletonAnimator sourceAnimator, SkeletonAnimator targetAnimator, HumanBodyBones bone1, HumanBodyBones[] bone2List)
        {
            Transform sourceBone1 = sourceAnimator.GetBoneTransform(bone1);
            Transform targetBone1 = targetAnimator.GetBoneTransform(bone1);
            
            Vector3 sourceBone2Avg = Vector3.zero;
            Vector3 targetBone2Avg = Vector3.zero;
            foreach (var bone2 in bone2List)
            {
                sourceBone2Avg += sourceAnimator.GetBoneTransform(bone2).position;
                targetBone2Avg += targetAnimator.GetBoneTransform(bone2).position;
            }
            sourceBone2Avg /= bone2List.Length;
            targetBone2Avg /= bone2List.Length;
            
            if (sourceBone1 && targetBone1)
            {
                Undo.RecordObject(targetBone1, "AlignSkeleton");
                
                Vector3 sourceDir = sourceBone2Avg - sourceBone1.position;
                Vector3 targetDir = targetBone2Avg - targetBone1.position;
                Vector3 localTargetDir = targetBone1.transform.InverseTransformDirection(targetDir);
                Vector3 localSourceDir = targetBone1.transform.InverseTransformDirection(sourceDir);
                // Debug.DrawLine(sourceBone1.position, sourceBone2.position, Color.yellow, 10.0f);
                // Debug.DrawLine(targetBone2.position, targetBone1.position, Color.yellow, 10.0f);
                Quaternion rotation = Quaternion.FromToRotation(localTargetDir, localSourceDir);
                targetBone1.localRotation = targetBone1.localRotation * rotation;
            }
        }
        
        private static void AlignBone(SkeletonAnimator sourceAnimator, SkeletonAnimator targetAnimator, HumanBodyBones bone1, HumanBodyBones bone2)
        {
            Transform sourceBone1 = sourceAnimator.GetBoneTransform(bone1);
            Transform sourceBone2 = sourceAnimator.GetBoneTransform(bone2);
            Transform targetBone1 = targetAnimator.GetBoneTransform(bone1);
            Transform targetBone2 = targetAnimator.GetBoneTransform(bone2);
            
            if (sourceBone1 && sourceBone2 && targetBone1 && targetBone2)
            {
                Undo.RecordObject(targetBone1, "AlignSkeleton");
                
                Vector3 sourceDir = sourceBone2.position - sourceBone1.position;
                Vector3 targetDir = targetBone2.position - targetBone1.position;
                Vector3 localTargetDir = targetBone1.transform.InverseTransformDirection(targetDir);
                Vector3 localSourceDir = targetBone1.transform.InverseTransformDirection(sourceDir);
                // Debug.DrawLine(sourceBone1.position, sourceBone2.position, Color.yellow, 10.0f);
                // Debug.DrawLine(targetBone2.position, targetBone1.position, Color.yellow, 10.0f);
                Quaternion rotation = Quaternion.FromToRotation(localTargetDir, localSourceDir);
                targetBone1.localRotation = targetBone1.localRotation * rotation;
            }
        }

        public static void DrawDebugAnimator(AvatarSettings avatarSettings, Color color)
        {
            SkeletonAnimator animator = new SkeletonAnimator(avatarSettings.avatarSetting.avatarDescription, avatarSettings.avatarRoot.transform);
            
            bool isInit = animator.InitBones();
            
            if (!isInit)
            {
                return;
            }

            
            Gizmos.color = color;
            if (animator != null)
            {
                for (int i = 0; i < animator.boneTransforms.Count; i++)
                {
                    Transform bone = animator.boneTransforms[i].transform;
                    if (bone)
                    {
                        Gizmos.DrawLine(bone.position, bone.parent.position);
                        Gizmos.DrawSphere(bone.position, 0.01f);
                    }
                }
            }
        }

        public static void DrawDebugAvatar(AvatarSettings avatarSettings, Color color)
        {
            Gizmos.color = color;

            Transform rootTransform = avatarSettings.avatarRoot.transform;
            AvatarSetting avatarSetting = avatarSettings.avatarSetting;

            if (avatarSetting != null)
            {
                for (int i = 0; i < avatarSetting.avatarDescription.humanBones.Length; i++)
                {
                    Vector3 position = rootTransform.TransformPoint(avatarSetting.avatarDescription.humanBones[i].position);
                    
                    Gizmos.DrawSphere(position, 0.01f);
                }
            }
        }

        
        public static void ResetAvatar(GameObject avatarRoot, ref AvatarSetting avatarSetting)
        {
            if (avatarSetting.avatarRef == null)
            {
                return;
            }
            
            Transform rootTransform = avatarRoot.transform;
            
            HumanDescription humanDescription = avatarSetting.avatarRef.humanDescription;
            ref AvatarDescription avatarDescription = ref avatarSetting.avatarDescription;
            
            avatarDescription.humanBones = new AvatarHumanBone[humanDescription.human.Length];
            Dictionary<string, SkeletonBone> skeletonBones = new Dictionary<string, SkeletonBone>();
            foreach (var skeletonBone in humanDescription.skeleton)
            {
                skeletonBones.TryAdd(skeletonBone.name, skeletonBone);
            }
            
            for (int i = 0; i < humanDescription.human.Length; i++)
            {
                var humanBone = humanDescription.human[i];
                SkeletonBone skeletonBone = skeletonBones[humanBone.boneName];
                Transform boneTransform = FindBone(rootTransform, humanBone.boneName);

                Vector3 bonePosition = boneTransform ? rootTransform.InverseTransformPoint(boneTransform.position) : skeletonBone.position;
                Quaternion boneRotation = boneTransform ? boneTransform.rotation * Quaternion.Inverse(rootTransform.rotation) : skeletonBone.rotation;
                Vector3 boneScale = boneTransform ? boneTransform.localScale : skeletonBone.scale;

                avatarDescription.humanBones[i] = new AvatarHumanBone()
                {
                    humanName = humanBone.humanName,
                    boneName = humanBone.boneName,
                    position = bonePosition,
                    rotation = boneRotation,
                    scale = boneScale
                };
            }
        }
        
        public static void SaveToAvatar(GameObject avatarRoot, ref AvatarSetting avatarSetting)
        {
            Transform rootTransform = avatarRoot.transform;
            
            for (int i = 0; i < avatarSetting.avatarDescription.humanBones.Length; i++)
            {
                ref AvatarHumanBone bone = ref avatarSetting.avatarDescription.humanBones[i];
                Transform boneTransform = FindBone(rootTransform, bone.boneName);
                if (boneTransform)
                {
                    bone.position = rootTransform.InverseTransformPoint(boneTransform.position) ;
                    bone.rotation = boneTransform.rotation * Quaternion.Inverse(rootTransform.rotation);
                    bone.scale = boneTransform.localScale;
                }
            }
        }

        public static void LoadFromAvatar(GameObject targetAvatarAvatarRoot, AvatarSetting targetAvatarAvatarSetting)
        {
            Transform rootTransform = targetAvatarAvatarRoot.transform;
            
            for (int i = 0; i < targetAvatarAvatarSetting.avatarDescription.humanBones.Length; i++)
            {
                AvatarHumanBone bone = targetAvatarAvatarSetting.avatarDescription.humanBones[i];
                Transform boneTransform = FindBone(rootTransform, bone.boneName);
                if (boneTransform)
                {
                    boneTransform.position = bone.position;
                    boneTransform.rotation = bone.rotation;
                    boneTransform.localScale = bone.scale;
                }
            }
        }
        
        
        // 递归查找
        public static Transform FindBone(Transform parent, string name)
        {
            if (!parent)
            {
                return null;
            }
            
            if (parent.name == name)
            {
                return parent;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform result = FindBone(parent.GetChild(i), name);
                if (result)
                {
                    return result;
                }
            }

            return null;
        }
    }
}