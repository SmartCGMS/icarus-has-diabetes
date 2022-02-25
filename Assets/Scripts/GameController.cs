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


        public UInt16 configClass = 1; //TODO - dependency on difficulty
        public UInt16 configId = 1;
        public UInt32 timeStep = 60000; //step for backend in [ms]
        public String logFilePath = Path.Combine(Directory.GetParent(GameParameters.LOCAL_STORAGE_PATH).FullName, 
            GameParameters.LOGFILE_PREFIX + GameParameters.LOGFILE_EXT);



        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        void Start()
        {

            Player = FindObjectOfType<PlayerController>();
            TimeCtrl = FindObjectOfType<TimeManager>();
            Player.Game = new SCGMS_Game(configClass, configId, timeStep, logFilePath);
            Directory.SetCurrentDirectory(Directory.GetParent(GameParameters.LOCAL_STORAGE_PATH).FullName);

        }

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
