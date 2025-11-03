
using UnityEngine;

/*

    LaserBlaster.cs : Hazards
    Emits a laser beam that deals damage to the player on contact.

*/

public class LaserBlaster : HazardBase, IActivatable
{
    [Header("Laser Settings")]
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private float laserWidth = 0.1f;
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private LayerMask hitMask;

    private GameObject currentLaser;
    [SerializeField] private bool isActive = false;

    void Start()
    {
        if (isActive)
        {
            FireLaser();
        }
    }

    public void onActivated()
    {
        if (!isActive)
        {
            isActive = true;
            FireLaser();
        }
        else
        {
            isActive = false;
            StopLaser();
        }
    }

    private void FireLaser()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.right;
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxDistance, hitMask);
        float distance = hit.collider ? hit.distance : maxDistance;
        Vector3 endPoint = origin + direction * distance;

        // Create or update the laser visual
        if (currentLaser == null)
        {
            currentLaser = new GameObject("Laser");
            currentLaser.tag = "Hazard";
            currentLaser.transform.parent = this.transform;
            var lr = currentLaser.AddComponent<LineRenderer>();
            lr.startWidth = laserWidth;
            lr.endWidth = laserWidth;
            lr.positionCount = 2;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.red;
            lr.endColor = Color.red;
            // Add collider for hazard detection
            var box = currentLaser.AddComponent<BoxCollider2D>();
            box.isTrigger = false;
        }

        // Set laser positions
        var line = currentLaser.GetComponent<LineRenderer>();
        line.SetPosition(0, origin);
        line.SetPosition(1, endPoint);

    // Adjust collider size and position
    var boxCol = currentLaser.GetComponent<BoxCollider2D>();
    float length = distance;
    boxCol.size = new Vector2(length, laserWidth);
    // Offset the collider so it starts at the origin and extends forward
    boxCol.offset = new Vector2(length / 2f, 0f);
    boxCol.isTrigger = false;
    // Position the laser at the origin
    currentLaser.transform.position = origin;
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    currentLaser.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void StopLaser()
    {
        if (currentLaser != null)
        {
            Destroy(currentLaser);
            currentLaser = null;
        }
    }

    private void OnDestroy()
    {
        StopLaser();
    }
}
