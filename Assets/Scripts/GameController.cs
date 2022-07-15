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

        //private PlayerCharacter Player;
        private TimeManager TimeCtrl;

        public static float VisualSpeedMultiplier = -2.0f;

        public UInt16 ConfigClass { get; private set; }

        public UInt16 ConfigId { get; private set; }
        public UInt32 TimeStep { get; private set; }

        private static readonly string[] PLAYERS_NAME = new string[] { "Ikaros", "AI" };

        //public String LogFilePath { get; private set; }



        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        //void Start()
        void Awake()
        {            
            if(PlayerPrefs.GetInt("type_game") != (int)TypeGame.PLAYBACK)
            {
                gameSetup(); 
            }
            else
            { 
                playbackSetup();
            }
            TimeCtrl = FindObjectOfType<TimeManager>();
            Directory.SetCurrentDirectory(Directory.GetParent(Application.persistentDataPath).FullName);

        }


        private void gameSetup()
        {
            Debug.Log("Full (" + (int)TypeGame.FULL_GAME + ") or Demo (" + (int)TypeGame.DEMO_GAME + ") game - " + PlayerPrefs.GetInt("type_game"));
            ConfigClass = 1; //TODO - dependency on difficulty
            ConfigId = 1;
            TimeStep = 60000;
            string logFilePath = Path.Combine(Directory.GetParent(Application.persistentDataPath).FullName,
            GameParameters.LOGFILE_PREFIX + GameParameters.LOGFILE_EXT);
            PlayerPrefs.SetInt("ConfigClass", ConfigClass);
            PlayerPrefs.SetInt("ConfigId", ConfigId);
            PlayerPrefs.SetInt("TimeStep", (int) TimeStep);
            PlayerPrefs.SetString("UserGameLog", logFilePath);

            if(MealDataStorage.MealList != null)
            {
                MealDataStorage.clearListOfMeal();
                //Debug.Log("The list of meal is clear.");
            }
            var player = FindObjectOfType<PlayerController>();
            player.Game = new SCGMS_Game(ConfigClass, ConfigId, TimeStep, logFilePath);
        }


        private void playbackSetup()
        {
            Debug.Log("Playback (" + (int)TypeGame.PLAYBACK + ") - " + PlayerPrefs.GetInt("type_game"));
            string logFilePath = PlayerPrefs.GetString("ReplayLogs");
            var typeReplay = PlayerPrefs.GetInt("TypeReplay");
            
            
            if(typeReplay != (int)TypeReplay.BOTH)
            {
                Debug.Log("Only one player in playback");
                var player = findPlayer(PLAYERS_NAME[typeReplay]);
                player.gameObject.SetActive(true);
                player.Game = new SCGMS_Game(logFilePath);
            }
            /*TODO - in the future when the options for BOTH_GAME will be available
            else
            {
                var playersArr = FindObjectsOfType<ReplayPlayerController>(true);
                var logsArr = logFilePath.Trim().Split("; ");
                foreach (var player in playersArr)
                {
                    player.gameObject.SetActive(true);
                    var index = (PLAYERS_NAME.Equals(PLAYERS_NAME[0])) ? 0 : 1;
                    player.Game = new SCGMS_Game(logsArr[index]);
                }
            }*/
            
        }

        private ReplayPlayerController findPlayer(string name)
        {
            var playersArr = FindObjectsOfType<ReplayPlayerController>(true);
            foreach (var player in playersArr)
            {
                if (player.name.Equals(name)) return player;
            }
            return null;
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
