using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlowUp : MonoBehaviour
{

    public float lifeTime = 1.0f;
    
    private float timeSpend;

    void Start(){
        timeSpend = 0.0f;
    }

    void Update()
    {
        Explode();
    }

    public void Explode(){
        timeSpend += Time.deltaTime;
        if(lifeTime <= timeSpend){
            Destroy(gameObject);
        }
    }

}
