using UnityEngine;

[ExecuteInEditMode]
public class CarPlacementManager : MonoBehaviour
{
    [SerializeField] private float horizontalDistanceBetweenPoints = 3f;
    [SerializeField] private float verticalDistanceBetweenPoints = 6f;

    private void OnTransformChildrenChanged()
    {
        SortChildren();
    }

    private void OnValidate()
    {
        SortChildren();
    }

    [ContextMenu("Sort")]
    private void SortChildren()
    {
        int row = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            Vector3 pos = Vector3.zero;
            pos.z -= row * verticalDistanceBetweenPoints;

            if (i % 2 != 0)
            {
                pos.z -= horizontalDistanceBetweenPoints / 2;
                row++;
            }

            if(i % 2 == 1)
                pos.x += horizontalDistanceBetweenPoints;
            
            transform.GetChild(i).localPosition = pos;
            transform.GetChild(i).localEulerAngles = transform.forward;
        }
    }
}
