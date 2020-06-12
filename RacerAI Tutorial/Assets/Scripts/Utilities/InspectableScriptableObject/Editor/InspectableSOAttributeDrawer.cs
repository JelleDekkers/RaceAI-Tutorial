using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// Draws the property field for any field marked with <see cref="InspectableSOAttribute"/>.
/// </summary>
[CustomPropertyDrawer(typeof(InspectableSOAttribute), true)]
public class InspectableSOAttributeDrawer : PropertyDrawer
{
    private readonly float INNER_SPACING = 6.0f;
    private readonly float OUTER_SPACING = 4.0f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float totalHeight = 0.0f;

        totalHeight += EditorGUIUtility.singleLineHeight;

        if (property.objectReferenceValue == null)
            return totalHeight;

        if (!property.isExpanded)
            return totalHeight;

        SerializedObject targetObject = new SerializedObject(property.objectReferenceValue);

        if (targetObject == null)
            return totalHeight;

        SerializedProperty field = targetObject.GetIterator();

        field.NextVisible(true);

        while (field.NextVisible(false))
        {
            totalHeight += EditorGUI.GetPropertyHeight(field, true) + EditorGUIUtility.standardVerticalSpacing;
        }

        totalHeight += INNER_SPACING * 2;
        totalHeight += OUTER_SPACING * 2;

        return totalHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Rect fieldRect = new Rect(position);
        fieldRect.height = EditorGUIUtility.singleLineHeight;

        EditorGUI.PropertyField(fieldRect, property, label, true);
        if (property.objectReferenceValue == null)
        {
            return;
        }

        property.isExpanded = EditorGUI.Foldout(fieldRect, property.isExpanded, GUIContent.none, true);
        if (!property.isExpanded)
        {
            return;
        }

        SerializedObject targetObject = new SerializedObject(property.objectReferenceValue);
        if (targetObject == null)
        {
            return;
        }

        List<Rect> propertyRects = new List<Rect>();
        Rect marchingRect = new Rect(fieldRect);

        Rect bodyRect = new Rect(fieldRect);
        bodyRect.xMin += EditorGUI.indentLevel * 14;
        bodyRect.yMin += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + OUTER_SPACING;

        SerializedProperty field = targetObject.GetIterator();
        field.NextVisible(true);

        marchingRect.y += INNER_SPACING + OUTER_SPACING;

        while (field.NextVisible(false))
        {
            marchingRect.y += marchingRect.height + EditorGUIUtility.standardVerticalSpacing;
            marchingRect.height = EditorGUI.GetPropertyHeight(field, true);
            propertyRects.Add(marchingRect);
        }

        marchingRect.y += INNER_SPACING;
        bodyRect.yMax = marchingRect.yMax;

        DrawBackground(bodyRect);
        DrawField(field, targetObject, propertyRects);
    }

    private void DrawField(SerializedProperty field, SerializedObject targetObject, List<Rect> propertyRects)
    {
        EditorGUI.indentLevel++;

        int index = 0;
        field = targetObject.GetIterator();
        field.NextVisible(true);

        while (field.NextVisible(false))
        {
            try
            {
                EditorGUI.PropertyField(propertyRects[index], field, true);
            }
            catch (StackOverflowException)
            {
                field.objectReferenceValue = null;
                Debug.LogError("Detected self-nesting cauisng a StackOverflowException, avoid using the same object iside a nested structure.");
            }

            index++;
        }

        targetObject.ApplyModifiedProperties();

        EditorGUI.indentLevel--;
    }

    private void DrawBackground(Rect rect)
    {
        EditorGUI.HelpBox(rect, "", MessageType.None);
    }
}
