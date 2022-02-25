using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace gpredict3_gaming.Ikaros
{
    public class MenuButtonEvents : MonoBehaviour
    {
        public void NewGame()
        {
            PlayerPrefs.SetFloat("maxGameTime", GameParameters.GAME_TIME);
            PlayerPrefs.SetInt("type_game", GameParameters.FULL_GAME);
            SceneManager.LoadScene(GameParameters.PROFILE_SCENE);
            
        }

        public void TestGame()
        {
            PlayerPrefs.SetFloat("maxGameTime", GameParameters.DEMO_GAME_TIME);
            PlayerPrefs.SetInt("type_game", GameParameters.DEMO_GAME);
            PlayerPrefs.SetFloat("difficulty", GameParameters.DIFFICULTY_EASY);
            SceneManager.LoadScene(GameParameters.GAME_SCENE);
            
        }

        public void ShowControls()
        {
            SceneManager.LoadScene(GameParameters.CONTROLS_SCENE);
        }

        public void ShowScoreboard()
        {
            SceneManager.LoadScene(GameParameters.SCOREBOARD_SCENE);
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        /// <summary>
        /// Method for return to main menu
        /// </summary>
        public void BackToMenu()
        {
            SceneManager.LoadScene(GameParameters.MAIN_MENU_SCENE);
        }



    }
}
