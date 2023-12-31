﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;


namespace gpredict3_gaming.Ikaros
{
    /// <summary>
    /// Class for managing of time and steps of game
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        //the initial time for game (in seconds)
        private float StartTime;

        //the actual time of the game (in seconds) 
        public float GameTime { get; private set; }


        //the duration of the whole game (in seconds)
        public float MaxGameTime { get; private set; }

        //simulation interval, i.e. interval between ticks to SmartCGMS backend
        public static readonly float SimulationTickIntervalSecs = 0.04f; //equal 1 minute (timeStep) in SmartCGMS backend;
        //private float SimulationTickIntervalSecs = 0.2f; //old stepping

        //interpolation interval, i.e. player move, meal move, etc.
        public static readonly float InterpolationTickIntervalSecs = 0.01f; //four times per one simulation tick
        //private float InterpolationTickIntervalSecs = 0.05f; //old stepping
        private float TimeLastInterpolationTick = 0.0f;

        //interval for heatlhy efect
        public static readonly float HealthyTickIntervalSecs = 0.4f;
        private float TimeLastHealthyTick = 0.0f;

        private Text TimeText;

        private PlayerCharacter[] PlayersArr; 

        

        public bool IsTerminated { get; set; }


        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            //Set time, when the data are taken from model of patient
            Time.fixedDeltaTime = SimulationTickIntervalSecs;
        }


        /// <summary>
        /// Start is called before the first frame update.
        /// </summary>
        void Start()
        {
            TimeText = GetComponent<Text>();
            GameTime = 0.0f;
            StartTime = Time.time;
            MaxGameTime = PlayerPrefs.GetFloat("maxGameTime");
            PlayersArr = FindObjectsOfType<PlayerCharacter>();
            IsTerminated = false;

        }


        

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        void Update()
        {
            //Compute game time and time to end of game
            GameTime = Time.time - StartTime;
            float remainTime = MaxGameTime - GameTime;

            //if game is terminated, the game over scene is called
            if (IsTerminated)
            {
                SceneManager.LoadScene(GameParameters.GAME_OVER_SCENE);
                return;
            }

            //if time to end of game is less of equal to zero, the game over scene is called
            //if the patient is dead, the game over scene is called, too
            if (remainTime <= 0 || isSomePatientDead())
            {
                TerminateGame();
                SceneManager.LoadScene(GameParameters.GAME_OVER_SCENE);
                return;
            }

            //calling the OnInterpolationTick event for each player
            if ((TimeLastInterpolationTick + InterpolationTickIntervalSecs) <= GameTime)
            {
                foreach(var player in PlayersArr)
                {
                    player.OnInterpolationTick(GameTime - TimeLastInterpolationTick);
                }
                TimeLastInterpolationTick = GameTime;
            }

            //calling the OnHealthyTick event for each player
            if ((TimeLastHealthyTick + HealthyTickIntervalSecs) <= GameTime)
            {
                foreach (var player in PlayersArr)
                {
                    player.OnHealthyTick();
                }
                
                TimeLastHealthyTick = GameTime;
            }


            //string for the output of the remain game time
            string remMinutes = ((int)(remainTime / 60)).ToString();
            string remSeconds = ((int)remainTime % 60).ToString("D2");
            TimeText.text = remMinutes + ":" + remSeconds;
        }

        //Fixed Update is called once per fix delta
        private void FixedUpdate()
        {
            if (IsTerminated)
            {
                return;
            }

            //calling the OnSimulationTick event for each player
            foreach (var player in PlayersArr)
            {
                player.OnSimulationTick(Time.time - StartTime);
            }
        }

        /// <summary>
        /// Reaction when the game is terminated
        /// </summary>
        public void TerminateGame()
        {
            foreach (var player in PlayersArr)
            {
                player.Game.Terminate();
            }
            Directory.SetCurrentDirectory(GameParameters.GAME_WORKING_DIRECTORY);
            IsTerminated = true;
        }


        /// <summary>
        /// Test if there is some player who is dead
        /// </summary>
        /// <returns>true if any player is dead, false otherwise</returns>
        private bool isSomePatientDead()
        {
            foreach (var player in PlayersArr)
            {
                if (player.IsPatientDead) return true;
            }
            return false;
        }


        /// <summary>
        /// Determine the current game time
        /// </summary>
        /// <returns>current game time</returns>
        public float GetActualTime()
        {
            return Time.time - StartTime;
        }

    }

}
