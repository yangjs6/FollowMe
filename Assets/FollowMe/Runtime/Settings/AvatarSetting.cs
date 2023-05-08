using System;
using System.Collections.Generic;
using UnityEngine;

namespace FollowMe.Runtime
{

    [Serializable]
    public struct AvatarHumanBone
    {
        public string humanName;
        public string boneName;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }
    
    [Serializable]
    public struct AvatarDescription
    {
        public AvatarHumanBone[] humanBones;
    }
    
    [CreateAssetMenu(fileName = "AvatarSetting", menuName = "FollowMe|AvatarSetting", order = 0)]
    public class AvatarSetting : ScriptableObject
    {
        public GameObject avatarRoot;
        
        public string bodyName;
        public List<string> partNames;
        
        public Avatar avatarRef;
        public AvatarDescription avatarDescription;
    }

    [Serializable]
    public struct AvatarSettings
    {
        public GameObject avatarRoot;
        public AvatarSetting avatarSetting;
        
        private GameObject _avatarBody;
        private List<GameObject> _avatarParts;

        public void Reset()
        {
            if (avatarSetting)
            {
                _avatarBody = avatarRoot.transform.Find(avatarSetting.bodyName).gameObject;
                _avatarParts = new List<GameObject>();
                foreach (var partName in avatarSetting.partNames)
                {
                    _avatarParts.Add(avatarRoot.transform.Find(partName).gameObject);
                }
            }
        }
        
        public GameObject avatarBody => _avatarBody;
        public List<GameObject> avatarParts => _avatarParts;
    }
}