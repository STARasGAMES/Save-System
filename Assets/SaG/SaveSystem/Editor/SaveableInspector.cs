using SaG.SaveSystem.GameStateManagement;
using UnityEditor;
using UnityEngine;

namespace SaG.SaveSystem.Editor
{
    [CustomEditor(typeof(Saveable), true)]
    public class SaveableInspector : UnityEditor.Editor
    {
        private Saveable targetComponent;

        private void OnEnable()
        {
            targetComponent = target as Saveable;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var serializedObject = new SerializedObject(targetComponent);
            var property = serializedObject.FindProperty("serializedSaveableComponents");

            SerializedProperty arraySizeProp = property.FindPropertyRelative("Array.size");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 15;
            EditorGUILayout.TextField(string.Format("Component ({0})", arraySizeProp.intValue), EditorStyles.boldLabel);
            EditorGUIUtility.labelWidth = 0;
            EditorGUILayout.TextField("Save Identifier", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < arraySizeProp.intValue; i++)
            {
                var subProperty = property.GetArrayElementAtIndex(i);
                var identifier = subProperty.FindPropertyRelative("identifier");
                var component = subProperty.FindPropertyRelative("component");

                EditorGUILayout.BeginHorizontal();
                GUI.enabled = false;
                EditorGUIUtility.labelWidth = 25;
                EditorGUILayout.PropertyField(component, new GUIContent());
                EditorGUIUtility.labelWidth = 0;
                GUI.enabled = true;

                EditorGUI.BeginChangeCheck();

                string identifierDrawer = EditorGUILayout.TextField(identifier.stringValue);

                if (EditorGUI.EndChangeCheck())
                {
                    identifier.stringValue = identifierDrawer;
                    serializedObject.ApplyModifiedProperties();

                    EditorUtility.SetDirty(targetComponent);
                    EditorUtility.SetDirty(targetComponent.gameObject);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Refresh"))
            {
                targetComponent.Refresh();
            }
        }
    }

}