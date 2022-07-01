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
    public class PlaybackManager : MonoBehaviour
    {
        ToggleGroup toggleGroup;

        // Start is called before the first frame update
        void Start()
        {
            toggleGroup = GetComponentInChildren<ToggleGroup>();
        }

        public void onPlayClick()
        {
            Toggle toggle = toggleGroup.ActiveToggles().FirstOrDefault();
            Debug.Log("The choice " + toggle.GetComponentInChildren<Text>().text + " will be replay");

            PlayerPrefs.SetInt("type_game", GameParameters.PLAYBACK);

            if (toggle.name.Equals(GameParameters.OWN_GAME))
            {
                PlayerPrefs.SetString("ReplayLogs", PlayerPrefs.GetString("UserGameLog"));
                SceneManager.LoadScene(GameParameters.PLAYBACK_SCENE);
            }
            else if (toggle.name.Equals(GameParameters.OPTIMIZED_GAME))
            {
                SceneManager.LoadScene(GameParameters.LOADING_SCENE);
            }
            /*TODO - in the future when the options for BOTH_GAME will be available
            else if (toggle.name.Equals(GameParameters.BOTH_GAME))
            {
                PlayerPrefs.SetString("ReplayLogs", PlayerPrefs.GetString("UserGameLog"));
                SceneManager.LoadScene(GameParameters.LOADING_SCENE);
            }*/
        }
    }

}