
using UnityEngine;

/*
	Meteor.cs : Hazards
	Handles meteor movement and destruction on collision with specified layers.
*/


[RequireComponent(typeof(Rigidbody2D))]
public class Meteor : MonoBehaviour
{
	[Header("Meteor Fall Settings")]
	[SerializeField] private float minSpeed = 8f;
	[SerializeField] private float maxSpeed = 16f;
	[SerializeField] private float minAngle = 75f;
	[SerializeField] private float maxAngle = 105f;

	[Header("Collision Settings")]
	[SerializeField] private LayerMask[] destroyOnLayers;

	[Header("Rotation Settings")]
	[SerializeField] private float minRotationSpeed = -180f; // degrees per second
	[SerializeField] private float maxRotationSpeed = 180f;

	[Header("Lifespan Settings")]
	[SerializeField] private float lifespan = 8f;

	private Rigidbody2D rb;
	private float rotationSpeed;
	private float lifeTimer = 0f;

	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		rb.bodyType = RigidbodyType2D.Dynamic;
		rb.gravityScale = 0f;
		rb.linearVelocity = GetRandomizedVelocity();
		rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
		lifeTimer = 0f;
	}
	void Update()
	{
		// Rotate meteor
		transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

		// Lifespan check
		lifeTimer += Time.deltaTime;
		if (lifeTimer >= lifespan)
		{
			Destroy(gameObject);
		}
	}

	private Vector2 GetRandomizedVelocity()
	{
		float angle = Random.Range(minAngle, maxAngle);
		float speed = Random.Range(minSpeed, maxSpeed);
		float angleRad = angle * Mathf.Deg2Rad;
		return new Vector2(Mathf.Cos(angleRad), -Mathf.Sin(angleRad)) * speed;
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		int otherLayer = collision.gameObject.layer;
		foreach (var mask in destroyOnLayers)
		{
			if ((mask.value & (1 << otherLayer)) != 0)
			{
				Destroy(gameObject);
				break;
			}
		}
	}
}
