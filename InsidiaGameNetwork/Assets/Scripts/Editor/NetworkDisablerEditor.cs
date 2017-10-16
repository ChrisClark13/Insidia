using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// No need to worry about anything in this mess of code unless you want to make something similar to it.
/// Created by Christian Clark
/// </summary>
[CustomEditor(typeof(NetworkDisabler))]
public class NetworkDisablerEditor : Editor {

    private SerializedProperty behaviourList;
    private SerializedProperty gameObjectList;
    private const float COLLUMN_WIDTH = 50f;
    private const float ADD_REMOVE_BUTTON_WIDTH = 30f;

	private void OnEnable () {
        behaviourList = serializedObject.FindProperty("behavioursToDisable");
        gameObjectList = serializedObject.FindProperty("gameObjectsToDisable");
	}

    public override void OnInspectorGUI()
    {
        //update our representation of the disabler
        serializedObject.Update();

        // Behaviour list //
        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUI.indentLevel++;

        behaviourList.isExpanded = EditorGUILayout.Foldout(behaviourList.isExpanded, "Behaviours to Disable");
        if (behaviourList.isExpanded)
        {
            ShowList(true);
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();

        // GameObject list //
        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUI.indentLevel++;

        gameObjectList.isExpanded = EditorGUILayout.Foldout(gameObjectList.isExpanded, "GameObjects to Disable");
        if (gameObjectList.isExpanded)
        {
            ShowList(false);
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();

    }

    private void ShowList(bool isBehaviourList)
    {
        SerializedProperty list = (isBehaviourList) ? behaviourList : gameObjectList;
        string entryPropName = (isBehaviourList) ? "behaviour" : "gameObject";
        Type entryType = (isBehaviourList) ? typeof(Behaviour) : typeof(GameObject);

        EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Enabled if: ","", GUILayout.MinWidth(100f));
            //GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal(GUILayout.Width(COLLUMN_WIDTH * 3f + ADD_REMOVE_BUTTON_WIDTH));
                EditorGUILayout.LabelField("Local", "", GUILayout.MaxWidth(COLLUMN_WIDTH));
                EditorGUILayout.LabelField("Client", "", GUILayout.MaxWidth(COLLUMN_WIDTH));
                EditorGUILayout.LabelField("Server", "", GUILayout.MaxWidth(COLLUMN_WIDTH));
                GUILayout.FlexibleSpace();
                GUILayout.Label("-/+", GUILayout.Width(ADD_REMOVE_BUTTON_WIDTH));
            EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < list.arraySize; i++)
        {
            SerializedProperty listEntry = list.GetArrayElementAtIndex(i);
            SerializedProperty isLocal = listEntry.FindPropertyRelative("disableIfNotLocal");
            SerializedProperty isClient = listEntry.FindPropertyRelative("disableIfNotClient");
            SerializedProperty isServer = listEntry.FindPropertyRelative("disableIfNotServer");

            EditorGUILayout.BeginHorizontal(GUI.skin.box);
                EditorGUILayout.ObjectField(listEntry.FindPropertyRelative(entryPropName), entryType, GUIContent.none, GUILayout.MinWidth(100f));
                //GUILayout.FlexibleSpace();
                EditorGUILayout.BeginHorizontal(GUILayout.Width(COLLUMN_WIDTH * 3f + ADD_REMOVE_BUTTON_WIDTH));
                    isLocal.boolValue = EditorGUILayout.Toggle(isLocal.boolValue, GUILayout.MaxWidth(COLLUMN_WIDTH));
                    isClient.boolValue = EditorGUILayout.Toggle(isClient.boolValue, GUILayout.MaxWidth(COLLUMN_WIDTH));
                    isServer.boolValue = EditorGUILayout.Toggle(isServer.boolValue, GUILayout.MaxWidth(COLLUMN_WIDTH));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("-", GUILayout.Width(ADD_REMOVE_BUTTON_WIDTH)))
                    {
                        list.DeleteArrayElementAtIndex(i);
                        break;
                    }
                EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+", GUILayout.Width(ADD_REMOVE_BUTTON_WIDTH)))
            {
                list.InsertArrayElementAtIndex(Mathf.Max(0, list.arraySize - 1));
                SerializedProperty newEntry = list.GetArrayElementAtIndex(list.arraySize - 1);
                newEntry.FindPropertyRelative(entryPropName).objectReferenceValue = null;
                newEntry.FindPropertyRelative("disableIfNotLocal").boolValue = true;
                newEntry.FindPropertyRelative("disableIfNotClient").boolValue = false;
                newEntry.FindPropertyRelative("disableIfNotServer").boolValue = false;
            }
        EditorGUILayout.EndHorizontal();
    }
}
