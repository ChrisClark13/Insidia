using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(AIAggroProfile.MultEntry))]
public class AIAggroProfileEditor : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //EditorGUI.BeginProperty(position, label, property);

        Rect tagRect = new Rect(position.x, position.y, position.width / 2f, position.height);
        Rect multiplierRect = new Rect(position.x + position.width / 2f, position.y, position.width / 2f, position.height);


        SerializedProperty tag = property.FindPropertyRelative("tag");
        tag.stringValue = EditorGUI.TagField(tagRect, tag.stringValue);

        multiplierRect.xMin += 8;
        GUI.Label(multiplierRect, "Multiplier");
        multiplierRect.xMin += 42;
        EditorGUI.PropertyField(multiplierRect, property.FindPropertyRelative("multiplier"), GUIContent.none);

        //EditorGUI.EndProperty();
    }
}
