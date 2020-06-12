using UnityEngine;

public static class DebugHelper
{
    public static void DebugCross(Vector3 position, float size = 0.5f, Color color = default, float duration = 0f, bool depthTest = true)
    {
        Vector3 xStart = position + Vector3.right * size * 0.5f;
        Vector3 xEnd = position - Vector3.right * size * 0.5f;

        Vector3 yStart = position + Vector3.up * size * 0.5f;
        Vector3 yEnd = position - Vector3.up * size * 0.5f;

        Vector3 zStart = position + Vector3.forward * size * 0.5f;
        Vector3 zEnd = position - Vector3.forward * size * 0.5f;

        Debug.DrawLine(xStart, xEnd, color, duration, depthTest);
        Debug.DrawLine(yStart, yEnd, color, duration, depthTest);
        Debug.DrawLine(zStart, zEnd, color, duration, depthTest);
    }
}
