using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Arena : MonoBehaviour
{
    public Creature creature_x;
    public Creature creature_y;

    public Transform camera_1;
    public Transform camera_2;
    public Text text;

    public Vector3 create_x_pos;
    public Vector3 create_y_pos;

    public float countDownLimit = 10.0f;

    private int creature_x_score = 0;
    private int creature_y_score = 0;

    private Creature current_x;
    private Creature current_y;
    private AudioSource musicSource;

    private string arena_name;

    private float arenaTimer;

    private bool alive = false;
    
    void Start(){
        // Spawn();
        
        // UpdateBoard();
        // PlayBackground();
    }

    void Update()
    {
        if(IsAlive()){
            if(!(current_x.IsAlive() && current_y.IsAlive())){
                UpdateScoreBoard();
            }
            else{
                // FollowCreatures();

                CountDownCheck();
            }

            // Super User Control
            SuperUserRestart();

            //Random Creature Update
            IntermediateRandomUpdate();
        }
    }

    public void SetArenaName(string name){
        arena_name = name;
    }

    public string GetArenaName(){
        return arena_name;
    }

    public bool IsAlive(){
        return alive;
    }

    private void SuperUserRestart(){
        if(Input.GetKeyDown(KeyCode.R)){
            Respawn();
        }
    }

    private void IntermediateRandomUpdate(){
        if(Input.GetKeyDown(KeyCode.X)){
            creature_x.UpdateMusclesRandomly();
        }

        if(Input.GetKeyDown(KeyCode.Y)){
            creature_y.UpdateMusclesRandomly();
        }
    }

    private void FollowCreatures(){
        camera_1.LookAt(current_x.GetPos());
        camera_2.LookAt(current_y.GetPos());
    }

    private void CountDownCheck(){
        arenaTimer += Time.deltaTime;

        if(arenaTimer >= countDownLimit){
            // Restart
            Respawn();

            // Reset Timer
            arenaTimer = 0.0f;
        }

    }

    private void UpdateScoreBoard(){
        if(!current_x.IsAlive() && current_y.IsAlive()){
            creature_y_score++;
            Respawn();
        }
        else if(current_x.IsAlive() && !current_y.IsAlive()){
            creature_x_score++;
            Respawn();
            
        }
        else{
            Respawn();
        }
        // UpdateBoard();
    }

    public void Spawn(){
        current_x = Instantiate(creature_x, create_x_pos, Quaternion.identity) as Creature;
        current_x.SetCreatureType("X");
        current_x.SetCreatureName(GetArenaName() + "_C_");
        current_x.name = current_x.GetCreatureName() + "_" + current_x.GetCreatureType();
        current_x.Init();
        current_x.Spawn(false);


        current_y = Instantiate(creature_y, create_y_pos, Quaternion.identity) as Creature;
        current_y.SetCreatureType("Y");
        current_y.SetCreatureName(GetArenaName() + "_C_");
        current_y.name = current_y.GetCreatureName() + "_" + current_y.GetCreatureType();
        current_y.Init();
        current_y.Spawn(false);

        current_x.transform.parent = this.transform;
        current_y.transform.parent = this.transform;

        arenaTimer = 0.0f;
        countDownLimit = 10.0f;
        alive = true;
    }

    private void Respawn(){
        if(current_x.IsAlive()){
            current_x.Suicide();
        }

        if(current_y.IsAlive()){
            current_y.Suicide();
        }

        current_x.transform.position = create_x_pos;
        current_x.Respawn();
        current_y.transform.position = create_y_pos;
        current_y.Respawn();
    }

    private void UpdateBoard(){
        text.text = "Evolution Arena\nC_TYSON  " + creature_x_score + " : " + creature_y_score + "  C_ALI";
    }


    private void PlayBackground()
    {
        musicSource = GetComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.Play(0);      
    }
}
