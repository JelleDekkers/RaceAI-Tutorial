using UnityEngine;

public class CarCameraController : MonoBehaviour
{
    [SerializeField] private Rigidbody target = null;
    [SerializeField] private float cameraHeight = 0;
    [SerializeField] private float dampStrength = 0.15f;
    [SerializeField] private float velocityStrength = 0.5f;

    private new Camera camera;
    private Vector3 velocity = Vector3.zero;

    private void Awake()
    {
        camera = GetComponent<Camera>();
    }

    private void Start()
    {
        transform.position = GetTargetDestination();
    }

    private void FixedUpdate()
    {
        Vector3 destination = GetTargetDestination();
        destination += target.velocity * velocityStrength;
        transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampStrength);
    }

    private Vector3 GetTargetDestination()
    {
        Vector3 delta = target.position - camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, cameraHeight));
        return transform.position + delta;
    }
}
