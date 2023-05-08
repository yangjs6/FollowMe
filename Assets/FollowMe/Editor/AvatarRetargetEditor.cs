using System.Linq;
using FollowMe.Runtime;
using UnityEditor;
using UnityEngine;

namespace FollowMe.Editor
{
    [CustomEditor(typeof(AvatarRetarget))]
    public class AvatarRetargetEditor : UnityEditor.Editor
    {
        private AvatarRetarget m_AvatarRetarget;
        private new SerializedObject m_SerializedObject;
        private void OnEnable()
        {
            if (target)
            {
                m_AvatarRetarget = (AvatarRetarget)target;
                m_SerializedObject = new SerializedObject(m_AvatarRetarget);
            }
        }

        private bool showAvatarTools;
        private bool showSourceAvatarTools = true;
        private bool showTargetAvatarTools = true;
        
        private void OnAvatarTools(ref bool showTools, string toolsLabel, bool isSourceAvatar)
        {
            showTools = EditorGUILayout.BeginFoldoutHeaderGroup(showTools, toolsLabel);
            if (showTools)
            {
                EditorGUI.indentLevel++;
                AvatarSettings avatarSettings = isSourceAvatar ? m_AvatarRetarget.sourceAvatar : m_AvatarRetarget.targetAvatar;
                
            
            
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("AvatarTools:");

                if (GUILayout.Button(new GUIContent("Reset","Reset Avatar Setting"))) 
                {
                    SkeletonRetargetUtils.ResetAvatar(avatarSettings.avatarRoot, ref avatarSettings.avatarSetting);
                }
                if (GUILayout.Button(new GUIContent("Save","Save To Avatar"))) 
                {
                    SkeletonRetargetUtils.SaveToAvatar(avatarSettings.avatarRoot, ref avatarSettings.avatarSetting);
                }
                if (GUILayout.Button(new GUIContent("Load","Load From Avatar"))) 
                {
                    SkeletonRetargetUtils.LoadFromAvatar(avatarSettings.avatarRoot, avatarSettings.avatarSetting);
                }
                EditorGUILayout.EndHorizontal();
                
                
                // 骨架工具
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("SkeletonTools:");

                if (GUILayout.Button(new GUIContent("Mirror(L_R)", "Mirror Skeleton from Left To Right")))
                {
                    SkeletonRetargetUtils.MirrorSkeleton(avatarSettings, true);
                }

                if (GUILayout.Button(new GUIContent("Mirror(R_L)", "Mirror Skeleton from Right To Left")))
                {
                    SkeletonRetargetUtils.MirrorSkeleton(avatarSettings, false);
                }
                
                if (GUILayout.Button(new GUIContent("AlignSkeleton", "Align skeleton")))
                {
                    SkeletonRetargetUtils.AlignSkeleton(m_AvatarRetarget.sourceAvatar, m_AvatarRetarget.targetAvatar);
                }


                EditorGUILayout.EndHorizontal();

                
                EditorGUILayout.BeginHorizontal();
                
                // 融合变形工具
                EditorGUILayout.LabelField("BlendShapeTools:");
                if (GUILayout.Button(new GUIContent("BuildBlendShape","BuildBlendShape"))) 
                {
                    if (m_AvatarRetarget.blendShapeMappingSettings.Count > 0)
                    {
                        BlendShapeMappingSettings setting = m_AvatarRetarget.blendShapeMappingSettings.Last();
                        if (isSourceAvatar)
                        {
                            setting.sourceBlendShapeNames = BlendShapeRetargetUtils.GetBodyBlendShapeNames(avatarSettings.avatarBody);
                        }
                        else
                        {
                            setting.targetBlendShapeNames = BlendShapeRetargetUtils.GetBodyBlendShapeNames(avatarSettings.avatarBody);
                        }
                    }
                }
                
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        public override void OnInspectorGUI()
        {

            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("RetargetTools:", GUILayout.MaxWidth(100));
                if (GUILayout.Button(new GUIContent("Reset","Reset Avatar Retarget"))) 
                {
                    m_AvatarRetarget.Reset();
                }
                EditorGUILayout.EndHorizontal();
                
            }

            base.OnInspectorGUI();

            // showAvatarTools = EditorGUILayout.BeginFoldoutHeaderGroup(showAvatarTools, "Avatar Tools");
            // if (showAvatarTools)
            {
                OnAvatarTools(ref showSourceAvatarTools,"Source Avatar Tools", true);
                OnAvatarTools(ref showTargetAvatarTools,"Target Avatar Tools", false);
            }
            
            // EditorGUILayout.EndFoldoutHeaderGroup();

        }

    }
}