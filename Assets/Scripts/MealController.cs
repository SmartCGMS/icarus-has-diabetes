using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace gpredict3_gaming.Ikaros
{

    public class MealController : MonoBehaviour
    {
        // range of meal size
        public static readonly float MinMealValue = 31;
        public static readonly float MaxMealValue = 100;

        //range of time between meals (in seconds)
        //one second equal to 25 minutes in SmartCGMS backend => time between meals in backend is from 3 hours to 5 hours
        private static readonly float MinMealTime = 7.2f;
        private static readonly float MaxMealTime = 12.0f;
        /*
        //old steppnig - one second equal to 5 minutes in SmartCGMS backend => time between meals in backend is from 3 hours to 5 hours
        private static readonly float MinMealTime = 36.0f;
        private static readonly float MaxMealTime = 60.0f;
        */

        //Scheduler of next meal
        public float NextMealTime { get; private set; }
        public float NextMealValue { get; private set; }
        public bool IsMealScheduled { get; set; }

        private System.Random RGen = new System.Random();


        private void Start()
        {
            ResetMeal();
        }

        /// <summary>
        /// Reaction when there is the time for meal
        /// </summary>
        /// <param name="gameTime">the current time of game</param>
        public void ScheduleMeal(float gameTime)
        {
            if (NextMealTime < 0)
            {
                NextMealTime = gameTime + MinMealTime + (float)RGen.NextDouble() * (MaxMealTime - MinMealTime);
                NextMealValue = MinMealValue + (float)RGen.NextDouble() * (MaxMealValue - MinMealValue);
                MealDataStorage.addMeal(NextMealTime, NextMealValue);
            }
        }

        public void ScheduleMeal()
        {
            if(NextMealTime < 0 && MealDataStorage.existNextMeal())
            {
                var meal = MealDataStorage.removeFirstMeal();
                NextMealTime = meal.MealTime;
                NextMealValue = meal.MealValue;
                Debug.Log("Scheduled meal at time " + NextMealTime + ", the size of meal is " + NextMealValue); 
            }
        }




        /// <summary>
        /// Reset meal scheduler
        /// </summary>
        public void ResetMeal()
        {
            NextMealTime = -1;
            NextMealValue = -1;
            IsMealScheduled = false;
        }
    }
}