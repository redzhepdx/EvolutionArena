using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Generator : MonoBehaviour
{   
    public Node node; // Node object
    public Muscle joint; // Joint Object
    
    public float MIN_MUSCLE_STRENGTH = 0.1f;
    public float NODE_MASS = 1.0f;
    public float node_offset = 3.0f;
    public uint edge_node_size = 4;
    public int max_connection_count_per_node = 2; 
    private uint layer_size;
    private uint total_node_count;
    private float[,] node_connections;
    private Dictionary<Vector3, Node> nodes;
    private Dictionary<Tuple<uint, uint>, Muscle> joints;

    private void RandomInit(){
        
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
        for(uint z = 0; z < edge_node_size; ++z){
            for(uint y = 0; y < edge_node_size; ++y){
                for(uint x = 0; x < edge_node_size; ++x){
                    Vector3 node_position = new Vector3(x * node_offset, y * node_offset, z * node_offset);
                    // Real position to vector position 
                    uint node_idx = z * layer_size + y * edge_node_size + x;
                    // Generate Connections
                    generateConnections(node_idx, node_position); 
                }
            }
        }
    }

    private void generateConnections(uint node_idx, Vector3 node_position){
        
        // If the node hasn't instantiated yet, instantiate a node object
        if (!nodes.ContainsKey(node_position)){
            nodes[node_position] = Instantiate(node, node_position, Quaternion.identity) as Node;
            nodes[node_position].mass = NODE_MASS;
            nodes[node_position].init();
            nodes[node_position].name = "Node_" + node_idx;
            nodes[node_position].transform.parent = this.transform;
        }
        
        bool has_valid_connection = false;

        for(uint node_neighbour = 0; node_neighbour < total_node_count; ++node_neighbour){
            // Don't look same nodes
            if(node_idx == node_neighbour){
                continue;
            }

            // Get real position of neighbour
            uint z = (uint)Mathf.Floor(node_neighbour / layer_size);
            uint y = (uint)Mathf.Floor((node_neighbour % layer_size) / edge_node_size);
            uint x = node_neighbour - (z * layer_size + y * edge_node_size);

            // Create real world coordiante
            Vector3 neighbour_pos = new Vector3((float)x * node_offset, (float)y * node_offset, (float)z * node_offset);
            
            bool joint_exists = joints.ContainsKey(new Tuple<uint, uint>(node_idx, node_neighbour)) || 
                                joints.ContainsKey(new Tuple<uint, uint>(node_neighbour, node_idx));

            bool valid_connection = node_connections[node_idx, node_neighbour] > MIN_MUSCLE_STRENGTH;

            has_valid_connection |= valid_connection;

            // If the connection between nodes is valid and there is no joint between them
            if(valid_connection && !joint_exists) {

                // Node is instantiated
                if (!nodes.ContainsKey(neighbour_pos)){
                    // Instantiate Node
                    nodes[neighbour_pos] = Instantiate(node, neighbour_pos, Quaternion.identity) as Node;
                    nodes[neighbour_pos].mass = NODE_MASS;
                    nodes[neighbour_pos].init();
                    nodes[neighbour_pos].name = "Node_" + node_neighbour;
                    nodes[neighbour_pos].transform.parent = this.transform;
                }

                // Instantiate Joint
                Muscle new_joint = Instantiate(joint, (node_position + neighbour_pos) / 2, Quaternion.identity) as Muscle;
                new_joint.node_1 = nodes[node_position].gameObject;
                new_joint.node_2 = nodes[neighbour_pos].gameObject;
                new_joint.strength = node_connections[node_idx, node_neighbour];
                new_joint.name = "Joint_" + node_idx + "_" + node_neighbour;

                new_joint.gameObject.transform.parent = this.transform;
                
                
                // Attach a spring joint in between nodes
                nodes[node_position].attachJoint(new_joint.GetComponent<Rigidbody>(), 6.0f);
                nodes[neighbour_pos].attachJoint(new_joint.GetComponent<Rigidbody>(), 6.0f);

                joints[new Tuple<uint, uint>(node_idx, node_neighbour)] = new_joint;
            }
        }

        // If it is alone node
        if(!has_valid_connection){
            nodes[node_position].gameObject.SetActive(false);
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
    

    void Awake(){
        this.layer_size = this.edge_node_size * this.edge_node_size;
        this.total_node_count = this.edge_node_size * this.layer_size;

	    node_connections = new float[total_node_count, total_node_count];
        nodes = new Dictionary<Vector3, Node>();
        joints = new Dictionary<Tuple<uint, uint>, Muscle>();
    }

    void Start()
    {           
        RandomInit();
        ManageDisconnectionedBodies("Kill");
        GenerateCreature();
    }

    void Update()
    {
        
    }
}
