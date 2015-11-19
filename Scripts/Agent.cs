using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]
[RequireComponent (typeof (Collider))]
public class Agent : MonoBehaviour {

	#region Public instance variables
	[Header("Locomotion")]
	[Tooltip("Mass (kg).")]
	public float mass = 1f;
	[Tooltip("Maximum speed (m/s)")]
	public float maxSpeed = 0.5f;
	[Tooltip("Maximum force (N)")]
	public float maxForce = 1f;

	[Header("Arrive Behavior")]
	[Tooltip("Arrive deceleration radius (m)")]
	public float decelerationRadius = 1f;

	[Header("Obstacle Avoidance Behavior")]
	[Tooltip("Avoidance radius (m). The desired amount of space between the agent and obstacles.")]
	public float avoidanceRadius = 0.02f;
	[Tooltip("Forward detection radius (m). The distance in front of the agent that is checked for obstacles.")]
	public float forwardDetection = 0.3f;

	[HideInInspector]
	public Transform seekTarget;	// Target for the agent to move towards
	#endregion

	#region Private instance variables
	private Rigidbody rb;					// Rigidbody component on the agent
	private Collider col;					// Collider component on the agent
	private Transform forwardIndicator;		// Forward heading indicator transform
	private Transform desiredIndicator;		// Desired heading indicator transform
	#endregion

	#region Lifecycle
	private void Start () {
	
		// Initialize the rigidbody
		this.rb = GetComponent<Rigidbody> ();
		this.rb.mass = this.mass;
		this.rb.useGravity = false;

		// Initialize the collider
		this.col = GetComponent<Collider> ();

		// Initialize the forward and desired velocity indicators
		this.forwardIndicator = this.transform.FindChild("ForwardParent");
		this.desiredIndicator = this.transform.FindChild("DesiredParent");
	}

	private void FixedUpdate () {

		// Calculate the total steering force by summing the active steering behaviors
		Vector3 force = Arrive () + ObstacleAvoidance();

		// Upper bound on force
		if (force.magnitude > this.maxForce) {
			force = force.normalized * this.maxForce;
		}

		// Apply the force
		this.rb.AddForce (force, ForceMode.Force);      

		// Upper bound on speed
		if (this.rb.velocity.magnitude > this.maxSpeed) {
			this.rb.velocity = this.rb.velocity.normalized * this.maxSpeed;
		}

		UpdateVelocityIndicators (force, rb.velocity);
	}
	#endregion

	#region Public instance methods
	/// <summary>
	/// Steering behavior that steers towards a target at max speed.
	/// </summary>
	private Vector3 Seek () {

		Vector3 desiredVelocity = (this.seekTarget.position - this.transform.position).normalized * this.maxSpeed;
		return desiredVelocity - this.rb.velocity;
	}
	
	/// <summary>
	/// Steering behavior that steers towards a target and attempts to slow down before it reaches the target. 
	/// Depending on the deceleration radius, max speed, and max force, it may not be able to come to a complete stop.
	/// </summary>
	private Vector3 Arrive () {

		// Scale the speed porporitionally to the distance from the target when within the deceleration radius
		Vector3 targetOffset = this.seekTarget.position - this.transform.position;
		float distance = targetOffset.magnitude;
		float scaledSpeed = (distance / this.decelerationRadius) * this.maxSpeed;
		float desiredSpeed = Mathf.Min (scaledSpeed, this.maxSpeed);

		// Compute the steering force
		Vector3 desiredVelocity = targetOffset.normalized * desiredSpeed;
		return desiredVelocity - this.rb.velocity;
	}

	/// <summary>
	/// A steering behavior that detects obstacles ahead of the agent 
	/// </summary>
	/// <returns>The avoidance.</returns>
	private Vector3 ObstacleAvoidance () {

		Vector3 steeringForce = Vector3.zero;

		// Cast a sphere, that bounds the avoidance zone of the agent, to detect obstacles
		RaycastHit[] hits = Physics.SphereCastAll(this.transform.position, this.col.bounds.extents.x + this.avoidanceRadius, this.rb.velocity, this.forwardDetection); 
		
		// Compute and sum the forces across all hits
		for(int i = 0; i < hits.Length; i++)	{

			// Ensure that the collidier is on a different object
			if (hits[i].collider.gameObject.GetInstanceID () != this.gameObject.GetInstanceID ()) {

				if (hits[i].distance > 0) {

					// Scale the force inversely proportional to the distance to the target
					float scaledForce = ((this.forwardDetection - hits[i].distance) / this.forwardDetection) * this.maxForce;
					float desiredForce = Mathf.Min (scaledForce, this.maxForce);

					// Compute the steering force
					steeringForce += hits[i].normal * desiredForce;
				}
			}
		}				

		return steeringForce;
	}

	private void UpdateVelocityIndicators (Vector3 desiredVelocity, Vector3 forwardVelocity) {

		// Rotate the desired velocity indicator to match the desired velocity.
		if (desiredVelocity.magnitude > 0.00001) {
			this.desiredIndicator.rotation = Quaternion.LookRotation (desiredVelocity.normalized);
		}

		// Rotate the forward velocity indicator to match the current velocity.
		if (forwardVelocity.magnitude > 0.00001) {
			this.forwardIndicator.rotation = Quaternion.LookRotation (forwardVelocity.normalized);
		}
	}
	#endregion
}





















