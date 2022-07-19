using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.IO;
using UnityEngine.Networking;

namespace gpredict3_gaming.Ikaros
{
    /// <summary>
    /// Represents a single record of score
    /// </summary>
    [System.Serializable]
    public class ScoreEntry
    {
        public string nickname;
        public double score;
    }

    /// <summary>
    /// Storage for all records of scores for different difficulties
    /// </summary>
    [System.Serializable]
    public class AllScores
    {
        public List<ScoreEntry> easy;
        public List<ScoreEntry> medium;
        public List<ScoreEntry> hard;
    }


    public class Response
    {
        public AllScores scoreboard;
    }


    /// <summary>
    /// Management storage of scores
    /// </summary>
    public class ScoreDataStorage : MonoBehaviour
    {
        private static ScoreDataStorage localInstance;
        public static ScoreDataStorage storage { get { return localInstance; } }


        private static string HallOfFameFilename = "hallOfFame";
        private static string HallOfFameExt = ".json";

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            if(localInstance != null && localInstance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                localInstance = this;
            }
            
        }

        /// <summary>
        /// Add the new record of score to local storage
        /// </summary>
        /// <param name="score">number of points</param>
        /// <param name="name">nickname</param>
        /// <param name="difficulty">difficulty of game</param>
        public static void AddLocalScoreEntry(int score, string name, string difficulty)
        {
            difficulty = difficulty.ToLower();
            //Create highscore entry
            ScoreEntry scoreEntry = new ScoreEntry { nickname = name, score = score };

            //Load saved Highscores
            AllScores allScores = LoadAllSavedLocalScores();

            //Add new entry to Highscores
            ((List<ScoreEntry>)allScores.GetType().GetField(difficulty).GetValue(allScores)).Add(scoreEntry);

            //Save updated Highscores
            SaveLocalScores(allScores);
        }

        /// <summary>
        /// Search local ranking depending on the achieved score
        /// </summary>
        /// <param name="score">number of points</param>
        /// <param name="name">nickname</param>
        /// <param name="difficulty">difficulty of game</param>
        /// <returns>local ranking</returns>
        public static string GetLocalRank(int score, string name, string difficulty)
        {
            difficulty = difficulty.ToLower();
            //Load saved Highscores
            AllScores allScores = LoadAllSavedLocalScores();

            List<ScoreEntry> scoreListByDifficulty = (List<ScoreEntry>)allScores.GetType().GetField(difficulty).GetValue(allScores);
            //Sort entry list by Score
            scoreListByDifficulty = scoreListByDifficulty.OrderByDescending(e => e.score).ToList();

            int rank = 1;
            ScoreEntry tmpEntry;
            for (int i = 0; i < scoreListByDifficulty.Count; i++)
            {
                tmpEntry = scoreListByDifficulty.ElementAt(i);
                if (tmpEntry.nickname.Equals(name) && tmpEntry.score == score)
                {
                    rank = i + 1;
                    break;
                }
            }
            return RankToString(rank);
        }

        /// <summary>
        /// Convert rank to string
        /// </summary>
        /// <param name="rank">rank</param>
        /// <returns>rank as string</returns>
        public static string RankToString(int rank)
        {
            string rankString;
            switch (rank)
            {
                case 1: rankString = "1st"; break;
                case 2: rankString = "2nd"; break;
                case 3: rankString = "3rd"; break;
                default: rankString = rank + "th"; break;
            }

            return rankString;
        }

        /// <summary>
        /// Loading all saved local scores from JSON file
        /// </summary>
        /// <returns>All saved records</returns>
        public static AllScores LoadAllSavedLocalScores()
        {
            string directory = Directory.GetParent(Application.persistentDataPath).FullName;
            string path = Path.Combine(directory, HallOfFameFilename + HallOfFameExt);

            AllScores allScores;
            if (File.Exists(path))
            {
                string dataAsJson = File.ReadAllText(path);
                allScores = JsonUtility.FromJson<AllScores>(dataAsJson);
            }
            else
            {
                allScores = new AllScores { easy = new List<ScoreEntry>(), medium = new List<ScoreEntry>(), hard = new List<ScoreEntry>() };
            }

            return allScores;

        }

        /// <summary>
        /// Loading all saved global scores from JSON file on server
        /// </summary>
        /// <returns>All saved records</returns>
        public static AllScores LoadAllSavedGlobalScores()
        {
            //AllScores allScores = null;
            Response response = new Response();

            if (storage == null)
            {
                GameObject obj = new GameObject("StorageHolder");
                localInstance = obj.AddComponent<ScoreDataStorage>();
            }

            IEnumerator e = storage.GetGlobalScoreboard(response);
            while (e.MoveNext());
            //Debug.Log(JsonUtility.ToJson(response.scoreboard));
            return response.scoreboard;
        }

        /// <summary>
        /// Loading chosen type of scores
        /// </summary>
        /// <param name="typeScore">type of score</param>
        /// <returns>chosen scores</returns>
        public static AllScores LoadAllSavedScores(string typeScore)
        {
            AllScores allScores = null;
            if(typeScore == GameParameters.LOCAL_TYPE_SCORE)
            {
                allScores = LoadAllSavedLocalScores();
            }
            else if(typeScore == GameParameters.GLOBAL_TYPE_SCORE)
            {
                allScores = LoadAllSavedGlobalScores();
            }

            return allScores;
        }


        /// <summary>
        /// Obtain scoreboard from JSON file on server
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        IEnumerator GetGlobalScoreboard(Response response)
        {
            UnityWebRequest request = UnityWebRequest.Get(GameParameters.LOG_URL_BASE + GameParameters.GLOBAL_SCOREBOARD_LOCATION);
            request.certificateHandler = new BypassCertificate();
            yield return request.SendWebRequest();

            while (!request.isDone)
            {
                yield return true;
            }

            if (request.responseCode == GameParameters.RESPONSE_OK)
            {
                Response tmp = JsonUtility.FromJson<Response>(request.downloadHandler.text);
                response.scoreboard = tmp.scoreboard;
            }
            else
            {
                response.scoreboard = new AllScores();
            }
            
        }

        /// <summary>
        /// Save all local scores to JSON file
        /// </summary>
        /// <param name="allScores">storage of all scores</param>
        private static void SaveLocalScores(AllScores allScores)
        {
            string directory = Directory.GetParent(Application.persistentDataPath).FullName;
            string path = Path.Combine(directory, HallOfFameFilename + HallOfFameExt);

            string dataAsJson = JsonUtility.ToJson(allScores);
            File.WriteAllText(path, dataAsJson);
        }

    }
}
