using System;
using System.Collections.Generic;
using FollowMe.Runtime;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace FollowMe.Editor
{
    [CustomEditor(typeof(BlendShapeMappingSettings))]
    public class BlendShapeMappingSettingsEditor : UnityEditor.Editor
    {
        public BlendShapeMappingSettings m_BlendShapeMappingSettings;
        private new SerializedObject m_SerializedObject;

        SerializedProperty m_ListProperty;
        ReorderableList m_ReorderableList;

        void OnEnable()
        {
            if (target == null) return;
            m_BlendShapeMappingSettings = (BlendShapeMappingSettings)target;
            m_SerializedObject = new SerializedObject(m_BlendShapeMappingSettings);

            // 重载 blendShapeMappings 的 UI，使用滑块
            m_ListProperty = m_SerializedObject.FindProperty("blendShapeMappings");
            m_ReorderableList = new ReorderableList(serializedObject, m_ListProperty, false, true, true, true);
            m_ReorderableList.elementHeight = EditorGUIUtility.singleLineHeight * 2 + 5;
            OnEnableReorderableList();
        }

        public override void OnInspectorGUI()
        {
            if (m_SerializedObject == null) return;

            // 工具按钮
            if (GUILayout.Button(new GUIContent("BuildMapping","BuildBlendShapeMapping"), GUILayout.MaxWidth(150))) 
            {
                BuildBlendShapeMappingSettings();
            }
            if (GUILayout.Button(new GUIContent("AutoMapping","AutoBlendShapeMapping"), GUILayout.MaxWidth(150))) 
            {
                AutoBlendShapeMappingSettings();
            }
            if (GUILayout.Button(new GUIContent("Inverse","InverseBlendShapeMapping"), GUILayout.MaxWidth(150))) 
            {
                InverseBlendShapeMappingSettings();
            }

            
            EditorGUI.BeginChangeCheck();
            m_SerializedObject.Update(); // to representation
            
            // blendShapeMappings 单独渲染，先渲染其他 UI
            DrawPropertiesExcluding(m_SerializedObject, "blendShapeMappings");
            // 重载 blendShapeMappings 的 UI，使用滑块
            m_ReorderableList.DoLayoutList();

            if (EditorGUI.EndChangeCheck())
            {
                m_SerializedObject.ApplyModifiedProperties(); // to properties
                Repaint();
            }

        }


        // 渲染单个 BlendShape 的滑块 UI
        private float OnBlendShapeUI(SerializedProperty element, Rect rect, float heightNow)
        {
            EditorGUI.indentLevel++;

            SerializedProperty namesProperty = element.FindPropertyRelative("targetBlendShapeNames");
            SerializedProperty weightsProperty = element.FindPropertyRelative("targetBlendShapeWeights");
            
            int arraySize = Math.Min(namesProperty.arraySize, weightsProperty.arraySize);
            
            for (int i = 0; i < arraySize; i++)
            {
                
                Rect propertyRect = new Rect(rect.x + 20f, heightNow, rect.width - 20, EditorGUIUtility.singleLineHeight);
                
                //content.text = targetBlendShapeWeight.Key;
                // Calculate the min and max values for the slider from the frame blendshape weights
                float sliderMin = 0f, sliderMax = 1f;

                
                GUIContent propertyGUIContent = new GUIContent();

                propertyGUIContent.text = namesProperty.GetArrayElementAtIndex(i).stringValue;
                SerializedProperty valueProperty = weightsProperty.GetArrayElementAtIndex(i);
                valueProperty.floatValue = EditorGUI.Slider(propertyRect, propertyGUIContent, valueProperty.floatValue, sliderMin, sliderMax);

                heightNow += EditorGUIUtility.singleLineHeight;
            }

            EditorGUI.indentLevel--;

            return heightNow;
        }

        // 设置一组 BlendShapeMappingSetting 的 UI
        private void OnEnableReorderableList()
        {
            
            // Header
            m_ReorderableList.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "Blend shape mappings"); };

            // Element
            m_ReorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                // Get hair variables
                SerializedProperty element = m_ListProperty.GetArrayElementAtIndex(index);

                SerializedProperty localFoldoutSettings = element.FindPropertyRelative("localFoldout");
                SerializedProperty sourceBlendShapeNameSettings = element.FindPropertyRelative("sourceBlendShapeName");

                float heightNow = rect.y + 4;

                string blendShapeName = sourceBlendShapeNameSettings.stringValue;

                {
                    EditorGUI.BeginDisabledGroup(Application.isPlaying);


                    localFoldoutSettings.boolValue =
                        EditorGUI.Foldout(
                            new Rect(rect.x + 20, heightNow, rect.width, EditorGUIUtility.singleLineHeight),
                            localFoldoutSettings.boolValue, blendShapeName, true);

                    using (new EditorGUI.DisabledScope(!localFoldoutSettings.boolValue))
                    {
                        heightNow += EditorGUIUtility.singleLineHeight;

                        if (localFoldoutSettings.boolValue)
                        {
                            heightNow = OnBlendShapeUI(element, rect, heightNow);
                        }
                    }

                    EditorGUILayout.EndFoldoutHeaderGroup();
                    EditorGUI.EndDisabledGroup();
                }

            };
            
            // Height
            m_ReorderableList.elementHeightCallback = (int index) =>
            {
                // Get hair variables
                SerializedProperty element = m_ListProperty.GetArrayElementAtIndex(index);

                
                SerializedProperty localFoldoutSettings = element.FindPropertyRelative("localFoldout");
                SerializedProperty targetBlendShapeWeightsSettings = element.FindPropertyRelative("targetBlendShapeWeights");
                
                int arraySize = targetBlendShapeWeightsSettings.arraySize;
                
                int customizeHeightMul = (localFoldoutSettings.boolValue ? arraySize + 1 : 0) + 1;

                return EditorGUIUtility.singleLineHeight * customizeHeightMul;
            };
        }
        
        // 从列表中获得映射权重
        private float GetTargetBlendShapeWeight(List<BlendShapeMappingSetting> blendShapeMappings, string blendShapeName, string targetBlendShapeName)
        {
            for (int i = 0; i < blendShapeMappings.Count; i++)
            {
                if (blendShapeName == blendShapeMappings[i].sourceBlendShapeName)
                {
                    for (int j = 0; j < blendShapeMappings[i].targetBlendShapeNames.Count; j++)
                    {

                        if (targetBlendShapeName == blendShapeMappings[i].targetBlendShapeNames[j])
                        {
                            return blendShapeMappings[i].targetBlendShapeWeights[j];
                        }
                    }
                }
            }

            return 0;
        }
        
        // 初始化生成映射匹配，权重初始化为 0
        private void BuildBlendShapeMappingSettings()
        {
            BlendShapeMappingSettings settings = m_BlendShapeMappingSettings;

            if (settings.sourceBlendShapeNames == null 
                || settings.sourceBlendShapeNames.Length <= 0
                || settings.targetBlendShapeNames == null 
                || settings.targetBlendShapeNames.Length <= 0)
            {
                return;
            }
            
            
            List<BlendShapeMappingSetting> blendShapeMappings = new List<BlendShapeMappingSetting>();
            
            for (int i = 0; i < settings.sourceBlendShapeNames.Length; i++)
            {
                string blendShapeName = settings.sourceBlendShapeNames[i];
                
                BlendShapeMappingSetting blendShapeMappingSetting = new BlendShapeMappingSetting(blendShapeName);
                
                for (int j = 0; j < settings.targetBlendShapeNames.Length; j++)
                {
                    string targetBlendShapeName = settings.targetBlendShapeNames[j];
                    blendShapeMappingSetting.targetBlendShapeNames.Add(targetBlendShapeName);
                    
                    float targetBlendShapeWeight = GetTargetBlendShapeWeight(settings.blendShapeMappings, blendShapeName, targetBlendShapeName);
                    blendShapeMappingSetting.targetBlendShapeWeights.Add(targetBlendShapeWeight);
                }
                blendShapeMappings.Add(blendShapeMappingSetting);
            }

            settings.blendShapeMappings = blendShapeMappings;
        }

        // 自动映射匹配名字相同的 BlendShape ,若找到名字相同的，权重为 1，否则为 0
        private void AutoBlendShapeMappingSettings()
        {
            BlendShapeMappingSettings settings = m_BlendShapeMappingSettings;

            if (settings.sourceBlendShapeNames == null 
                || settings.sourceBlendShapeNames.Length <= 0
                || settings.targetBlendShapeNames == null 
                || settings.targetBlendShapeNames.Length <= 0)
            {
                return;
            }
            
            
            List<BlendShapeMappingSetting> blendShapeMappings = new List<BlendShapeMappingSetting>();
            
            for (int i = 0; i < settings.sourceBlendShapeNames.Length; i++)
            {
                string blendShapeName = settings.sourceBlendShapeNames[i];
                
                BlendShapeMappingSetting blendShapeMappingSetting = new BlendShapeMappingSetting(blendShapeName);
                
                for (int j = 0; j < settings.targetBlendShapeNames.Length; j++)
                {
                    string targetBlendShapeName = settings.targetBlendShapeNames[j];
                    blendShapeMappingSetting.targetBlendShapeNames.Add(targetBlendShapeName);

                    float targetBlendShapeWeight = 0;
                    if (blendShapeName.Contains(targetBlendShapeName) || targetBlendShapeName.Contains(blendShapeName))
                    {
                        targetBlendShapeWeight = 1;
                    }
                    
                    blendShapeMappingSetting.targetBlendShapeWeights.Add(targetBlendShapeWeight);
                }
                blendShapeMappings.Add(blendShapeMappingSetting);
            }

            settings.blendShapeMappings = blendShapeMappings;
        }

        // 将 source 和 target 转换，即 source 为 target，target 为 source
        private void InverseBlendShapeMappingSettings()
        {
            BlendShapeMappingSettings settings = m_BlendShapeMappingSettings;

            if (settings.sourceBlendShapeNames == null 
                || settings.sourceBlendShapeNames.Length <= 0
                || settings.targetBlendShapeNames == null 
                || settings.targetBlendShapeNames.Length <= 0)
            {
                return;
            }
            
            (settings.sourceBlendShapeNames, settings.targetBlendShapeNames) = (settings.targetBlendShapeNames, settings.sourceBlendShapeNames);
            

            List<BlendShapeMappingSetting> blendShapeMappings = new List<BlendShapeMappingSetting>();
            
            for (int i = 0; i < settings.sourceBlendShapeNames.Length; i++)
            {
                string blendShapeName = settings.sourceBlendShapeNames[i];
                
                BlendShapeMappingSetting blendShapeMappingSetting = new BlendShapeMappingSetting(blendShapeName);
                
                for (int j = 0; j < settings.targetBlendShapeNames.Length; j++)
                {
                    string targetBlendShapeName = settings.targetBlendShapeNames[j];
                    blendShapeMappingSetting.targetBlendShapeNames.Add(targetBlendShapeName);

                    float targetBlendShapeWeight = 0;
                    
                    targetBlendShapeWeight = GetTargetBlendShapeWeight(settings.blendShapeMappings, targetBlendShapeName, blendShapeName);
                    
                    blendShapeMappingSetting.targetBlendShapeWeights.Add(targetBlendShapeWeight);
                }
                blendShapeMappings.Add(blendShapeMappingSetting);
            }

            settings.blendShapeMappings = blendShapeMappings;
        }

    }
}