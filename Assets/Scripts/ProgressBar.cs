using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameDummyTest;
using System;
using System.IO;
using UnityEngine.SceneManagement;

namespace gpredict3_gaming.Ikaros { 

    /// <summary>
    /// Class which obtain the log with optimal game strategy
    /// </summary>
    public class ProgressBar : MonoBehaviour
    {
        private Slider slider;
        private Text progressText;

        private double currentProgress;

        private SCGMS_Game_Opt opt;
        private SCGMS_Game_Opt.Optimizer_Status status;
        private String OptimalLogfilePath;



        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            slider = gameObject.GetComponent<Slider>();
            progressText = gameObject.GetComponentInChildren<Text>();
            OptimalLogfilePath = Path.Combine(GameParameters.DIRECTORY, GameParameters.OPT_LOGFILE + GameParameters.LOGFILE_EXT);
            opt = new SCGMS_Game_Opt((ushort) PlayerPrefs.GetInt("ConfigClass"),
                        (ushort) PlayerPrefs.GetInt("ConfigId"),
                        (uint) PlayerPrefs.GetInt("TimeStep"),
                        PlayerPrefs.GetString("UserGameLog"),
                        OptimalLogfilePath,
                        GameParameters.DEGREE_OF_OPT);
            //Debug.Log("Parameters: " + (ushort)PlayerPrefs.GetInt("ConfigClass") + ", " + (ushort)PlayerPrefs.GetInt("ConfigId") + ", " + (uint)PlayerPrefs.GetInt("TimeStep"));
        }


        // Update is called once per frame
        void Update()
        {
            try {
                if (Input.GetKeyDown(KeyCode.Escape)) //case when the player want to delete the chosen operation
                {
                    AsyncCancel handler = opt.Cancel_Optimalization;
                    bool res = handler(true);
                    Debug.Log("Canceled. ");
                    SceneManager.LoadScene(GameParameters.MAIN_MENU_SCENE);
                }

                
                status = opt.Get_Status(out currentProgress);
                //If the optimizer is in a running state, the value of the progress bar is updated
                if (status == SCGMS_Game_Opt.Optimizer_Status.Running)
                {
                    slider.value = (float) currentProgress;
                    var progressInPercent = (int) Math.Round(currentProgress * 100);
                    progressText.text = progressInPercent + " %";
                }
                else if(status == SCGMS_Game_Opt.Optimizer_Status.Success) //otherwise, the output is finalized and the playback scene is loaded
                {
                    opt.Finalize_Output();
                    String logfilesPath = OptimalLogfilePath;
                    if (PlayerPrefs.GetInt("TypeReplay")== (int) TypeReplay.BOTH)
                    {
                        var tmp = PlayerPrefs.GetString("ReplayLogs");
                        logfilesPath = tmp + "; " + OptimalLogfilePath;       
                    }
                    //Debug.Log(logfilesPath);
                    PlayerPrefs.SetString("ReplayLogs", logfilesPath);
                    
                    SceneManager.LoadScene(GameParameters.PLAYBACK_SCENE);
                }
                else if(status == SCGMS_Game_Opt.Optimizer_Status.Failed)
                {
                    SceneManager.LoadScene(GameParameters.MAIN_MENU_SCENE);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Optimizer error: " + ex.Message);
            }
        }


        private delegate bool AsyncCancel(bool wait_for_cancel);



    }
}
