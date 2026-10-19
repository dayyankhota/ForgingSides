using UnityEngine;

public class AudioManager : MonoBehaviour
{
        [Header ("--- Audio Source---")]
    [SerializeField] AudioSource musicSource;
      [SerializeField] AudioSource SFXSource;

      [Header ("--- Audio Clip---")]
      public AudioClip steamEffect;
      public AudioClip metalClang;
      public AudioClip waterDrop;
   

      private void Start()
      {

      }

      public void PlaySFX(AudioClip clip)
      {
          SFXSource.PlayOneShot(clip);
      }

}
