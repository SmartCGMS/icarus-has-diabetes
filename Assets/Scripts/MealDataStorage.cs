using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace gpredict3_gaming.Ikaros
{
    public struct MealEntry
    {
        public float MealTime { get; private set; }
        public float MealValue { get; private set; }

        public MealEntry(float mealTime, float mealValue)
        {
            MealTime = mealTime;
            MealValue = mealValue;
        }

    }
    public static class MealDataStorage
    {
        public static List<MealEntry> MealList { get; private set; }


        public static void addMeal(float mealTime, float mealValue)
        {
            if(MealList == null)
            {
                MealList = new List<MealEntry>();
            }
            MealList.Add(new MealEntry(mealTime, mealValue));
        }

        public static bool existNextMeal()
        {
            return MealList.Count > 0;
        }


        public static MealEntry removeFirstMeal()
        {
            if (existNextMeal())
            {
                var meal = MealList[0];
                MealList.RemoveAt(0);
                return meal;
            }
            else throw new NullReferenceException();
        }

        public static void clearListOfMeal()
        {
            MealList.Clear();
        }

    }
}
