using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Reflection;

[CustomEditor(typeof(MonoBehaviour), true)]
public class ButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var mono = target as MonoBehaviour;

        var methods = mono.GetType()
            .GetMembers(BindingFlags.Instance | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(o => Attribute.IsDefined(o, typeof(ButtonAttribute)));

        foreach (var memberInfo in methods)
        {
            ButtonAttribute buttonAttribute = memberInfo.GetCustomAttribute<ButtonAttribute>();
            string label = (buttonAttribute.label != string.Empty) ? buttonAttribute.label : memberInfo.Name;

            if (GUILayout.Button(label))
            {
                var method = memberInfo as MethodInfo;
                method.Invoke(mono, null);
            }
        }
    }
}