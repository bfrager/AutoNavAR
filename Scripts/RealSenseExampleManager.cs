using UnityEngine;
using System.Collections;

public class RealSenseExampleManager : MonoBehaviour {

	#region Enumerations
	// State machine that controls the scene:
	// 		Start => SceneInitialized -> TargetInitialized -> AgentInitialized
	private enum SceneState {SceneInitialized, TargetInitialized, AgentInitialized};
	#endregion

	#region Public instance variables
	[Tooltip("Target prefab.")]
	public GameObject targetPrefab;
	[Tooltip("Agent prefab.")]
	public GameObject agentPrefab;
	#endregion

	#region Private instance variables
	private SceneState state = SceneState.SceneInitialized;	// Initial scene state.
	private GameObject target;								// Reference to the spawned target.
	private Agent agent;									// Reference to the spawned agent.
	#endregion

	#region Lifecycle
  	private void Update () {

		// Trigger when the user "clicks" with either the mouse or a touch up gesture.
		if(Input.GetMouseButtonUp (0)) {
			TouchHandler ();
		}
	}
	#endregion

	#region Private instance methods
	/// <summary>
	/// Handler for touch events. 
	/// </summary>
	private void TouchHandler () {

		RaycastHit hit;

		// Raycast from the point touched on the screen
		if (Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit)) {
		
			if (hit.collider.gameObject.name == "meshPrefab(Clone)") {
			
				switch (this.state) {
				case SceneState.SceneInitialized:
					SpawnTarget (hit);
					this.state = SceneState.TargetInitialized;
					break;
				case SceneState.TargetInitialized:
					SpawnAgent (hit);
					this.state = SceneState.AgentInitialized;
					break;
				case SceneState.AgentInitialized:
					MoveTarget (hit);
					break;
				default:
					Debug.LogError("Invalid scene state.");
					break;
				}
			}
		}
	}
	
	/// <summary>
	/// Spawn the target at the point of the raycast hit. 
	/// </summary>
	/// <param name="hit">RaycastHit from user input.</param>
	private void SpawnTarget (RaycastHit hit) {

		// Instantiate the target prefab at the point of the raycast
		target = Instantiate (targetPrefab, hit.point, Quaternion.FromToRotation(Vector3.up, AverageMeshNormal (MeshFromRaycastHit (hit)))) as GameObject ;
	}

	/// <summary>
	/// Spawn the agent above the generated mesh at the point of the raycast hit.
	/// </summary>
	/// <param name="hit">RaycastHit from user input.</param>
	private void SpawnAgent (RaycastHit hit) {

		// Instantiate the agent prefab above the point of the raycast
		GameObject agentClone = Instantiate (agentPrefab, hit.point + (Vector3.up * 0.1f), Quaternion.identity) as GameObject;

		// Obtain reference to the agent controller and set the initial seek target.
		agent = agentClone.GetComponent<Agent> ();
		agent.seekTarget = target.transform.FindChild("Target");
	}


	/// <summary>
	/// Move the target to the new position at the point of the raycast hit.
	/// </summary>
	/// <param name="hit">RaycastHit from user input.</param>
	private void MoveTarget (RaycastHit hit) {

		target.transform.position = hit.point;
		target.transform.rotation = Quaternion.FromToRotation (Vector3.up, AverageMeshNormal (MeshFromRaycastHit (hit)));
	} 

	/// <summary>
	/// Compute the average normal of a mesh. Vertices in a mesh can be prone to variance. 
	/// If the vertices are in a known plane, then the average normal can be computed which will approximate the normal of the plane.
	/// </summary>
	/// <returns>Average normal of the mesh.</returns>
	/// <param name="mesh">Input mesh.</param>
	private Vector3 AverageMeshNormal(Mesh mesh) {

		Vector3 sum = Vector3.zero; 

		// Sum all the normals in the mesh.
		for (int i = 0; i < mesh.normals.Length; i++){
			sum += mesh.normals[i];
		}
	
		// Return the average
		return sum / mesh.normals.Length; 
	}

	/// <summary>
	/// Return the mesh connected to the collider of a RaycastHit.
	/// </summary>
	/// <returns>Mesh.</returns>
	/// <param name="hit">Input RaycastHit.</param>
	private Mesh MeshFromRaycastHit (RaycastHit hit) {

		return hit.collider.gameObject.GetComponent<MeshFilter> ().mesh;
	}
	#endregion
}
