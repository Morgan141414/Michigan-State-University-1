using UnityEngine;

public class CometSpawner : MonoBehaviour
{
    public Material cometMaterial;
    public float spawnInterval = 5f;
    public float spawnDistance = 34f;

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnComet();
        }
    }

    private void SpawnComet()
    {
        Vector3 start = new Vector3(
            Random.Range(-spawnDistance, spawnDistance),
            Random.Range(-2f, 5f),
            Random.Range(-spawnDistance, spawnDistance));
        Vector3 destination = Random.insideUnitSphere * 5f;
        Vector3 direction = (destination - start).normalized;

        GameObject comet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        comet.name = "Comet";
        comet.transform.SetParent(transform);
        comet.transform.position = start;
        comet.transform.localScale = Vector3.one * 0.42f;
        comet.GetComponent<Renderer>().sharedMaterial = cometMaterial;
        comet.GetComponent<Collider>().isTrigger = true;

        TrailRenderer trail = comet.AddComponent<TrailRenderer>();
        trail.time = 1.4f;
        trail.startWidth = 0.22f;
        trail.endWidth = 0f;
        trail.material = cometMaterial;

        CometMotion motion = comet.AddComponent<CometMotion>();
        motion.direction = direction;
        motion.speed = Random.Range(6f, 10f);
    }
}