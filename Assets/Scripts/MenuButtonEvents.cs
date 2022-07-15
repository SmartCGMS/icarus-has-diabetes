using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace gpredict3_gaming.Ikaros
{
    public class MenuButtonEvents : MonoBehaviour
    {
        /// <summary>
        /// Reaction on the "Play" button click.
        /// The full game will be started when the form with information about the player is filled.
        /// </summary>
        public void NewGame()
        {
            PlayerPrefs.SetFloat("maxGameTime", GameParameters.GAME_TIME);
            PlayerPrefs.SetInt("type_game", (int)TypeGame.FULL_GAME);
            SceneManager.LoadScene(GameParameters.PROFILE_SCENE);
            
        }

        /// <summary>
        /// Reaction on the "Training" button click.
        /// The demo game will be started.
        /// </summary>
        public void TestGame()
        {
            PlayerPrefs.SetFloat("maxGameTime", GameParameters.DEMO_GAME_TIME);
            PlayerPrefs.SetInt("type_game", (int)TypeGame.DEMO_GAME);
            PlayerPrefs.SetFloat("difficulty", GameParameters.DIFFICULTY_EASY);
            SceneManager.LoadScene(GameParameters.GAME_SCENE);
            
        }

        /// <summary>
        /// Reaction on the "Controls" button click.
        /// The shortcuts for the playing game are shown.
        /// </summary>
        public void ShowControls()
        {
            SceneManager.LoadScene(GameParameters.CONTROLS_SCENE);
        }

        /// <summary>
        /// Reaction on the "Hall of fame" button click.
        /// The scoreboard is shown.
        /// </summary>
        public void ShowScoreboard()
        {
            SceneManager.LoadScene(GameParameters.SCOREBOARD_SCENE);
        }

        /// <summary>
        /// Method for exit the game.
        /// </summary>
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
