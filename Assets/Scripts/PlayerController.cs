using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using GameDummyTest;

namespace gpredict3_gaming.Ikaros
{
    //[RequireComponent(typeof(Rigidbody2D))]
    /// <summary>
    /// Class which represents game character
    /// </summary>
    public class PlayerController : PlayerCharacter
    {
        //scheduled bolus
        private float PendingBolusValue = -1.0f;
        private float PendingBolusTime = -1.0f;

        //scheduled sugar
        private float PendingSugarValue = -1.0f;
        private float PendingSugarTime = -1.0f;

        //basal
        private float ChangeBasalTime = -1.0f;
        private bool ChangeBasalWaiting = false;

        //For score computing
        private float BolusScoreCooldownStartTime = -1;
        private static readonly float PenalizedScoreCoef = 0.35f;
        private static readonly float NormalScoreCoef = 1.0f;
        private static readonly float PenalizationIntervalInSimTicks = 14.5f;

        /// <summary>
        /// Player reaction to time ticks marked as "simulation" (the new values are inserted to the CGMS simulation on the background)
        /// </summary>
        /// <param name="gameTime">actual game time </param>
        public override void OnSimulationTick(float gameTime)
        {
            if (!isSimulationInitialized)
            {
                isSimulationInitialized = true;
                return;
            }
            double mealVal = 0, basal, bolusVal = 0, CHOVal = 0;
            double mealTime = 0, basalTime, bolusTime = 0, CHOTime = 0;
            MealCtrl.ScheduleMeal(gameTime);

            if (MealCtrl.NextMealTime <= gameTime && !MealCtrl.IsMealScheduled)
            {
                mealVal = MealCtrl.NextMealValue;
                mealTime = 1 - ((Math.Abs(gameTime - MealCtrl.NextMealTime)) / TimeManager.SimulationTickIntervalSecs);
                if (mealTime < 0) mealTime = 0.0;
                Game.ScheduleCarbohydratesIntake(mealVal, mealTime);
                //Debug.Log("GAME - MEAL - Val: " + mealVal + " time: " + mealTime + " game time:" + gameTime + " nextMealTime: " + MealCtrl.NextMealTime);

                MealCtrl.IsMealScheduled = true;
            }

            if (PendingSugarValue > 0)
            {
                //Debug.Log("CHO Pend - Val: " + PendingSugarValue + " time: " + PendingSugarTime + " game time: " + gameTime);
                CHOVal = PendingSugarValue;
                CHOTime = 1 - ((Math.Abs(gameTime - PendingSugarTime)) / TimeManager.SimulationTickIntervalSecs);
                if (CHOTime < 0) CHOTime = 0.0;
                Game.ScheduleCarbohydratesRescue(CHOVal, CHOTime);
                //Debug.Log("CHO - Val: " + CHOVal + " time: " + CHOTime);

                ResetCarb();
            }

            if (ChangeBasalWaiting)
            {
                basal = BasalRate;
                basalTime = 1 - ((Math.Abs(gameTime - ChangeBasalTime)) / TimeManager.SimulationTickIntervalSecs);
                if (basalTime < 0) basalTime = 0.0;
                Game.ScheduleInsulinBasalRate(basal, basalTime);
                //Debug.Log("Basal - Val: " + basal + " time: " + basalTime);

                ResetBasal();
            }

            if (PendingBolusValue > 0)
            {
                //Debug.Log("BOLUS Pend - Val: " + PendingBolusValue + " time: " + PendingBolusTime + " game time: " + gameTime);
                BolusScoreCooldownStartTime = gameTime;
                bolusVal += PendingBolusValue;
                bolusTime = 1 - ((Math.Abs(gameTime - PendingBolusTime)) / TimeManager.SimulationTickIntervalSecs);
                if (bolusTime < 0) bolusTime = 0.0;
                Game.ScheduleInsulinBolus(bolusVal, bolusTime);
                //Debug.Log("BOLUS - Val: " + bolusVal + " time: " + bolusTime);

                ScoreManager.ScoreCoef = PenalizedScoreCoef;
                ResetBolus();
            }

            if ((BolusScoreCooldownStartTime + PenalizationIntervalInSimTicks * TimeManager.SimulationTickIntervalSecs) <= gameTime)
            {
                ScoreManager.ScoreCoef = NormalScoreCoef;
                BolusScoreCooldownStartTime = -1;
            }

            Game.Step();
            SetMMolLValue(Game.BloodGlucose);

        }

        /// <summary>
        /// Setting the BG value and score and position of player according to this value
        /// </summary>
        public override void SetMMolLValue(double val)
        {
            base.SetMMolLValue(val);
            ScoreManager.IncreaseScore(val);
        }

        /// <summary>
        /// Event which is done when some bolus button is pressed
        /// </summary>
        /// <param name="value">amount of insulin</param>
        public void AddBolus(int value)
        {
            if (BolusEffectStartTime < 0)
            {
                PendingBolusValue = value;
                PendingBolusTime = TimeCtrl.GetActualTime();
                BolusEffectStartTime = PendingBolusTime;
            }
        }

        /// <summary>
        /// Reset the information about bolus when it was added to simulation
        /// </summary>
        private void ResetBolus()
        {
            PendingBolusValue = -1;
            PendingBolusTime = -1;
        }

        /// <summary>
        /// Reset of game effect for bolus
        /// </summary>
        protected override void ResetBolusEffect()
        {
            BolusEffectStartTime = -1;
        }

        /// <summary>
        /// Event which is done when some sugar button is pressed
        /// </summary>
        /// <param name="value">amount of carbohydrates</param>
        public void AddCarb(int value)
        {
            if (SugarEffectStartTime < 0)
            {
                PendingSugarValue = value;
                PendingSugarTime = TimeCtrl.GetActualTime();
                SugarEffectStartTime = PendingSugarTime;
            }
        }

        /// <summary>
        /// Reset the information about carbs when it was added to simulation
        /// </summary>
        private void ResetCarb()
        {
            PendingSugarValue = -1;
            PendingSugarTime = -1;
        }

        /// <summary>
        /// Reset of game effect for carbs
        /// </summary>
        protected override void ResetCarbEffect()
        {
            SugarEffectStartTime = -1;
        }

        /// <summary>
        /// Change value of basal.
        /// </summary>
        /// <param name="newBasal">the new basal value</param>
        public override void ChangeBasal(float newBasal)
        {
            base.ChangeBasal(newBasal);
            //Debug.Log("Basal: " + newBasal);
            ChangeBasalTime = TimeCtrl.GetActualTime();
            ChangeBasalWaiting = true;
        }


        /// <summary>
        /// Reset the information about change of basal when it was added to simulation
        /// </summary>
        private void ResetBasal()
        {
            ChangeBasalTime = -1;
            ChangeBasalWaiting = false;
        }


    }
}
