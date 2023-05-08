using UnityEngine;

namespace FollowMe.Runtime
{
    public struct SkeletonBoneSetting
    {
        public HumanBodyBones bone;
        public string name;
        public bool enable;

        public SkeletonBoneSetting(HumanBodyBones bone, string name, bool enable)
        {
            this.bone = bone;
            this.name = name;
            this.enable = enable;
        }
    }

    public static class SkeletonSettings
    {
        public static SkeletonBoneSetting[] boneSettings = new[]
        {
            new SkeletonBoneSetting(HumanBodyBones.Hips, "Hips", true),
            new SkeletonBoneSetting(HumanBodyBones.LeftUpperLeg, "LeftUpperLeg", true),
            new SkeletonBoneSetting(HumanBodyBones.RightUpperLeg, "RightUpperLeg", true),
            new SkeletonBoneSetting(HumanBodyBones.LeftLowerLeg, "LeftLowerLeg", true),
            new SkeletonBoneSetting(HumanBodyBones.RightLowerLeg, "RightLowerLeg", true),
            new SkeletonBoneSetting(HumanBodyBones.LeftFoot, "LeftFoot", true),
            new SkeletonBoneSetting(HumanBodyBones.RightFoot, "RightFoot", true),
            new SkeletonBoneSetting(HumanBodyBones.Spine, "Spine", true),
            new SkeletonBoneSetting(HumanBodyBones.Chest, "Chest", true),
            new SkeletonBoneSetting(HumanBodyBones.Neck, "Neck", true),
            new SkeletonBoneSetting(HumanBodyBones.Head, "Head", true),
            new SkeletonBoneSetting(HumanBodyBones.LeftShoulder, "LeftShoulder", true),
            new SkeletonBoneSetting(HumanBodyBones.RightShoulder, "RightShoulder", true),
            new SkeletonBoneSetting(HumanBodyBones.LeftUpperArm, "LeftUpperArm", true),
            new SkeletonBoneSetting(HumanBodyBones.RightUpperArm, "RightUpperArm", true),
            new SkeletonBoneSetting(HumanBodyBones.LeftLowerArm, "LeftLowerArm", true),
            new SkeletonBoneSetting(HumanBodyBones.RightLowerArm, "RightLowerArm", true),
            new SkeletonBoneSetting(HumanBodyBones.LeftHand, "LeftHand", true),
            new SkeletonBoneSetting(HumanBodyBones.RightHand, "RightHand", true),
            new SkeletonBoneSetting(HumanBodyBones.LeftToes, "LeftToes", true),
            new SkeletonBoneSetting(HumanBodyBones.RightToes, "RightToes", true),
            // new SkeletonBoneSetting(HumanBodyBones.LeftEye, "LeftEye", false),
            // new SkeletonBoneSetting(HumanBodyBones.RightEye, "RightEye", false),
            // new SkeletonBoneSetting(HumanBodyBones.Jaw, "Jaw", false),
            new SkeletonBoneSetting(HumanBodyBones.LeftThumbProximal, "Left Thumb Proximal", true),
            new SkeletonBoneSetting(HumanBodyBones.LeftThumbIntermediate, "Left Thumb Intermediate", true),
            new SkeletonBoneSetting(HumanBodyBones.LeftThumbDistal, "Left Thumb Distal", true),
            new SkeletonBoneSetting(HumanBodyBones.LeftIndexProximal, "Left Index Proximal", true),
            new SkeletonBoneSetting(HumanBodyBones.LeftIndexIntermediate, "Left Index Intermediate", true),
            new SkeletonBoneSetting(HumanBodyBones.LeftIndexDistal, "Left Index Distal", true),
            new SkeletonBoneSetting(HumanBodyBones.LeftMiddleProximal, "Left Middle Proximal", true),
            new SkeletonBoneSetting(HumanBodyBones.LeftMiddleIntermediate, "Left Middle Intermediate", true),
            new SkeletonBoneSetting(HumanBodyBones.LeftMiddleDistal, "Left Middle Distal", true),
            new SkeletonBoneSetting(HumanBodyBones.LeftRingProximal, "Left Ring Proximal", true),
            new SkeletonBoneSetting(HumanBodyBones.LeftRingIntermediate, "Left Ring Intermediate", true),
            new SkeletonBoneSetting(HumanBodyBones.LeftRingDistal, "Left Ring Distal", true),
            new SkeletonBoneSetting(HumanBodyBones.LeftLittleProximal, "Left Little Proximal", true),
            new SkeletonBoneSetting(HumanBodyBones.LeftLittleIntermediate, "Left Little Intermediate", true),
            new SkeletonBoneSetting(HumanBodyBones.LeftLittleDistal, "Left Little Distal", true),
            new SkeletonBoneSetting(HumanBodyBones.RightThumbProximal, "Right Thumb Proximal", true),
            new SkeletonBoneSetting(HumanBodyBones.RightThumbIntermediate, "Right Thumb Intermediate", true),
            new SkeletonBoneSetting(HumanBodyBones.RightThumbDistal, "Right Thumb Distal", true),
            new SkeletonBoneSetting(HumanBodyBones.RightIndexProximal, "Right Index Proximal", true),
            new SkeletonBoneSetting(HumanBodyBones.RightIndexIntermediate, "Right Index Intermediate", true),
            new SkeletonBoneSetting(HumanBodyBones.RightIndexDistal, "Right Index Distal", true),
            new SkeletonBoneSetting(HumanBodyBones.RightMiddleProximal, "Right Middle Proximal", true),
            new SkeletonBoneSetting(HumanBodyBones.RightMiddleIntermediate, "Right Middle Intermediate", true),
            new SkeletonBoneSetting(HumanBodyBones.RightMiddleDistal, "Right Middle Distal", true),
            new SkeletonBoneSetting(HumanBodyBones.RightRingProximal, "Right Ring Proximal", true),
            new SkeletonBoneSetting(HumanBodyBones.RightRingIntermediate, "Right Ring Intermediate", true),
            new SkeletonBoneSetting(HumanBodyBones.RightRingDistal, "Right Ring Distal", true),
            new SkeletonBoneSetting(HumanBodyBones.RightLittleProximal, "Right Little Proximal", true),
            new SkeletonBoneSetting(HumanBodyBones.RightLittleIntermediate, "Right Little Intermediate", true),
            new SkeletonBoneSetting(HumanBodyBones.RightLittleDistal, "Right Little Distal", true),
            new SkeletonBoneSetting(HumanBodyBones.UpperChest, "UpperChest", true),
        };

        public static HumanBodyBones[] BonesToUse
        {
            get {
                var bones = new HumanBodyBones[boneSettings.Length];
                for (int i = 0; i < boneSettings.Length; i++)
                {
                    bones[i] = boneSettings[i].bone;
                }
                return bones;
            }
        }
    }

}