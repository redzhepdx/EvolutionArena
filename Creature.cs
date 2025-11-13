using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EvolutionUtils;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class Creature : MonoBehaviour
{   
    public Node node; // Node object
    public Muscle joint; // Joint Object    
    public Color fist_color = new Color (0.0f, 0.6f, 0.6f, 1.0f); // Fist Color


    public float MIN_MUSCLE_STRENGTH = 0.1f;
    public float NODE_MASS = 1.0f;
    public float node_offset = 3.0f;
    public uint edge_node_size = 4;
    public int max_connection_count_per_node = 2;
    public uint total_fist_count = 2;
    
    
    private uint layer_size;
    private uint total_node_count;
    private bool dead;
    private string creature_type;
    private string creature_name;
    private float[,] node_connections;
    private Dictionary<uint, HashSet<uint>> nodes_graph;
    
    [SerializeField]
    private Dictionary<Vector3, Node> nodes;

    [SerializeField]
    private Dictionary<Tuple<uint, uint>, Muscle> joints;
    private Vector3 pos; // Position of Creature
    
    void Update()
    {
        if(!dead){    
            Die();
            UpdatePosition();
        }
    }

    public void Init(){
        
        this.layer_size = this.edge_node_size * this.edge_node_size;
        this.total_node_count = this.edge_node_size * this.layer_size;

	    node_connections = new float[total_node_count, total_node_count];
        nodes_graph = new Dictionary<uint, HashSet<uint>>();

        nodes = new Dictionary<Vector3, Node>();
        joints = new Dictionary<Tuple<uint, uint>, Muscle>();
    }

    public void UpdateMusclesRandomly(){
        // Create a new node_connection matrix
        for(uint x = 0; x < total_node_count; ++x){
            for(uint y = x; y < total_node_count; ++y){
                
                if(node_connections[x, y] > MIN_MUSCLE_STRENGTH){
                    float strength = Random.Range(-1.0f, 1.0f);
                    node_connections[x, y] = strength;

                    // Update Muscle
                    if(strength < MIN_MUSCLE_STRENGTH){
                        // Cut the muscle
                        joints[new Tuple<uint, uint>(x, y)].Suicide();

                        // Make Nodes Lose Joints
                        nodes[Utils.index2position(x, node_offset, layer_size, edge_node_size)].LoseJoint();
                        nodes[Utils.index2position(y, node_offset, layer_size, edge_node_size)].LoseJoint();
                    }
                    else{
                        // Update the muscle
                        joints[new Tuple<uint, uint>(x, y)].UpdateStrength(strength, this.GetCreatureType());
                    }

                    // Add Muscle ? 
                }
            }
        }
    }

    public void UpdateMusclesWithAI(float[,] ai_update){
        // This function is useless for now
        // It will be usefull after adding the AI module
        for(uint x = 0; x < total_node_count; ++x){
            for(uint y = x; y < total_node_count; ++y){
                
                if(node_connections[x, y] > MIN_MUSCLE_STRENGTH){
                    float strength = ai_update[x, y];
                    node_connections[x, y] = strength;

                    // Update Muscle
                    if(strength < MIN_MUSCLE_STRENGTH){
                        // Cut the muscle
                        joints[new Tuple<uint, uint>(x, y)].Suicide();
                        // Make Nodes Lose Joints
                        nodes[Utils.index2position(x, node_offset, layer_size, edge_node_size)].LoseJoint();
                        nodes[Utils.index2position(y, node_offset, layer_size, edge_node_size)].LoseJoint();
                    }
                    else{
                        // Update the muscle
                        joints[new Tuple<uint, uint>(x, y)].UpdateStrength(strength, this.GetCreatureType());
                    }

                    // Add Muscle ? 
                }
            }
        }
    }

    public uint GetTotalNodeCount(){
        return total_node_count;
    }

    public Vector3 GetPos(){
        return pos;
    }

    public void SetCreatureType(string type){
        creature_type = type;
    }

    public string GetCreatureType(){
        return creature_type;
    }

    public void SetCreatureName(string name){
        creature_name = name;
    }

    public string GetCreatureName(){
        return creature_name;
    }


    public void AssignFists()
    {
        HashSet<uint> generatedFists = new HashSet<uint>(); 
        while(generatedFists.Count < total_fist_count * 2){
            uint node_1 = (uint)Random.Range(0, total_node_count - 1);
            uint node_2 = (uint)Random.Range(0, total_node_count - 1);
            Tuple<uint, uint> connection = new Tuple<uint, uint>(node_1, node_2);

            while(node_connections[node_1, node_2] > MIN_MUSCLE_STRENGTH && !generatedFists.Contains(node_1) && !generatedFists.Contains(node_2) ){

                nodes[Utils.index2position(node_1, node_offset, layer_size, edge_node_size)].BecomeFist(fist_color);
                

                nodes[Utils.index2position(node_2, node_offset, layer_size, edge_node_size)].BecomeFist(fist_color);
                
                generatedFists.Add(node_1);
                generatedFists.Add(node_2);
            }
        }
    }

    public bool IsAlive(){
        return !dead;
    }


    public void Suicide(){
        SuicideNodes();
        SuicideMuscles();
        Kill();
    }

    public void Revive()
    {
        dead = false;
    }

    public void Spawn(bool reset){
        // Classic Instantiation
        RandomInit(reset);
        // ManageDisconnectionedBodies("Kill");
        float start = Time.realtimeSinceStartup;
        GenerateCreature(); // Expensive MOFO
        Debug.Log("[Time]GenerateCreature : " + (Time.realtimeSinceStartup - start).ToString("f6"));
        
        MoveToInitialPosition();
        AssignFists();
        Play();
    }


    public void Respawn(){
        Spawn(true);
        Revive();
    }

    public void Play(){
        foreach(KeyValuePair<Tuple<uint, uint>, Muscle> connection_muscle in joints){
            if(joints[connection_muscle.Key].IsAlive()){
                joints[connection_muscle.Key].node_1.GetComponent<Node>().Play();
                joints[connection_muscle.Key].node_2.GetComponent<Node>().Play();
                joints[connection_muscle.Key].Play();
            }
        }
    }

    public void PrintNodeSpecs(){
        foreach(KeyValuePair<Vector3, Node> pos_node in nodes){
            if(nodes[pos_node.Key].IsAlive()){
                nodes[pos_node.Key].Specs();
            }
        }
    }

    public void WakeUpNodes(){
        foreach(KeyValuePair<Vector3, Node> pos_node in nodes){
            if(!nodes[pos_node.Key].IsAlive()){
                nodes[pos_node.Key].Revive();
            }
        }
    }

    public void SuicideNodes(){
        foreach(KeyValuePair<Vector3, Node> pos_node in nodes){
            if(nodes[pos_node.Key].IsAlive()){
                nodes[pos_node.Key].Suicide();
            }
        }
    }

    public void WakeUpMuscles(){
        foreach(KeyValuePair<Tuple<uint, uint>, Muscle> connection_muscle in joints){
            if(!joints[connection_muscle.Key].IsAlive()){
                joints[connection_muscle.Key].Revive();
            }
        }
    }

    public void SuicideMuscles(){
        foreach(KeyValuePair<Tuple<uint, uint>, Muscle> connection_muscle in joints){
            if(joints[connection_muscle.Key].IsAlive()){
                joints[connection_muscle.Key].Suicide();

                // Reset connections
                node_connections[connection_muscle.Key.First, connection_muscle.Key.Second] = 0.0f;
            }
        }
    }

    private void Kill(){
        nodes_graph.Clear();
        dead = true;
    }

    private void Die(){
        uint aliveNodeCount = 0;
        
        // Update! Only active nodes
        foreach(uint node_idx in nodes_graph.Keys){
            aliveNodeCount += (uint)(nodes[Utils.index2position(node_idx, node_offset, layer_size, edge_node_size)].IsAlive() ? 1 : 0);
        }

        if(aliveNodeCount <= 0){
            this.Kill();
        }
    }

    private void UpdatePosition(){
        Vector3 total_position = new Vector3(0.0f, 0.0f, 0.0f);

        int alive_count = 0;

        foreach(KeyValuePair<Vector3, Node> pos_node in nodes){
            if(nodes[pos_node.Key].IsAlive()){
                total_position += nodes[pos_node.Key].gameObject.transform.position;
            }
        }
        if(alive_count > 0){
            pos = total_position / alive_count;
        }
    }

    private void MoveToInitialPosition(){
        foreach(KeyValuePair<Vector3, Node> pos_node in nodes){
            nodes[pos_node.Key].gameObject.transform.position += transform.position;
        }

        foreach(KeyValuePair<Tuple<uint, uint>, Muscle> connection_muscle in joints){
            joints[connection_muscle.Key].UpdatePosition();
        }
    }
    
    private void RandomInit(bool reset){
        if(reset){
            for(uint x = 0; x < total_node_count; ++x){
                for(uint y = 0; y < total_node_count; ++y){
                    node_connections[x, y] = 0.0f;
                }
            }
        }

        
        for(uint x = 0; x < total_node_count; ++x){
            int connection_count = max_connection_count_per_node;
            for(uint y = x; y < total_node_count; ++y){
        
                if(x == y){
                    node_connections[x, y] = 0.0f;
                }
                else{
                    float strength = Random.Range(-1.0f, 1.0f);
                    node_connections[x, y] = strength;
                    node_connections[y, x] = strength;
                    
                    if(strength > MIN_MUSCLE_STRENGTH){
                        
                        if(!nodes_graph.ContainsKey(x)){
                            nodes_graph[x] = new HashSet<uint>();
                        }
                        
                        if(!nodes_graph.ContainsKey(y)){
                            nodes_graph[y] = new HashSet<uint>();
                        }

                        nodes_graph[x].Add(y);
                        nodes_graph[y].Add(x);

                        connection_count--;
                    }
                }

                if(connection_count < 0){
                    break;
                }
            }
        }
    }


    public void GenerateCreature(){
        foreach(uint node_idx in nodes_graph.Keys){
            if(nodes_graph[node_idx].Count > 0){
                generateConnections(node_idx, Utils.index2position(node_idx, node_offset,layer_size, edge_node_size));
            }
        }
    }

    private void generateConnections(uint node_idx, Vector3 node_position){
        
        // If the node hasn't instantiated yet, instantiate a node object
        if (!nodes.ContainsKey(node_position)){
            string node_name = GetCreatureName() + "_" + GetCreatureType() +"_Node_" + node_idx;

            nodes[node_position] = Instantiate(node, node_position, Quaternion.identity) as Node;
            nodes[node_position].mass = NODE_MASS;
            nodes[node_position].init();
            nodes[node_position].name = node_name;
            nodes[node_position].transform.SetParent(this.transform);
        }
        else if(!nodes[node_position].IsAlive()){
            nodes[node_position].Revive();
            nodes[node_position].transform.position = node_position;
        }
        
        bool has_valid_connection = false;

        // for(uint node_neighbour = 0; node_neighbour < total_node_count; ++node_neighbour){
        foreach(uint node_neighbour in nodes_graph[node_idx]){
            // Don't look same nodes
            if(node_idx == node_neighbour){
                continue;
            }

            // Create real world coordiante
            Vector3 neighbour_pos = Utils.index2position(node_neighbour, node_offset,layer_size, edge_node_size);
            
            bool joint_exists = false;
            bool joint_dead = false;

            Tuple<uint, uint> connection_1 = new Tuple<uint, uint>(node_idx, node_neighbour);
            Tuple<uint, uint> connection_2 = new Tuple<uint, uint>(node_neighbour, node_idx);

            Tuple<uint, uint> current_muscle_connection = connection_1;
            if(joints.ContainsKey(connection_1)){
                if(joints[connection_1].IsAlive()){
                    joint_exists = true;
                }
                else{
                    joint_dead = true;
                }
            }
            else if(joints.ContainsKey(connection_2)){
                current_muscle_connection = connection_2;
                if(joints[connection_2].IsAlive()){
                    joint_exists = true;
                }
                else{
                    joint_dead = true;
                }
            }

            bool valid_connection = node_connections[node_idx, node_neighbour] > MIN_MUSCLE_STRENGTH;

            has_valid_connection |= valid_connection;

            // If the connection between nodes is valid and there is no joint between them
            if(valid_connection && !joint_exists) {

                // Node is not instantiated
                if (!nodes.ContainsKey(neighbour_pos)){
                    
                    // Instantiate Node
                    string node_name = GetCreatureName() + "_" + GetCreatureType() +"_Node_" + node_neighbour;
                    
                    nodes[neighbour_pos] = Instantiate(node, neighbour_pos, Quaternion.identity) as Node;
                    nodes[neighbour_pos].mass = NODE_MASS;
                    nodes[neighbour_pos].init();
                    nodes[neighbour_pos].name = node_name;
                    nodes[neighbour_pos].transform.SetParent(this.transform);
                }
                else if(!nodes[neighbour_pos].IsAlive()){
                    nodes[neighbour_pos].Revive();
                    nodes[neighbour_pos].transform.position = neighbour_pos;
                }

                // Instantiate Joint
                float strength = node_connections[node_idx, node_neighbour];
                if(!joint_dead){
                    string joint_name = GetCreatureName() + "_" + GetCreatureType() +"_Joint_" + node_idx + "_" + node_neighbour;
                    joints[current_muscle_connection] = Instantiate(joint, (node_position + neighbour_pos) / 2, Quaternion.identity) as Muscle;

                    joints[current_muscle_connection].node_1 = nodes[node_position].gameObject;
                    joints[current_muscle_connection].node_2 = nodes[neighbour_pos].gameObject;
                    joints[current_muscle_connection].strength = node_connections[node_idx, node_neighbour];
                    joints[current_muscle_connection].name = joint_name;

                    joints[current_muscle_connection].gameObject.transform.SetParent(this.transform);

                    // Attach a spring joint in between nodes
                    nodes[node_position].attachJoint(joints[current_muscle_connection].GetComponent<Rigidbody>(), 6.0f);
                    nodes[neighbour_pos].attachJoint(joints[current_muscle_connection].GetComponent<Rigidbody>(), 6.0f);
                    
                    if(GetCreatureType() == "X"){
                        joints[current_muscle_connection].lineColor = new Color(1.0f - strength, 0.0f, 0.0f);
                    }
                    else{
                        joints[current_muscle_connection].lineColor = new Color(0.0f, 1.0f - strength, 0.0f);
                    }

                    joints[current_muscle_connection].Init();
                }
                else{
                    joints[current_muscle_connection].UpdatePosition();
                    joints[current_muscle_connection].Revive();

                    nodes[node_position].attachJoint(joints[current_muscle_connection].GetComponent<Rigidbody>(), 6.0f);
                    nodes[neighbour_pos].attachJoint(joints[current_muscle_connection].GetComponent<Rigidbody>(), 6.0f);
                    
                    joints[current_muscle_connection].UpdateStrength(strength, GetCreatureType());
                }

                // [Optimization] Remove the current node from neighbour node's connections
                nodes_graph[node_neighbour].Remove(node_idx);
            }
        }

        // Clear Connections of that node
        nodes_graph[node_idx].Clear();

        // If it is alone node
        if(!has_valid_connection){
            nodes[node_position].Suicide();
        }

    }

    private HashSet<uint> getSubGraph(uint start_index){
        HashSet<uint> sub_graph = new HashSet<uint>();
        Queue<uint> node_queue = new Queue<uint>();
        
        node_queue.Enqueue(start_index);

        while(node_queue.Count > 0){
            uint current_node = node_queue.Dequeue();
            sub_graph.Add(current_node);

            for(uint neighbour_node = 0; neighbour_node < total_node_count; ++neighbour_node){
                if((neighbour_node != current_node) && (node_connections[current_node, neighbour_node] > MIN_MUSCLE_STRENGTH) && !sub_graph.Contains(neighbour_node)){
                    node_queue.Enqueue(neighbour_node);
                }
            }
        }

        return sub_graph;
    }

    private List<HashSet<uint>> FindAllSubGraphs(){
        List<HashSet<uint>> sub_graphs = new List<HashSet<uint>>();
        HashSet<uint> total_graph = new HashSet<uint>();

        while(total_graph.Count < total_node_count){
            for(uint node_id = 0; node_id < total_node_count; ++node_id){
                if(!total_graph.Contains(node_id)){
                    HashSet<uint> new_sub_graph = getSubGraph(node_id);
                    sub_graphs.Add(new_sub_graph);
                    
                    total_graph.UnionWith(new_sub_graph);
                }
            }
        } 
        return sub_graphs;
    }

    private void ManageDisconnectionedBodies(string strategy="Kill"){
        if(strategy == "Kill"){
            List<HashSet<uint>> sub_graphs = FindAllSubGraphs();
            int max_sub_graph_index = 0;
            int max_size = sub_graphs[max_sub_graph_index].Count;

            // Find the largest sugb graph
            for(int idx = 1; idx < sub_graphs.Count; ++idx){
                if(sub_graphs[idx].Count > max_size){
                    max_size = sub_graphs[idx].Count;
                    max_sub_graph_index = idx;
                }    
            }

            // Remove all other sub_graphs
            for(int idx = 0; idx < sub_graphs.Count; ++idx){
                if(idx != max_sub_graph_index){
                    foreach(uint node_id in sub_graphs[idx]){
                        // Too Slow! It checks all connections! 
                        for(uint neighbour_node_id = 0; neighbour_node_id < total_node_count; ++neighbour_node_id){
                            node_connections[node_id, neighbour_node_id] = 0.0f;
                        }
                    }
                }
            }
        }
        else{
            Debug.Log("WrongStrategy");
        }
    }
}
