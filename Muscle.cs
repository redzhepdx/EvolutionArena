using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Muscle : MonoBehaviour
{
    public GameObject node_1;
    public GameObject node_2;
	public Material animation_material;
	public Color lineColor;

    public float strength = 0.1f;
	public float min_dist = 1.5f;
	public float max_dist = 4.0f;

	private LineRenderer lineRenderer;

	private Vector3 relative_speed_11;
    private Vector3 relative_speed_21;
    private Vector3 relative_speed_12;
    private Vector3 relative_speed_22;
	private Vector3 pos_1;
	private Vector3 pos_2;

	// Idle Node Detection Variables
	private float dist_1;
	private float dist_2;
	private float idleTimer;
	private float maxNodeIdleTime = 2.0f;
	private Vector3 node_I_start_pos;
	private Vector3 node_II_start_pos;

	// State Machine Variables	
	private bool move_towards_1 = true;
    private bool move_towards_2 = true;

	public bool alive;
	private int counter = 0;
	private bool play;

	// Call this function at the beginning of the game
	// void Start(){
	// 	Init();
	// }

    // Update is called once per frame
    void Update(){
		if(IsAlive() && play){
			pos_1 = node_1.transform.position;
			pos_2 = node_2.transform.position;

			relative_speed_11 = (transform.position - pos_1).normalized;
			relative_speed_21 = (pos_1 - transform.position).normalized;

			relative_speed_12 = (transform.position - pos_2).normalized;
			relative_speed_22 = (pos_2 - transform.position).normalized;
				
			dist_1 = Vector3.Distance(transform.position, pos_1);
			dist_2 = Vector3.Distance(transform.position, pos_2);
			
			MuscleStateMachine();

			UpdateMuscleAnimator();

			// Muscle needs to be in between two nodes
			transform.position = (pos_1 + pos_2) / 2;
			
			IdleDetection();

		}
    }

	public void Init(){
		CreateMuscleAnimator();
		alive = true;
		play = false;
		idleTimer = 0.0f;
		node_I_start_pos = node_1.transform.position;
		node_II_start_pos = node_2.transform.position;
	}

	public bool IsAlive(){
		return alive;
	}

	public void Play(){
		play = true;
	}

	public void Revive(){
		Plug();
	}

	public void Suicide(){
		Unplug();
	}

	public void Unplug(){
		play = false;
		alive = false;
        gameObject.GetComponent<Rigidbody>().detectCollisions = false;
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        gameObject.GetComponent<Collider>().enabled = false;
		gameObject.GetComponent<LineRenderer>().enabled = false;
    }

    public void Plug(){
		alive = true;
        gameObject.GetComponent<Rigidbody>().detectCollisions = true;
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        gameObject.GetComponent<Collider>().enabled = true;
		gameObject.GetComponent<LineRenderer>().enabled = true;
    }

	public void UpdatePosition(){
		pos_1 = node_1.GetComponent<Transform>().transform.position;
		pos_2 = node_2.GetComponent<Transform>().transform.position;
		transform.position = (pos_1 + pos_2) / 2;
	}

	public void UpdateStrength(float strength, string creature_type){
		this.strength = strength;

		// Line Renderer Update
		gameObject.GetComponent<LineRenderer>().startWidth = strength;
		gameObject.GetComponent<LineRenderer>().endWidth = strength;

		 // Update Color of the Muscle
		if(creature_type == "X"){
			this.lineColor = new Color(1.0f - strength, 0.0f, 0.0f);
		}
		else{
			this.lineColor = new Color(0.0f, 1.0f - strength, 0.0f);
		}
	}

	private void UpdateMuscleAnimator(){
		lineRenderer.SetPosition (0, pos_1);
		lineRenderer.SetPosition (1, pos_2);
	}

	private void CreateMuscleAnimator(){
		Material mat = new Material(Shader.Find("Standard"));
		mat.color = lineColor;
		
		lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.startWidth = strength;
		lineRenderer.endWidth = strength;
		lineRenderer.positionCount = 2;
		lineRenderer.receiveShadows = false;
		lineRenderer.material = mat;
	}

	private void IdleDetection(){

		idleTimer += Time.deltaTime;
		if(idleTimer >= maxNodeIdleTime){

			if(Vector3.Distance(node_1.transform.position, node_I_start_pos) <= 0.1f){
				move_towards_1 = !move_towards_1;
			}

			if(Vector3.Distance(node_2.transform.position, node_II_start_pos) <= 0.1f){
				move_towards_2 = !move_towards_2;
			}

			// Reset Start Positions
			node_I_start_pos = node_1.transform.position;
			node_II_start_pos = node_2.transform.position;

			// Reset Timer
			idleTimer = 0.0f;
		}
		
		
	}

	private void MuscleStateMachine(){
		counter++;
		if ((dist_1 >= min_dist) && move_towards_1 && (dist_2 >= min_dist) && move_towards_2){
			if(counter % 2 == 1){
				node_1.GetComponent<Rigidbody>().velocity += new Vector3(
							strength * relative_speed_11.x, 	
							strength * relative_speed_11.y, 
							strength * relative_speed_11.z);
				node_2.GetComponent<Rigidbody>().velocity += new Vector3(
								strength * relative_speed_12.x, 	
								strength * relative_speed_12.y, 
								strength * relative_speed_12.z);
			}
			else{
				node_2.GetComponent<Rigidbody>().velocity += new Vector3(
							strength * relative_speed_12.x, 	
							strength * relative_speed_12.y, 
							strength * relative_speed_12.z);
				node_1.GetComponent<Rigidbody>().velocity += new Vector3(
							strength * relative_speed_11.x, 	
							strength * relative_speed_11.y, 
							strength * relative_speed_11.z);
			}
			
		}
		else{
			move_towards_1 = false;
			move_towards_2 = false;	
		}

		if ((dist_1 <= max_dist) && !move_towards_1 && (dist_2 <= max_dist) && !move_towards_2) {
			if(counter % 2 == 1){
				node_1.GetComponent<Rigidbody>().velocity += new Vector3(
								strength * relative_speed_21.x, 	
								strength * relative_speed_21.y, 
								strength * relative_speed_21.z);

				node_2.GetComponent<Rigidbody>().velocity += new Vector3(
								strength * relative_speed_22.x, 	
								strength * relative_speed_22.y, 
								strength * relative_speed_22.z);
			}
			else{
				node_2.GetComponent<Rigidbody>().velocity += new Vector3(
								strength * relative_speed_22.x, 	
								strength * relative_speed_22.y, 
								strength * relative_speed_22.z);
				node_1.GetComponent<Rigidbody>().velocity += new Vector3(
								strength * relative_speed_21.x, 	
								strength * relative_speed_21.y, 
								strength * relative_speed_21.z);
			}	
		}
		else {
			move_towards_1 = true;
			move_towards_2 = true;
		}
	}

}
