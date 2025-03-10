﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable] public class BoulderMovement {
	public float speed = 5f;
	public float rotSpeed = 100f;	
	public Vector3 offset;
	public bool onGround;
}

public class BoulderTrap : ActionObject
{
	public float startDelay = 0;
	public float speed = 5f;
	public float rotSpeed = 100f;
	public BoulderMovement [] movements;
    public float bumpHeight = 1f;
    public float bumpAscendingSpeedMult = 5f;
    public float bumpDescendingSpeedMult = 1f;
    public bool loop = false;
    public bool trigger = true;
	
    private bool active = false;

    private Coroutine bump = null;
    private bool ascending = true;
    private float height = 0;
    private GameObject boulder;
    private ParticleSystem dust;
    private int currMovement = 0;
    private bool move = false;
    private Vector3 initPos;

    // Audio
    private AudioSource rollingAudio;
    private AudioSource impactAudio;

    // Start is called before the first frame update
    void Start()
    {
        boulder = transform.GetChild(0).gameObject;
        dust = transform.GetChild(1).gameObject.GetComponent<ParticleSystem>();
        initPos = transform.position;

        AudioSource[] audios = GetComponents<AudioSource>();
        rollingAudio = audios[0];
        impactAudio = audios[1];
        //impactAudio.Play()

        if(!trigger)
        	StartCoroutine(Init());
    }

    IEnumerator Init(){
    	yield return new WaitForSeconds(startDelay);
    	Action();
    }

    void Update(){
    	if(!move)
    		return;

    	if(currMovement >= movements.Length){
        	if(!loop){
        		SetActive(false);
        		dust.Stop();
                rollingAudio.Stop();
        		PlaySoundInterval(impactAudio,0,0.5f);
                move = false;
        		return;
        	}
        	else{
        		transform.position = initPos;
        		currMovement = 0;
        	}
        }

        Move();
    }

    public override void Action(){
  		SetActive(true);
    	move = true;
  	}

  	public void Move(){
  		move = false;
  		BoulderMovement movement = movements[currMovement];
    	Vector3 endPos = transform.position+movement.offset;
        
        Coroutine rotation = null;
		rotation = StartCoroutine(MovementUtils.SmoothRotationConst(boulder, Vector3.back, movement.rotSpeed));

    	StartCoroutine(MovementUtils.SmoothMovement( (bool done) => {
    		if(rotation != null)
            	StopCoroutine(rotation);
        	if(!movement.onGround)
        		PlaySoundInterval(impactAudio,0,0.5f);
            currMovement++;
            move = true;
        }, gameObject, endPos, movement.speed));

        //Play or dust ps
		if(movement.onGround){
			rollingAudio.Play();
    		dust.Play();
		}
    	else{
    		rollingAudio.Stop();
    		dust.Stop();
    	}
  	}

    public override void ExitAction(){}

    IEnumerator BumpRoutine() {
        while(true){
            Vector3 offset;
            if(ascending)
                offset = Vector3.up*Time.deltaTime*bumpAscendingSpeedMult;
            else
                offset = Vector3.down*Time.deltaTime*bumpDescendingSpeedMult;

            height += offset.y;
            transform.position += offset;
            if(height >= bumpHeight)
                ascending = false;
            else if(height <= 0){
            	if(movements[currMovement].onGround)
            		dust.Play();
                yield break;
            }

            yield return null;
        }
    }

    public void Bump(){
    	if(bump == null){
    		dust.Stop();
            bump = StartCoroutine(BumpRoutine());
    	}
    }	

    private void SetActive(bool state){
        active = state;
        boulder.GetComponent<Boulder>().SetColliderTrigger(state);
    }

    void PlaySoundInterval(AudioSource audioSource, float fromSeconds, float toSeconds)
    {
        audioSource.time = fromSeconds;
        audioSource.Play();
        audioSource.SetScheduledEndTime(AudioSettings.dspTime+(toSeconds-fromSeconds));
    }
}
