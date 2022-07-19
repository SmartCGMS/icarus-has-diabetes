using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using GameDummyTest;
using UnityEditorInternal;

namespace gpredict3_gaming.Ikaros
{
    //[RequireComponent(typeof(Rigidbody2D))]
    /// <summary>
    /// Class which represents the character in playback mode
    /// </summary>
    public class ReplayPlayerController : PlayerCharacter
    {
        //carbs and bolus
        private float SugarEffectValue = -1.0f;
        private float BolusEffectValue = -1.0f;
        private Text SugarEffectText;
        private Text BolusEffectText;
        public GameObject Legend;

        //basal
        public GameObject Basal;
        private Slider BasalSlider;
        private Text BasalText;
        private string BasalStartText;

        private static readonly float Margin = 20.0f, Space = 20.0f;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        public void Awake()
        {
            base.Awake();
            BasalInitialization();
            EffectInitialization();
            var typeReplay = PlayerPrefs.GetInt("TypeReplay");
            if (typeReplay != (int) TypeReplay.BOTH)
            {
                var transformLegend = Legend.GetComponent<RectTransform>();
                transformLegend.anchoredPosition = new Vector2(Margin, -Margin);
                var transformBasal = Basal.GetComponent<RectTransform>();
                var transformBasalText = BasalText.GetComponent<RectTransform>();
                transformBasal.anchoredPosition = new Vector2((transformBasal.rect.height + Space)/2, -transformBasalText.rect.height/2); //x-coord use height of basal slider due to rotation
            }
            Legend.SetActive(true);
            Basal.SetActive(true);
            
        }


        /// <summary>
        /// Player reaction to time ticks marked as "simulation" (the new values are obtained from the CGMS simulation on the background)
        /// </summary>
        /// <param name="gameTime">actual game time </param>
        public override void OnSimulationTick(float gameTime)
        {
            if (!isSimulationInitialized)
            {
                isSimulationInitialized = true;
                return;
            }
            double bolusVal = 0, CHOVal = 0;
            if (Game.Step()) 
            { 
                MealCtrl.ScheduleMeal();
                if(Game.CarbsRegular.Count > 0)
                {
                    var meal = 0.0;
                    foreach (var val in Game.CarbsRegular)
                    {
                        meal += val;
                        
                    }
                    //Debug.Log("Meal Log - Val: " + meal + " time: " + gameTime);
                }

                if (Game.CarbsRescue.Count > 0)
                {
                    foreach(var val in Game.CarbsRescue)
                    {
                        CHOVal += val;
                    }
                    AddCarbEffect(CHOVal);

                    //Debug.Log("CHO Pend - Val: " + SugarEffectValue + " time: " + SugarEffectStartTime);

                }

                if (Game.Boluses.Count > 0)
                {
                    foreach (var val in Game.Boluses)
                    {
                        bolusVal += val;

                    }
                    AddBolusEffect(bolusVal);

                    //Debug.Log("Bolus Pend - Val: " + BolusEffectValue + " time: " + BolusEffectStartTime);

                }

                if(Game.Basals.Count > 0)
                {
                    foreach (var val in Game.Basals)
                    {
                        ChangeBasal((float) val);
                        //Debug.Log("Basal - Val: " + BasalRate + " time: " + gameTime);
                    }
                }

                SetMMolLValue(Game.BloodGlucose);
            }

        }

        /// <summary>
        /// Set the game effect for carbs
        /// </summary>
        /// <param name="val">amount of carbs</param>
        private void AddCarbEffect(double val)
        {
            SugarEffectStartTime = TimeCtrl.GetActualTime();
            SugarEffectValue = (float)val;
            SugarEffectText.text = Math.Round(SugarEffectValue).ToString("F0");
        }

        /// <summary>
        /// Reset of game effect for carbs
        /// </summary>
        protected override void ResetCarbEffect()
        {
            SugarEffectStartTime = -1;
            SugarEffectValue = -1;
        }

        /// <summary>
        /// Set the game effect for bolus
        /// </summary>
        /// <param name="val">size of bolus</param>
        private void AddBolusEffect(double val)
        {
            BolusEffectStartTime = TimeCtrl.GetActualTime();
            BolusEffectValue = (float)val;
            BolusEffectText.text = Math.Round(BolusEffectValue).ToString("F0");
        }

        /// <summary>
        /// Reset of game effect for bolus
        /// </summary>
        protected override void ResetBolusEffect()
        {
            BolusEffectStartTime = -1;
            BolusEffectValue = -1;
        }

        /// <summary>
        /// Change value of basal
        /// </summary>
        /// <param name="newBasal">the new basal value</param>
        public override void ChangeBasal(float newBasal)
        {
            base.ChangeBasal(newBasal);
            BasalSlider.value = newBasal;
            BasalText.text = BasalStartText + newBasal.ToString("F1",GameParameters.nfi) + " U/hr";
        }

        /// <summary>
        /// Initialization of basal slider
        /// </summary>
        private void BasalInitialization()
        {
            BasalSlider = Basal.GetComponent<Slider>();
            BasalSlider.minValue = GameParameters.MIN_BASAL_RATE;
            BasalSlider.maxValue = GameParameters.MAX_BASAL_RATE;
            BasalText = Basal.GetComponentInChildren<Text>();
            BasalStartText = BasalText.text + System.Environment.NewLine;
            BasalText.text = BasalStartText + " 0.0 U/hr";
        }

        /// <summary>
        /// Initialization of game objects for effects
        /// </summary>
        private void EffectInitialization()
        {
            BolusEffectText = Bolus.GetComponentInChildren<Text>();
            SugarEffectText = Sugar.GetComponentInChildren<Text>();
        }



    }
}
