using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ParticleTimer : MonoBehaviour
{

    AudioManager audioManager;

    public ParticleSystem steamParticle = null;
    public ParticleSystem waterDropParticle = null;
   
    public int intervalInSeconds = 15;
     public int intervalInSecondsDrop = 10;
    private void Start()
    {
        
  audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
          
            StartCoroutine(ActivateParticlesLoop());
            StartCoroutine(ActivateParticlesLoopDrop());
    }    


    private IEnumerator ActivateParticlesLoop()
    {
       
        while (true)
        {
           
            steamParticle.Play();
            audioManager.PlaySFX(audioManager.steamEffect);
       
            yield return new WaitForSeconds(intervalInSeconds);
        }
    }
       private IEnumerator ActivateParticlesLoopDrop()
    {
       
        while (true)
        {
           
            waterDropParticle.Play();
            audioManager.PlaySFX(audioManager.waterDrop);
       
            yield return new WaitForSeconds(intervalInSecondsDrop);
        }
    }

}
