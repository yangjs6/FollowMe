using System.Collections.Generic;
using System.Linq;
using FollowMe.Runtime;
using UnityEditor;
using UnityEngine;

namespace FollowMe.Editor
{
    [CustomEditor(typeof(AvatarSetting))]
    public class AvatarSettingEditor : UnityEditor.Editor
    {
        private AvatarSetting m_AvatarSetting;
        private new SerializedObject m_SerializedObject;
        private void OnEnable()
        {
            if (target)
            {
                m_AvatarSetting = (AvatarSetting)target;
                m_SerializedObject = new SerializedObject(m_AvatarSetting);
            }
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}