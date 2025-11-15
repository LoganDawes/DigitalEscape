using UnityEngine;

public class BottomBoundary : MonoBehaviour
{
    private float boundaryY;

    void Start()
    {
        boundaryY = transform.position.y;
    }

    void Update()
    {
        // Find all players in the scene
        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.transform.position.y < boundaryY && player.currentHealth > 0)
            {
                player.DamagePlayer(player.maxHealth);
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // Draw a dotted red horizontal line at boundaryY across the scene
        Gizmos.color = Color.red;
        float lineLength = 100f;
        int segments = 50;
        float xStart = transform.position.x - lineLength / 2f;
        float xEnd = transform.position.x + lineLength / 2f;
        float y = transform.position.y;
        float segmentLength = lineLength / segments;
        for (int i = 0; i < segments; i += 2)
        {
            Vector3 p1 = new Vector3(xStart + i * segmentLength, y, 0f);
            Vector3 p2 = new Vector3(xStart + (i + 1) * segmentLength, y, 0f);
            Gizmos.DrawLine(p1, p2);
        }
    }
#endif
}
