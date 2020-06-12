using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ColliderUpdateHelper))]
public class ColliderUpdateHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Update Collider"))
        {
            ColliderUpdateHelper obj = ((ColliderUpdateHelper)target);
            MeshCollider col = obj.GetComponent<MeshCollider>();
            if (!col)
                return;

            DestroyImmediate(col);
            obj.gameObject.AddComponent<MeshCollider>();
        }
    }
}
