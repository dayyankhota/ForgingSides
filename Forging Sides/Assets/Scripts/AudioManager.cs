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
      public AudioClip Furnace;
      public AudioClip BoxInteract;
      public AudioClip CustomerSuccess;
      public AudioClip CustomerFail;

      private void Start()
      {

      }

      public void PlaySFX(AudioClip clip)
      {
          SFXSource.PlayOneShot(clip);
      }

}
