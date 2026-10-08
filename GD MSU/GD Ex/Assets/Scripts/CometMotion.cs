using UnityEngine;

public class CometMotion : MonoBehaviour
{
    public Vector3 direction;
    public float speed = 7f;
    public float lifetime = 18f;

    private void Update()
    {
        transform.position += direction * (speed * Time.deltaTime);
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
        {
            Destroy(gameObject);
        }
    }
}