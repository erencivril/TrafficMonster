using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.2f;

    private float shakeTimer = 0f;
    private Vector3 shakeOffset = Vector3.zero;

    private void Update()
    {
        if (shakeTimer > 0)
        {
            shakeOffset = Random.insideUnitSphere * shakeMagnitude;
            shakeTimer -= Time.unscaledDeltaTime;
        }
        else
        {
            shakeOffset = Vector3.zero;
        }
    }


    public void TriggerShake()
    {
        shakeTimer = shakeDuration;
    }

    public Vector3 GetOffset()
    {
        return shakeOffset;
    }
}