using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace gpredict3_gaming.Ikaros
{
    public class ScoreManager : MonoBehaviour
    {
        public static double Score;
        public static float ScoreCoef;

        private Text ScoreText;

        private static readonly double MaxScore = 50.0;
        public static readonly double TargetMmolL = 5.5;
        public static readonly double MaxDiffMmolL = 5.0;

        /// <summary>
        /// Initialization of score label
        /// </summary>
        private void Start()
        {
            ScoreText = GetComponent<Text>();
            Score = 0;
            ScoreCoef = 1.0f;
        }

        /// <summary>
        /// Update the score label
        /// </summary>
        private void Update()
        {
            if (Score < 0)
            {
                Score = 0;
            }

            ScoreText.text = "" + Score.ToString("F0");
        }

        /// <summary>
        /// Calcalution of score which is based on the difference between current glucose level and ideal glucose level.
        /// </summary>
        /// <param name="curMmolL">current glucose level</param>
        public static void IncreaseScore(double curMmolL)
        {
            //TODO: improve calculation
            double diff = Math.Abs(TargetMmolL - curMmolL);

            if(diff > MaxDiffMmolL)
            {
                diff = MaxDiffMmolL;
            }

            Score += MaxScore  * (1.0 - (diff/MaxDiffMmolL)) * ScoreCoef;
        }

        /// <summary>
        /// Reset the score
        /// </summary>
        public static void Reset()
        {
            Score = 0;
        }
    }
}