﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class FlameParticles : MonoBehaviour
{
    public float damage = 5f;
    private ParticleSystem ps;

    private List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();
    private List<ParticleSystem.Particle> exit = new List<ParticleSystem.Particle>();

    void OnEnable()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void OnParticleTrigger()
    {
		int enterPs = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
    	if(enterPs > 0){
    		Health playerHealth = GameObject.FindWithTag("Player").GetComponent<Health>();
            playerHealth.DamageOvertime(damage);
            ps.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, new List<ParticleSystem.Particle>());
    	}
    }
}