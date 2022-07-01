using System.Collections;
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
        private float GameTime;


        //the duration of the whole game (in seconds)
        private float MaxGameTime;

        //simulation interval, i.e. interval between ticks to SmartCGMS backend
        private float SimulationTickIntervalSecs = 0.04f; //equal 1 minute (timeStep) in SmartCGMS backend;
        //private float SimulationTickIntervalSecs = 0.2f; //old stepping

        //interpolation interval, i.e. player move, meal move, etc.
        private float InterpolationTickIntervalSecs = 0.01f; //four times per one simulation tick
        //private float InterpolationTickIntervalSecs = 0.05f; //old stepping
        private float TimeLastInterpolationTick = 0.0f;

        //interval for heatlhy efect
        private float HealthyTickIntervalSecs = 0.4f;
        private float TimeLastHealthyTick = 0.0f;

        private Text TimeText; //!!! TODO Playback - mozna nebude cas zobrazovan???

        //public string GameOverScene;
        public PlayerController Player;

        

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
            Player = FindObjectOfType<PlayerController>();
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
            if (remainTime <= 0 || Player.IsPatientDead) //!!! TODO Playback - tady mozna bude nutna uprava druhe podminky???
            {
                TerminateGame();
                SceneManager.LoadScene(GameParameters.GAME_OVER_SCENE);
                return;
            }

            //calling the OnInterpolationTick event for player
            if ((TimeLastInterpolationTick + InterpolationTickIntervalSecs) <= GameTime)
            {
                Player.OnInterpolationTick(GameTime-TimeLastInterpolationTick);
                TimeLastInterpolationTick = GameTime;
            }

            //calling the OnHealthyTick event for player
            if ((TimeLastHealthyTick + HealthyTickIntervalSecs) <= GameTime)
            {
                Player.OnHealthyTick();
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

            //calling the OnSimulationTick event for player
            Player.OnSimulationTick(Time.time - StartTime);
        }


        public void TerminateGame()
        {
            //NativeBridge.Filter_GameInsulinTick(-1, -1, -1);
            //NativeBridge.TerminateFilterChain();
            Player.Game.Terminate();
            Directory.SetCurrentDirectory(GameParameters.GAME_WORKING_DIRECTORY);
            IsTerminated = true;
        }



        /// <summary>
        /// Getter for tick of simulation step in seconds
        /// </summary>
        /// <returns>tick of simulation step</returns>
        public float GetSimulationTickIntervalSecs()
        {
            return SimulationTickIntervalSecs;
        }

        /// <summary>
        /// Getter for tick of interpolation step in seconds
        /// </summary>
        /// <returns>tick of interpolation step</returns>
        public float GetInterpolationTickIntervalSecs()
        {
            return InterpolationTickIntervalSecs;
        }

        /// <summary>
        /// Getter for game time in seconds
        /// </summary>
        /// <returns>game time</returns>
        public float GetGameTime()
        {
            return GameTime;
        }


        public float GetActualTime()
        {
            return Time.time - StartTime;
        }

        public float GetMaxGameTime()
        {
            return MaxGameTime;
        }
    }
}
