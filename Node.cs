using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{   
    public float mass;
    public bool processed = false;
    public bool is_fist = false;
    public int health = 100;
    public GameObject explosionObject;  
    private int damage;
    public int activeJointCount;
    private bool dead;
    private bool reviveMode;
    private bool play;
    private Color originalColor;

    private Dictionary<string, SpringJoint> connectedJoints;
    
    void Update(){
        Die();
    }

    void OnCollisionEnter(Collision collision){
        if(collision.gameObject.tag != this.gameObject.tag && collision.gameObject.tag.StartsWith("node")){
            Node colliderNode = collision.gameObject.GetComponent<Node>();
            if(colliderNode.is_fist){
                if(this.is_fist){
                    // Fist Bump
                    this.ReceiveDamage((int)(colliderNode.GetPower() / 2));
                }
                else{
                    // Fist vs Normal
                    this.ReceiveDamage(colliderNode.GetPower());
                }
            }

            // Update Color for Better COLLISION Animation
            gameObject.GetComponent<Renderer>().material.color = new Color (1.0f - originalColor.r, 1.0f - originalColor.g, 1.0f - originalColor.b);
        }
    }
    
    void OnCollisionExit(Collision collision){
        // Revert the Original Color
        gameObject.GetComponent<Renderer>().material.color = originalColor;
    }


    public Node(float mass, Vector3 position){
        this.mass = mass;
        this.transform.position = position;
    }

    public void init(){
        connectedJoints = new Dictionary<string, SpringJoint>();

        // Add Physical Module
        Rigidbody rb = this.gameObject.AddComponent<Rigidbody>();
        rb.mass = this.mass;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        // Keep the Original Color
        originalColor = this.gameObject.GetComponent<Renderer>().material.color;
        
        // General Specs
        activeJointCount = 0;
        damage = 100;
        dead = false;
        reviveMode = false;
        play = false;
    }

    public void attachJoint(Rigidbody joint_object, float strength){
        string joint_name = joint_object.transform.name;
        if(!connectedJoints.ContainsKey(joint_name)){
            connectedJoints[joint_name] = gameObject.AddComponent<SpringJoint>();
        }
        connectedJoints[joint_name].spring = Mathf.Min(strength, 10.0f);
        connectedJoints[joint_name].connectedBody = joint_object;
        AddJoint();
    }

    public void Die(){
        if(!dead && !reviveMode){
            if(health <= 0){
                 // Break joint connections
                foreach(KeyValuePair<string, SpringJoint> name_joint in connectedJoints){
                    SpringJoint joint = name_joint.Value;
                    Rigidbody cBody = joint.connectedBody;
                    if(cBody){    
                        Muscle muscle = cBody.gameObject.GetComponent<Muscle>();
                        if(muscle.IsAlive()){
                            muscle.Suicide();
                            muscle.node_1.GetComponent<Node>().LoseJoint();
                            muscle.node_1.GetComponent<Node>().RemoveJoint(name_joint.Key);
                            muscle.node_2.GetComponent<Node>().LoseJoint();
                            muscle.node_2.GetComponent<Node>().RemoveJoint(name_joint.Key);
                        }
                    }
                    
                }
                // Trigger Explosion Animation
                Explode();

                Suicide();
            }
            else{
                if(activeJointCount <= 0){
                    // Trigger Explosion Animation
                    Explode();
                    Suicide();
                }
            }
        }
    }

    public bool IsAlive(){
        return !dead;
    }

    public void Play(){
        play = true;
    }

    public void Revive(){
        this.dead = false;
        this.reviveMode = true;
        this.health = 100;
        this.activeJointCount = 0;
        BecomeNormal();
        Plug();
    }

    public void Suicide(){
        this.dead = true;
        this.health = 0;
        this.activeJointCount = 0;
        this.play = false;
        BecomeNormal();
        Unplug();
    }

    public void Unplug(){
        gameObject.GetComponent<Rigidbody>().detectCollisions = false;
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        gameObject.GetComponent<Renderer>().enabled = false;
        gameObject.GetComponent<Collider>().enabled = false;

        // Break joint connections
        foreach(KeyValuePair<string, SpringJoint> pos_node in connectedJoints){
            connectedJoints[pos_node.Key].connectedBody = null;
            connectedJoints[pos_node.Key].spring = 0.0f;
        }
    }

    public void RemoveJoint(string joint_name){
        if(connectedJoints.ContainsKey(joint_name)){
            if(connectedJoints[joint_name].connectedBody){
                connectedJoints[joint_name].connectedBody = null;
                connectedJoints[joint_name].spring = 0.0f;
            }   
        }
    }

    public void Plug(){
        gameObject.GetComponent<Rigidbody>().detectCollisions = true;
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        gameObject.GetComponent<Renderer>().enabled = true;
        gameObject.GetComponent<Collider>().enabled = true;
    }

    public void BecomeFist(Color fist_color){
        SetColor(fist_color);
        gameObject.GetComponent<Renderer>().material.color = fist_color;
        gameObject.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
        transform.localScale *= 1.6f;
        is_fist = true;
    }

    public void BecomeNormal(){
        SetColor(originalColor);
        gameObject.GetComponent<Renderer>().material.color = originalColor;
        gameObject.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
        if(is_fist){
            transform.localScale /= 1.6f;
            is_fist = false;
        }
    }

    public void AddJoint(){
        activeJointCount++;
        if(reviveMode){
            reviveMode = false;
        }
    }

    public void LoseJoint(){
        activeJointCount--;
    }

    public void Specs(){
        Debug.Log("Node : " + transform.name + " Active Joint Count : " + activeJointCount + " Health : " + health);
    }

    public void ReceiveDamage(int damage){
        health -= damage;
    }

    public int GetPower(){
        return damage;
    }

    public void SetPower(int power){
        this.damage = power;
    }
    public void SetColor(Color color){
        this.originalColor = color;
    }
    
    private void Explode(){
        // Instantiate Explosion
        Instantiate(explosionObject, transform.position, Quaternion.identity);
    }

}
