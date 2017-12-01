using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[Flags]
public enum PathListOptions {
    None = 0,
    ListSize = 1,
    ListLabel = 2,
    ElementLabels = 4,
    Default = ListSize | ListLabel | ElementLabels,
    NoElementLabels = ListSize | ListLabel
}

public static class PathPointsList {
    private static GUIContent addButtonContent = new GUIContent("Add new point", "add point");
    private static GUIContent moveDownButtonContent = new GUIContent("▼", "move down");
    private static GUIContent moveUpButtonContent = new GUIContent("▲", "move up");
    private static GUIContent deleteButtonContent = new GUIContent("✖", "delete");
    private static GUILayoutOption miniButtonWidth = GUILayout.Width(20f);

    public static void Show(SerializedProperty list, PathListOptions options = PathListOptions.Default) {
        bool showListLabel = (options & PathListOptions.ListLabel) != 0;
        bool showListSize = (options & PathListOptions.ListSize) != 0;

        if (showListLabel) {
            EditorGUILayout.PropertyField(list);
            EditorGUI.indentLevel += 1;
        }

        if (!showListLabel || list.isExpanded) {
            if (showListSize) EditorGUILayout.PropertyField(list.FindPropertyRelative("Array.size"));
            ShowElements(list, options);
        }
        if (showListLabel) EditorGUI.indentLevel -= 1;
    }

    private static void ShowElements(SerializedProperty list, PathListOptions options) {
        bool showElementLabels = (options & PathListOptions.ElementLabels) != 0;

        for (int i = 0; i < list.arraySize; i++) {
            EditorGUILayout.BeginHorizontal();
            if (showElementLabels) EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i));
            else EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), GUIContent.none);
            ShowButtons(list, i);
            EditorGUILayout.EndHorizontal();
        }
        ShowAddButton(list);
    }

    private static void ShowButtons(SerializedProperty list, int index) {
        Transform pathPoint = ((PathEditor) list.serializedObject.targetObject).pathObjects[index];
        int pointIndex = pathPoint.GetSiblingIndex();

        if (GUILayout.Button(moveUpButtonContent, EditorStyles.miniButtonLeft, miniButtonWidth)) {
            if (pointIndex > 0) pathPoint.SetSiblingIndex(pathPoint.GetSiblingIndex() - 1);
        }
        if (GUILayout.Button(moveDownButtonContent, EditorStyles.miniButtonMid, miniButtonWidth)) {
            if (pointIndex < list.arraySize - 1) pathPoint.SetSiblingIndex(pointIndex + 1);
        }
        if (GUILayout.Button(deleteButtonContent, EditorStyles.miniButtonRight, miniButtonWidth)) {
            GameObject.DestroyImmediate(pathPoint.gameObject);
        }
    }

    private static void ShowAddButton(SerializedProperty list) {
        if (GUILayout.Button(addButtonContent)) {
            List<Transform> pathObjects = ((PathEditor) list.serializedObject.targetObject).pathObjects;
            Transform lastPoint = pathObjects[pathObjects.Count - 1];

            GameObject newPoint = GameObject.Instantiate(lastPoint.gameObject);
            newPoint.transform.parent = lastPoint.parent;
            newPoint.transform.position = lastPoint.transform.position;
            newPoint.name = "PathPoint " + pathObjects.Count.ToString().PadLeft(3, '0');
        }
    }
}