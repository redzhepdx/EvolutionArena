using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColosseumManager : MonoBehaviour
{


    public Arena arenaPrefab;
    public Transform gameArenaField;
    public uint arenaGridEdge;
    public float arena_width;
    public float arena_offset;

    void Awake(){
        SpawnAreanas();
    }

    void SpawnAreanas(){
        for(uint arena_idx = 0; arena_idx < arenaGridEdge; ++arena_idx){
            for(uint arena_idy = 0; arena_idy < arenaGridEdge; ++arena_idy){
                Vector3 arenaPosition = new Vector3((float)arena_idx * (arena_width + arena_offset), 
                                                    -1.0f, 
                                                    (float)arena_idy * (arena_width + arena_offset));
                
                // Generate the Field
                Transform currentField = Instantiate(gameArenaField, arenaPosition, Quaternion.identity) as Transform;
                currentField.name = "Field_" + arena_idx + "_" + arena_idy;
                // currentField.parent = this.transform;

                // Generate Creatures
                Arena currentArena = Instantiate(arenaPrefab, arenaPosition, Quaternion.identity) as Arena;
                currentArena.SetArenaName("Arena_" + arena_idx + "_" + arena_idy);
                currentArena.create_x_pos = new Vector3(arenaPosition.x + (arena_width / 3), 
                                                        0, 
                                                        arenaPosition.z + (arena_width / 3));
                currentArena.create_y_pos = new Vector3(arenaPosition.x - (arena_width / 3), 
                                                        0, 
                                                        arenaPosition.z - (arena_width / 3));

                currentArena.Spawn();
                currentArena.transform.parent = this.transform;
            }
        }
    }
}
