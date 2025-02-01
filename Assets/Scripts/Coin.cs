using UnityEngine;

/// <summary>
/// Handles collectible coin behavior with optional magnetic attraction to the player.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class Coin : MonoBehaviour 
{
	[Header("Collection")]
	[Tooltip("Sound played when coin is collected")]
	public AudioClip pickupSound;
	[Tooltip("Whether the coin should magnetically attract to the player")]
	public bool hasAttractionEffect = true;
	[Tooltip("Distance at which the coin is automatically collected")]
	public float pickupDistance = 1f;

    [Tooltip("Amount of coins to grant the player for picking it up.")]
    public int pickupAmount = 1;

    [Header("Animation")]
	[Tooltip("Base rotation speed and direction")]
	public Vector3 baseRotationSpeed = new Vector3(0, 80, 0);
	[Tooltip("Additional rotation when attracting to player")]
	public Vector3 attractionSpinBoost = new Vector3(10, 20, 10);

	[Header("Attraction Settings")]
	[Tooltip("Radius within which coin starts moving towards player")]
	public float attractionRadius = 7f;
	[Tooltip("Initial movement speed towards player")]
	public float initialAttractionSpeed = 3f;
	[Tooltip("How quickly attraction speed increases")]
	public float attractionAcceleration = 0.2f;

	private bool _attractingToPlayer = false;
	private float currentAttractionSpeed = 0f;
	private Vector3 currentRotationSpeed = Vector3.zero;
	private SphereCollider coinCollider;
	
	/// <summary>
	/// Sets up the coin's collection radius and validates components
	/// </summary>
	void Awake()
	{
		if (!CompareTag("Coin"))
			tag = "Coin";

		currentRotationSpeed = baseRotationSpeed;
		currentAttractionSpeed = initialAttractionSpeed;
	}
	
	/// <summary>
	/// Handles coin rotation and magnetic attraction behavior
	/// </summary>
	void Update() {
		transform.Rotate(currentRotationSpeed * Time.deltaTime, Space.World);
	}
	
	
	void OnTriggerStay(Collider other)
	{
		if (!other.CompareTag("Player")) return;

		if (hasAttractionEffect)
		{
			if (!_attractingToPlayer)
				BeginAttraction();
			else
				UpdateAttraction(other);
		}

		// Check if close enough to collect
		float distanceToPlayer = Vector3.Distance(transform.position, other.transform.position);
		if (distanceToPlayer <= pickupDistance)
		{
			CollectCoin();
		}
	}
	
	private void BeginAttraction()
	{
        _attractingToPlayer = true;
		currentRotationSpeed = baseRotationSpeed;
		currentAttractionSpeed = initialAttractionSpeed;
	}
	
	private void UpdateAttraction(Collider other)
	{
		currentAttractionSpeed += attractionAcceleration;
		currentRotationSpeed += attractionSpinBoost;
		
		transform.position = Vector3.Lerp(transform.position, other.transform.position, currentAttractionSpeed * Time.deltaTime);
	}
	
	private void CollectCoin()
	{
		if (pickupSound)
		{
			AudioSource.PlayClipAtPoint(pickupSound, transform.position);
		}
		
		GameManager.Instance.CoinsCollected += pickupAmount;
		
		Destroy(gameObject);
	}
}
