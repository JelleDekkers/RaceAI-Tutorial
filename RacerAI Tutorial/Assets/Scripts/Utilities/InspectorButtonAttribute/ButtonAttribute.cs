using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.Method)]
public class ButtonAttribute : PropertyAttribute
{
    public string label = string.Empty;

    /// <summary>
    /// Creates a button in the inspector, uses the method name as the button's label
    /// </summary>
    public ButtonAttribute() { }

    /// <summary>
    /// Creates a button in the inspector 
    /// </summary>
    /// <param name="label">Button label</param>
    public ButtonAttribute(string label)
    {
        this.label = label;
    }
}