using System;
using UnityEngine;

/// <summary>
/// Draws this ScriptableObject as a regular field, showing all serialized field and children object and allows for changing them
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class InspectableSOAttribute : PropertyAttribute
{

}

