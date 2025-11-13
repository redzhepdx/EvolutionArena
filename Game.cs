using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public Creature creature_x;
    public Creature creature_y;

    public Transform camera_1;
    public Transform camera_2;
    public Text text;

    public Vector3 create_x_pos;
    public Vector3 create_y_pos;

    private int creature_x_score = 0;
    private int creature_y_score = 0;

    private Creature current_x;
    private Creature current_y;
    private AudioSource musicSource;
    
    void Awake(){
        current_x = Instantiate(creature_x, create_x_pos, Quaternion.identity) as Creature;
        current_x.SetType("X");
        current_y = Instantiate(creature_y, create_y_pos, Quaternion.identity) as Creature;
        current_y.SetType("Y");
        UpdateBoard();
        PlayBackground();
    }

    void Update()
    {
        if(!(current_x.IsAlive() && current_y.IsAlive())){
            UpdateScoreBoard();
        }
        else{
            FollowCreatures();
        }

        // Super User Control
        SuperUserRestart();
    }

    void SuperUserRestart(){
        if(Input.GetKeyDown(KeyCode.R)){
            Respawn();
        }
    }

    void FollowCreatures(){
        camera_1.LookAt(current_x.GetPos());
        camera_2.LookAt(current_y.GetPos());
    }

    void UpdateScoreBoard(){
        if(!current_x.IsAlive() && current_y.IsAlive()){
            creature_y_score++;
            Respawn();
        }
        else if(current_x.IsAlive() && !current_y.IsAlive()){
            creature_x_score++;
            Respawn();
            
        }
        UpdateBoard();
    }

    void Respawn(){
        Destroy(current_x.gameObject);
        Destroy(current_y.gameObject);
        current_x = Instantiate(creature_x, create_x_pos, Quaternion.identity) as Creature;
        current_x.SetType("X");
        current_y = Instantiate(creature_y, create_y_pos, Quaternion.identity) as Creature;
        current_y.SetType("Y");
    }

    void UpdateBoard(){
        text.text = "Evolution Arena\nC_TYSON  " + creature_x_score + " : " + creature_y_score + "  C_ALI";
    }


    void PlayBackground()
    {
        musicSource = GetComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.Play(0);      
    }
}
