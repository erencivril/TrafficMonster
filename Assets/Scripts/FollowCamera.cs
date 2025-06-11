using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 5f, -10f);
    public float followSpeed = 5f;
    public CameraShake cameraShake;

    private Vector3 velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position + offset;

            Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 1f / followSpeed);

            if (cameraShake != null)
                smoothedPosition += cameraShake.GetOffset();

            transform.position = smoothedPosition;
        }
    }
}