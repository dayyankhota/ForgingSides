using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
        public static bool GameIsPaused = false; 
    public GameObject pauseMenuUI; 



      public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked; 
    }



    public void QuitGame()
    {
        Application.Quit();
    }

    
     
    
      
      
    
}  
   
        

