using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameDummyTest;
using System;
using System.IO;
using UnityEngine.SceneManagement;

namespace gpredict3_gaming.Ikaros { 

    public class ProgressBar : MonoBehaviour
    {
        private Slider slider;
        private Text progressText;

        private double currentProgress;

        private SCGMS_Game_Opt opt;
        private SCGMS_Game_Opt.Optimizer_Status status;
        private String OptimalLogfilePath;


        

        private void Awake()
        {
            slider = gameObject.GetComponent<Slider>();
            progressText = gameObject.GetComponentInChildren<Text>();
            OptimalLogfilePath = Path.Combine(Directory.GetParent(Application.persistentDataPath).FullName,
                GameParameters.OPT_LOGFILE + GameParameters.LOGFILE_EXT);
            opt = new SCGMS_Game_Opt((ushort) PlayerPrefs.GetInt("ConfigClass"),
                        (ushort) PlayerPrefs.GetInt("ConfigId"),
                        (uint) PlayerPrefs.GetInt("TimeStep"),
                        PlayerPrefs.GetString("UserGameLog"),
                        OptimalLogfilePath,
                        GameParameters.DEGREE_OF_OPT);
            //Debug.Log("Parameters: " + (ushort)PlayerPrefs.GetInt("ConfigClass") + ", " + (ushort)PlayerPrefs.GetInt("ConfigId") + ", " + (uint)PlayerPrefs.GetInt("TimeStep"));
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            try { 
                status = opt.Get_Status(out currentProgress);
                if (status == SCGMS_Game_Opt.Optimizer_Status.Running)
                {
                    slider.value = (float) currentProgress;
                    var progressInPercent = (int) Math.Round(currentProgress * 100);
                    progressText.text = progressInPercent + " %";
                }
                else
                {
                    opt.Finalize_Output();
                    String logfilesPath = OptimalLogfilePath;
                    /*TODO - in the future when the options for BOTH_GAME will be available
                    var tmp = PlayerPrefs.GetString("ReplayLogfile");
                    logfilesPath = tmp + ", " + OptimalLogfilePath; //maybe another separator will be needed
                    */
                    PlayerPrefs.SetString("ReplayLogfile", logfilesPath);
                    
                    SceneManager.LoadScene(GameParameters.PLAYBACK_SCENE);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Optimizer error: " + ex.Message);
            }
        }

    }
}
