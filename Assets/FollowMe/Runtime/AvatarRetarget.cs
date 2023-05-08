using System;
using System.Collections.Generic;
using UnityEngine;

namespace FollowMe.Runtime
{
    public class AvatarRetarget : MonoBehaviour
    {
        public AvatarSettings sourceAvatar;
        public AvatarSettings targetAvatar;

        public float blendShapeScale = 1;
        public List<BlendShapeToBoneSettings> blendShapeToBoneSettings;
        public List<BlendShapeMappingSettings> blendShapeMappingSettings;
        
        private SkeletonRetarget skeletonRetarget = new SkeletonRetarget();
        private BlendShapeRetarget blendShapeRetarget = new BlendShapeRetarget();

        public bool drawDebugGizmos;
        public bool updateInEditor;
        
        public void Reset()
        {
            sourceAvatar.Reset();
            targetAvatar.Reset();
        }

        void OnValidate()
        {
            Reset();
        }

        // Start is called before the first frame update
        void Start()
        {
            Reset();
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (skeletonRetarget != null)
            {
                skeletonRetarget.UpdateTargetSkeleton(sourceAvatar, targetAvatar);
            }

            if (blendShapeRetarget != null)
            {
                blendShapeRetarget.UpdateTargetBlendShape(sourceAvatar.avatarBody, targetAvatar.avatarBody, targetAvatar.avatarParts,
                    blendShapeMappingSettings, blendShapeToBoneSettings, blendShapeScale);
            }
        }

        void OnDrawGizmos()
        {
            // Your gizmo drawing thing goes here if required...
 
#if UNITY_EDITOR
            if (updateInEditor)
            {
                LateUpdate();
                if (drawDebugGizmos)
                {
                    SkeletonRetargetUtils.DrawDebugAnimator(sourceAvatar, Color.red);
                    SkeletonRetargetUtils.DrawDebugAnimator(targetAvatar, Color.green);
                }
            }
            else
            {
                if (drawDebugGizmos)
                {
                    SkeletonRetargetUtils.DrawDebugAvatar(sourceAvatar, Color.red);
                    SkeletonRetargetUtils.DrawDebugAvatar(targetAvatar, Color.green);
                }
            }
            
#endif
        }

    }
}
