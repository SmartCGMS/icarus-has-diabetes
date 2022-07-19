using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using GameDummyTest;
using System.IO;
using System;
using UnityEngine.SceneManagement;

namespace gpredict3_gaming.Ikaros
{
    /// <summary>
    /// Class which permit to choose the type of playback
    /// </summary>
    public class PlaybackManager : MonoBehaviour
    {
        ToggleGroup toggleGroup;

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        void Start()
        {
            toggleGroup = GetComponentInChildren<ToggleGroup>();
        }

        /// <summary>
        /// The reaction on the "play" button click
        /// The playback of the selected players will be started.
        /// </summary>
        public void onPlayClick()
        {
            Toggle toggle = toggleGroup.ActiveToggles().FirstOrDefault();
            Debug.Log("The choice " + toggle.GetComponentInChildren<Text>().text + " will be replay");

            PlayerPrefs.SetInt("type_game", (int)TypeGame.PLAYBACK);

            if (toggle.name.Equals(GameParameters.OWN_GAME))
            {
                PlayerPrefs.SetString("ReplayLogs", PlayerPrefs.GetString("UserGameLog"));
                PlayerPrefs.SetInt("TypeReplay", (int)TypeReplay.PLAYER);
                SceneManager.LoadScene(GameParameters.PLAYBACK_SCENE);
            }
            else if (toggle.name.Equals(GameParameters.OPTIMIZED_GAME))
            {
                PlayerPrefs.SetInt("TypeReplay", (int)TypeReplay.AI);
                SceneManager.LoadScene(GameParameters.LOADING_SCENE);
            }
            else if (toggle.name.Equals(GameParameters.BOTH_GAME))
            {
                PlayerPrefs.SetString("ReplayLogs", PlayerPrefs.GetString("UserGameLog"));
                PlayerPrefs.SetInt("TypeReplay", (int)TypeReplay.BOTH);
                SceneManager.LoadScene(GameParameters.LOADING_SCENE);
            }
        }
    }

}