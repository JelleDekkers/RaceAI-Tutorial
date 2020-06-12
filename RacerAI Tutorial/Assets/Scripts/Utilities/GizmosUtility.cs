using UnityEngine;

public static class GizmosUtility
{
    public static void DrawArrow(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.DrawRay(pos, direction);
        DrawArrowHead(pos, direction, Gizmos.color, arrowHeadLength, arrowHeadAngle);
    }

    public static void DrawArrow(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.color = color;
        Gizmos.DrawRay(pos, direction);
        DrawArrowHead(pos, direction, color, arrowHeadLength, arrowHeadAngle);
    }

    private static void DrawArrowHead(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.color = color;

        Vector3 up = Quaternion.LookRotation(direction) * Quaternion.Euler(0, arrowHeadAngle, 0) * Vector3.back;
        Gizmos.DrawRay(pos + direction, up * arrowHeadLength);

        Vector3 down = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -arrowHeadAngle, 0) * Vector3.back;
        Gizmos.DrawRay(pos + direction, down * arrowHeadLength);
    }

    public static void DrawText(string text, Vector3 position, int fontSize = 0)
    {
        DrawText(text, position, Color.black, fontSize);
    }

    public static void DrawText(string text, Vector3 position, Color color, int fontSize = 0)
    {
#if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.normal.textColor = color;
        style.fontSize = fontSize;

        UnityEditor.Handles.BeginGUI();
        UnityEditor.Handles.Label(position, text, style);
        UnityEditor.Handles.EndGUI();
#endif
    }

    /// <summary>
    /// Draws a <see cref="Gizmos.DrawCube(Vector3, Vector3)"/> that will properly rotate and scale to it's transform 
    /// </summary>
    /// <param name="translation"></param>
    /// <param name="center"></param>
    /// <param name="size"></param>
    public static void DrawBox(Transform translation, Vector3 center, Vector3 size)
    {
        DrawBox(translation, center, size, Gizmos.color);
    }

    /// <summary>
    /// Draws a <see cref="Gizmos.DrawCube(Vector3, Vector3)"/> that will properly rotate and scale to it's transform 
    /// </summary>
    /// <param name="translation"></param>
    /// <param name="center"></param>
    /// <param name="size"></param>
    /// <param name="color"></param>
    public static void DrawBox(Transform translation, Vector3 center, Vector3 size, Color color)
    {
        Matrix4x4 prevMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(translation.TransformPoint(-translation.position), translation.rotation, translation.lossyScale);

        Color prevColor = Gizmos.color;
        Gizmos.color = color;
        Gizmos.DrawCube(center, size);
        Gizmos.color = prevColor;

        Gizmos.matrix = prevMatrix;
    }

    /// <summary>
    /// Draws a <see cref="Gizmos.DrawWireCube(Vector3, Vector3)"/> that will properly rotate and scale to it's transform 
    /// </summary>
    /// <param name="translation"></param>
    /// <param name="center"></param>
    /// <param name="size"></param>
    /// <param name="color"></param>
    public static void DrawWireBox(Transform translation, Vector3 center, Vector3 size, Color color)
    {
        Matrix4x4 prevMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(translation.TransformPoint(-translation.position), translation.rotation, translation.lossyScale);

        Color prevColor = Gizmos.color;
        Gizmos.color = color;
        Gizmos.DrawWireCube(center, size);
        Gizmos.color = prevColor;

        Gizmos.matrix = prevMatrix;
    }
}