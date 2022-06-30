using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEditor;
using System.IO;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using AOT;
using UnityEngine.SceneManagement;
using GameDummyTest;


namespace gpredict3_gaming.Ikaros
{
    public class GameController : MonoBehaviour
    {

        public PlayerController Player;
        public TimeManager TimeCtrl;

        public static float VisualSpeedMultiplier = -2.0f;

        public UInt16 ConfigClass { get; private set; }

        public UInt16 ConfigId { get; private set; }
        public UInt32 TimeStep { get; private set; }

        public String LogFilePath { get; private set; }



        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        void Start()
        {
            gameSetup();
            Player = FindObjectOfType<PlayerController>();
            TimeCtrl = FindObjectOfType<TimeManager>();
            Player.Game = new SCGMS_Game(ConfigClass, ConfigId, TimeStep, LogFilePath);
            Directory.SetCurrentDirectory(Directory.GetParent(Application.persistentDataPath).FullName);

        }


        private void gameSetup()
        {
            ConfigClass = 1; //TODO - dependency on difficulty
            ConfigId = 1;
            TimeStep = 60000;
            LogFilePath = Path.Combine(Directory.GetParent(Application.persistentDataPath).FullName,
            GameParameters.LOGFILE_PREFIX + GameParameters.LOGFILE_EXT);
            PlayerPrefs.SetInt("ConfigClass", ConfigClass);
            PlayerPrefs.SetInt("ConfigId", ConfigId);
            PlayerPrefs.SetInt("TimeStep", (int) TimeStep);
            PlayerPrefs.SetString("UserGameLog", LogFilePath);

        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TimeCtrl.TerminateGame();
                SceneManager.LoadScene(GameParameters.MAIN_MENU_SCENE);
            }
        }

        /// <summary>
        /// The event for quit of application
        /// </summary>
        public void OnApplicationQuit()
        {
            TimeCtrl.TerminateGame();
        }


       


        /*private String GetNativeLibraryPath()
        {
#if UNITY_EDITOR_32
            String path = Path.Combine(Application.dataPath, Path.Combine("Plugins", "x86"));
#elif UNITY_EDITOR_64
            String path = Path.Combine(Application.dataPath, Path.Combine("Plugins", "x86_64"));
#else
            String path = Path.Combine(Application.dataPath, "Plugins");
#endif
            return path;
        }*/

    }
}
