using System;
using UnityEditor;
using UnityEngine;

namespace Config.Attributes
{
#if UNITY_EDITOR 
    using UnityEditor;
    [AttributeUsage(AttributeTargets.Field)]
    public class ValidatedStringAttribute : PropertyAttribute
    {
        public int MaxLength { get; private set; }
        public bool AllowEmpty { get; private set; }
    
        public ValidatedStringAttribute(int maxLength, bool allowEmpty = false)
        {
            MaxLength = maxLength;
            AllowEmpty = allowEmpty;
        }
    }

    [CustomPropertyDrawer(typeof(ValidatedStringAttribute))]
    public class ValidatedStringDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = (ValidatedStringAttribute)attribute;
        
            if (property.propertyType == SerializedPropertyType.String)
            {
                EditorGUI.BeginChangeCheck();
                string newValue = EditorGUI.TextField(position, label, property.stringValue);
            
                if (EditorGUI.EndChangeCheck())
                {
                    if (newValue.Length > attr.MaxLength)
                    {
                        newValue = newValue.Substring(0, attr.MaxLength);
                    }
                    
                    if (!attr.AllowEmpty && string.IsNullOrEmpty(newValue))
                    {
                        Debug.LogWarning("Поле не может быть пустым");
                    }
                    else
                    {
                        property.stringValue = newValue;
                    }
                }
                
                Rect infoRect = new Rect(position.x, position.y + position.height + 2, position.width, 16);
                string infoText = $"{property.stringValue.Length}/{attr.MaxLength}";
            
                if (string.IsNullOrEmpty(property.stringValue) && !attr.AllowEmpty)
                {
                    infoText += " (требуется заполнить)";
                    EditorGUI.LabelField(infoRect, infoText, EditorStyles.boldLabel);
                }
                else
                {
                    EditorGUI.LabelField(infoRect, infoText);
                }
            }
        }
    
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + 18;
        }
    }
#endif
}