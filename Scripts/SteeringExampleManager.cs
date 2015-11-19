using UnityEngine;
using System.Collections;

public class SteeringExampleManager : MonoBehaviour {

	#region Public instance variables
	[Header("References")]
	[Tooltip("Target transform.")]
	public Transform target;
	[Tooltip("Agent controller.")]
	public Agent agent;

	[Header("Parameters")]
	[Tooltip("Enable obstacles for the agent to avoid.")]
	public bool enableObstacles = true;
	#endregion

	#region Lifecycle
	private void Start () {
	
		// Initialize obstacles
		GameObject.Find ("Obstacles").SetActive (this.enableObstacles);

		// Initialize the seek target of the agent
		this.agent.seekTarget = target;
	}
	#endregion
}
