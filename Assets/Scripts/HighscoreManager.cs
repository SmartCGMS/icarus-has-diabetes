using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using System.IO;

namespace gpredict3_gaming.Ikaros
{
    public class HighscoreManager : MonoBehaviour
    {

        private Transform EntryContainer;
        private Transform EntryTemplate;

        public string EntryContainerName;
        public string EntryTemplateName;
        public string RankName;
        public string ScoreName;
        public string NicknameName;
        public string TrophyName;
        public string BgEntryName;

        private int NumberResults = 10;
        private float TemplateHeight = 25.0f;

        private List<ScoreEntry> ScoreEntryList;
        private List<Transform> HighscoreEntryTransformList;

        private string CurrentDifficulty = "easy";

        private string CurrentTypeScore = "local";




        private void Awake()
        {
            EntryContainer = transform.Find(EntryContainerName);
            EntryTemplate = EntryContainer.Find(EntryTemplateName);
            EntryTemplate.gameObject.SetActive(false);

            GenerateHighscoreTable();

        }

        private void GenerateHighscoreTable()
        {
            
            ClearTable(HighscoreEntryTransformList);
            AllScores highscores = ScoreDataStorage.LoadAllSavedScores(CurrentTypeScore);

            ScoreEntryList = (List<ScoreEntry>) highscores.GetType().GetField(CurrentDifficulty).GetValue(highscores);

            //Sort entry list by Score
            ScoreEntryList = ScoreEntryList.OrderByDescending(e => e.score).ToList();

            HighscoreEntryTransformList = new List<Transform>();


            int range = Math.Min(NumberResults, ScoreEntryList.Count);
            for (int i = 0; i < range; i++)
            {
                CreateHighscoreEntryTransform(ScoreEntryList.ElementAt(i), EntryContainer, HighscoreEntryTransformList);
            }
        }

        private void ClearTable(List<Transform> transformList)
        {
            if (HighscoreEntryTransformList != null)
            {
                foreach (Transform entryTransform in transformList)
                {
                    Destroy(entryTransform.gameObject);
                }
            }
        }


        private void CreateHighscoreEntryTransform(ScoreEntry scoreEntry, Transform container, List<Transform> transformList)
        {
            Transform entryTransform = Instantiate(EntryTemplate, container);
            RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
            entryRectTransform.anchoredPosition = new Vector2(0, -transformList.Count * TemplateHeight);
            entryTransform.gameObject.SetActive(true);

            int rank = transformList.Count + 1;
            string rankString = ScoreDataStorage.RankToString(rank);
            entryTransform.Find(RankName).GetComponent<Text>().text = rankString;

            double score = scoreEntry.score;
            entryTransform.Find(ScoreName).GetComponent<Text>().text = score.ToString("F0");

            string nickname = scoreEntry.nickname;
            entryTransform.Find(NicknameName).GetComponent<Text>().text = nickname;

            //Set background visible odds, easier to read
            entryTransform.Find(BgEntryName).gameObject.SetActive((rank % 2) == 1);

            //Highlight first
            if (rank == 1)
            {
                entryTransform.Find(RankName).GetComponent<Text>().color = Color.blue;
                entryTransform.Find(ScoreName).GetComponent<Text>().color = Color.blue;
                entryTransform.Find(NicknameName).GetComponent<Text>().color = Color.blue;
            }

            switch (rank)
            {
                case 1: entryTransform.Find(TrophyName).GetComponent<Image>().color = new Color(1.0f, 210f / 255f, 0f, 1.0f); break;
                case 2: entryTransform.Find(TrophyName).GetComponent<Image>().color = new Color(198f / 255f, 198f / 255f, 198f / 255f, 1.0f); break;
                case 3: entryTransform.Find(TrophyName).GetComponent<Image>().color = new Color(183f / 255f, 111f / 255f, 86f / 255f, 1.0f); break;
                default: entryTransform.Find(TrophyName).gameObject.SetActive(false); break;

            }


            transformList.Add(entryTransform);
        }

        


        public void ChangeDifficultForHighscores(Toggle sender)
        {
            if (sender.isOn)
            {
                CurrentDifficulty = sender.tag.ToLower();
                GenerateHighscoreTable();
            }
        }


        public void ChangeTypeHighscores(Toggle sender)
        {
            if (sender.isOn)
            {
                CurrentTypeScore = sender.tag;
                GenerateHighscoreTable();
            }
        }

    }
}
