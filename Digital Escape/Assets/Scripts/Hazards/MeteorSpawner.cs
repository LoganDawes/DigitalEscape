using UnityEngine;

/*

    MeteorSpawner.cs : Hazards
    Spawns meteors that fall from the sky and deal damage to the player on contact.

*/

public class MeteorSpawner : MonoBehaviour
{
    [Header("Meteor Spawner Settings")]
    [SerializeField] private GameObject meteorPrefab;
    [SerializeField] private float minSpawnInterval = 1.0f;
    [SerializeField] private float maxSpawnInterval = 3.0f;
    [SerializeField] private float horizontalSpawnRange = 5.0f;

    private float nextSpawnTime = 0f;

    void Start()
    {
        ScheduleNextSpawn();
    }

    void Update()
    {
        if (meteorPrefab == null)
            return;

        if (Time.time >= nextSpawnTime)
        {
            SpawnMeteor();
            ScheduleNextSpawn();
        }
    }

    void SpawnMeteor()
    {
        // Determine spawn position
        Vector3 spawnPos = transform.position;
        if (horizontalSpawnRange > 0f)
        {
            float offset = Random.Range(-horizontalSpawnRange / 2f, horizontalSpawnRange / 2f);
            spawnPos.x += offset;
        }
        Instantiate(meteorPrefab, spawnPos, Quaternion.identity);
    }

    void ScheduleNextSpawn()
    {
        float interval = Random.Range(minSpawnInterval, maxSpawnInterval);
        nextSpawnTime = Time.time + interval;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = transform.position;
        Vector3 left = center + Vector3.left * (horizontalSpawnRange / 2f);
        Vector3 right = center + Vector3.right * (horizontalSpawnRange / 2f);
        Gizmos.DrawLine(left, right);
        Gizmos.DrawWireSphere(left, 0.2f);
        Gizmos.DrawWireSphere(right, 0.2f);
    }
#endif
}
