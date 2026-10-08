using UnityEngine;

public class OrbitMotion : MonoBehaviour
{
    public float radius = 8f;
    public float degreesPerSecond = 8f;
    public float phaseDegrees;

    private float elapsed;

    private void Start()
    {
        elapsed = phaseDegrees;
    }

    private void Update()
    {
        elapsed += degreesPerSecond * Time.deltaTime;
        float radians = elapsed * Mathf.Deg2Rad;
        transform.localPosition = new Vector3(Mathf.Cos(radians) * radius, 0f, Mathf.Sin(radians) * radius);
    }
}