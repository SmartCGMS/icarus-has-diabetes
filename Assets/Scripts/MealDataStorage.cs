using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace gpredict3_gaming.Ikaros
{
    /// <summary>
    /// Class for storage information about one meal
    /// </summary>
    public struct MealEntry
    {
        /// <summary>
        /// Time of meal
        /// </summary>
        public float MealTime { get; private set; }
        
        /// <summary>
        /// Size of meal
        /// </summary>
        public float MealValue { get; private set; }

        public MealEntry(float mealTime, float mealValue)
        {
            MealTime = mealTime;
            MealValue = mealValue;
        }

    }

    /// <summary>
    /// Class for storage information about all scheduled meals
    /// </summary>
    public static class MealDataStorage
    {
        /// <summary>
        /// List of all meals
        /// </summary>
        public static List<MealEntry> MealList { get; private set; }

        /// <summary>
        /// Add meal to list
        /// </summary>
        /// <param name="mealTime">meal time</param>
        /// <param name="mealValue">meal size</param>
        public static void addMeal(float mealTime, float mealValue)
        {
            if(MealList == null)
            {
                MealList = new List<MealEntry>();
            }
            MealList.Add(new MealEntry(mealTime, mealValue));
        }

        /// <summary>
        /// Is there any meal in the list?
        /// </summary>
        /// <returns>true if the list is not empty, false otherwise</returns>
        public static bool existNextMeal()
        {
            return MealList.Count > 0;
        }

        /// <summary>
        /// Remove first meal from the list
        /// </summary>
        /// <returns>the removed meal</returns>
        public static MealEntry removeFirstMeal()
        {
            if (existNextMeal())
            {
                var meal = MealList[0];
                MealList.RemoveAt(0);
                return meal;
            }
            else throw new NullReferenceException("No other meal");
        }

        /// <summary>
        /// Clear the whole list of meals
        /// </summary>
        public static void clearListOfMeal()
        {
            MealList.Clear();
        }

    }
}
