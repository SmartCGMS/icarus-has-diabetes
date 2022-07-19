using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace gpredict3_gaming.Ikaros
{

    public class GameOverMenu : MonoBehaviour
    {
        /// <summary>
        /// Class for storage info about game and player
        /// </summary>
        private class Metadata
        {
            public int score;
            public int player_qualification;
            public string disease_code;
            public string nickname;
            public int difficulty;
            public string date;
        }

        private class ResponseMeta
        {
            public string id;
        }

        //displaying the achieved score and achieved rank
        public string NameScoreLabel;
        private double AchievedScore;
        private Text ScoreLabel;

        public string NameRankLabel;
        private Text RankLabel;

        public string NameModeLabel;
        private Text ModeLabel;

        public string NamePlaybackButton;
        private Button PlaybackBtn;

        
        private string LogMetadataExt = ".json";
        private string LocalDestDirName = "Logs";
        private string SubmittedDirName = "Submitted";

        private Metadata metadata;

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        void Start()
        {
            AchievedScore = ScoreManager.Score;
            ScoreLabel = GameObject.Find(NameScoreLabel).GetComponent<Text>();
            ScoreLabel.text = "SCORE: " + AchievedScore.ToString("F0");

            RankLabel = GameObject.Find(NameRankLabel).GetComponent<Text>();
            ModeLabel = GameObject.Find(NameModeLabel).GetComponent<Text>();
            PlaybackBtn = GameObject.Find(NamePlaybackButton).GetComponent<Button>();

            int typeGame = PlayerPrefs.GetInt("type_game");
            if (typeGame == (int) TypeGame.FULL_GAME)
            {
                string nickname = PlayerPrefs.GetString("nickname");
                string difficulty = PlayerPrefs.GetString("difficulty_name");
                ScoreDataStorage.AddLocalScoreEntry((int)Math.Round(AchievedScore), nickname, difficulty);

                RankLabel.text = "LOCAL RANK: " + ScoreDataStorage.GetLocalRank((int)Math.Round(AchievedScore), nickname, difficulty);
                RankLabel.gameObject.SetActive(true);

                ModeLabel.text = "MODE: " + difficulty;
                ModeLabel.gameObject.SetActive(true);

                PlaybackBtn.gameObject.SetActive(true);

                DateTime dateTime = DateTime.Now;
                metadata = new Metadata { score = (int) Math.Round(AchievedScore),
                                          player_qualification = PlayerPrefs.GetInt("player_qualification"),
                                          disease_code = PlayerPrefs.GetString("disease_code"),
                                          nickname = nickname,
                                          difficulty = PlayerPrefs.GetInt("difficulty_id"),
                                          date = dateTime.ToString("yyyy-MM-dd HH:mm:ss") };
                CopyLogAndSubmit(dateTime);
            }
            else
            {
                RankLabel.gameObject.SetActive(false);
                ModeLabel.gameObject.SetActive(false);
                PlaybackBtn.gameObject.SetActive(false);
                if(typeGame == (int)TypeGame.PLAYBACK)
                {
                    ScoreLabel.gameObject.SetActive(false);
                }
            }


        }

        private void Submit(string dirWithFile, string logFilename)
        {
            string metadataAsJson = JsonUtility.ToJson(metadata);
            StartCoroutine(PostRequest(metadataAsJson, dirWithFile, logFilename));

        }

        IEnumerator PostRequest(string jsonString, string dirWithFile, string filename)
        {
            using UnityWebRequest requestFirstPass = UnityWebRequest.Post(GameParameters.LOG_URL_BASE + GameParameters.UPLOAD_META_LOCATION, jsonString);
            requestFirstPass.downloadHandler = new DownloadHandlerBuffer();
            requestFirstPass.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonString));
            requestFirstPass.SetRequestHeader("Content-Type", "application/json");
            requestFirstPass.certificateHandler = new BypassCertificate();
            yield return requestFirstPass.SendWebRequest();

            if (requestFirstPass.responseCode == GameParameters.RESPONSE_OK)
            {
                string responseJson = requestFirstPass.downloadHandler.text;
                var response = JsonUtility.FromJson<ResponseMeta>(responseJson);

                string fileFullPath = Path.Combine(dirWithFile, filename + GameParameters.LOGFILE_EXT);
                byte[] logFile = File.ReadAllBytes(fileFullPath);

                var dataLog = new List<IMultipartFormSection>();
                dataLog.Add(new MultipartFormFileSection("log", logFile, filename + GameParameters.LOGFILE_EXT, "text/plain"));

                byte[] boundary = UnityWebRequest.GenerateBoundary();

                string urlLog = GameParameters.LOG_URL_BASE + GameParameters.UPLOAD_DATA_LOCATION + response.id;
                using UnityWebRequest requestSecondPass = UnityWebRequest.Post(urlLog, dataLog, boundary);
                requestSecondPass.downloadHandler = new DownloadHandlerBuffer();
                requestSecondPass.certificateHandler = new BypassCertificate();
                yield return requestSecondPass.SendWebRequest();

                if (requestSecondPass.responseCode == GameParameters.RESPONSE_OK)
                {
                    var destDir = Path.Combine(dirWithFile, SubmittedDirName);
                    if (!Directory.Exists(destDir))
                    {
                        Debug.Log("Destination directory not found.");
                        Directory.CreateDirectory(destDir);
                    }
                    var sourceFileFullPathLog = Path.Combine(dirWithFile, filename + GameParameters.LOGFILE_EXT);
                    var sourceFileFullPathMeta = Path.Combine(dirWithFile, filename + LogMetadataExt);
                    var destFileFullPathLog = Path.Combine(destDir, filename + GameParameters.LOGFILE_EXT);
                    var destFileFullPathMeta = Path.Combine(destDir, filename + LogMetadataExt);
                    File.Move(sourceFileFullPathLog, destFileFullPathLog);
                    File.Move(sourceFileFullPathMeta, destFileFullPathMeta);
                }
                else
                {
                    Debug.Log("Submission was failed!");
                }

            }
        }

        /// <summary>
        /// Method for displaying the hall of fame
        /// </summary>
        public void HighScore()
        {
            SceneManager.LoadScene(GameParameters.SCOREBOARD_SCENE);
        }

        /// <summary>
        /// Method for return to main menu
        /// </summary>
        public void BackToMenu()
        {
            SceneManager.LoadScene(GameParameters.MAIN_MENU_SCENE);
        }



        /// <summary>
        /// Show the offer for playback
        /// </summary>
        public void Playback()
        {
            SceneManager.LoadScene(GameParameters.PLAYBACK_MENU);
        }


        /// <summary>
        /// Copy game log to the repository and submit it on the server
        /// </summary>
        /// <param name="dateTime"></param>
        private void CopyLogAndSubmit(DateTime dateTime)
        {
            string sourceDir = Directory.GetParent(Application.persistentDataPath).FullName;
            string destDir = Path.Combine(sourceDir, LocalDestDirName);
            string sourceFileFullPath = PlayerPrefs.GetString("UserGameLog");
            //Debug.Log(sourceDir);
            //Debug.Log(destDir);
            //Debug.Log(sourceFileFullPath);
            FileInfo fS = new FileInfo(sourceFileFullPath);

            if (fS.Exists)
            {
                if (!Directory.Exists(destDir))
                {
                    Debug.Log("Destination directory not found.");
                    Directory.CreateDirectory(destDir);
                }
                string destFilename = GameParameters.LOGFILE_PREFIX + "_" + dateTime.ToString("yyyy-MM-dd_HH-mm-ss");
                string destFileFullPathLog = Path.Combine(destDir, destFilename + GameParameters.LOGFILE_EXT);
                string destFileFullPathMeta = Path.Combine(destDir, destFilename + LogMetadataExt);
                File.Copy(sourceFileFullPath, destFileFullPathLog, true);

                string metadataAsJson = JsonUtility.ToJson(metadata);
                File.WriteAllText(destFileFullPathMeta, metadataAsJson);

                Submit(destDir, destFilename);
            }
            else
            {
                Debug.Log("Log file not found");
            }
            
        }


    }
}
