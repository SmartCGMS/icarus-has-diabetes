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
    public class ReplayPlayerController : PlayerCharacter
    {
        private float SugarEffectValue = -1.0f;
        private float BolusEffectValue = -1.0f;
        public GameObject Legend;

        public GameObject Basal;
        private Slider BasalSlider;
        private Text BasalText;
        private string BasalStartText;

        public void Awake()
        {
            base.Awake();
            var typeReplay = PlayerPrefs.GetInt("TypeReplay");
            if (typeReplay != (int) TypeReplay.BOTH)
            {
                var transformLegend = Legend.GetComponent<RectTransform>();
                transformLegend.anchoredPosition = new Vector2(20, -20);
                var transformBasal = Basal.GetComponent<RectTransform>();
                transformBasal.anchoredPosition = new Vector2(368, -122);
            }
            Legend.SetActive(true);
            BasalInitialization();

        }



        public override void OnSimulationTick(float gameTime)
        {
            //TODO: ??? musim prozkoumat, zda to tu ma byt
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
                    Debug.Log("Meal Log - Val: " + meal + " time: " + gameTime);
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


        private void AddCarbEffect(double val)
        {
            SugarEffectStartTime = TimeCtrl.GetActualTime();
            SugarEffectValue = (float)val;
        }

        protected override void ResetCarbEffect()
        {
            SugarEffectStartTime = -1;
            SugarEffectValue = -1;
        }

        private void AddBolusEffect(double val)
        {
            BolusEffectStartTime = TimeCtrl.GetActualTime();
            BolusEffectValue = (float)val;
        }

        protected override void ResetBolusEffect()
        {
            BolusEffectStartTime = -1;
            BolusEffectValue = -1;
        }

        public override void ChangeBasal(float newBasal)
        {
            base.ChangeBasal(newBasal);
            BasalSlider.value = newBasal;
            BasalText.text = BasalStartText +newBasal.ToString("F1",GameParameters.nfi) + " U/hr";
        }

        private void BasalInitialization()
        {
            Basal.SetActive(true);
            BasalSlider = Basal.GetComponent<Slider>();
            BasalSlider.minValue = TouchControls.MinBasalRate;
            BasalSlider.maxValue = TouchControls.MaxBasalRate;
            BasalText = Basal.GetComponentInChildren<Text>();
            BasalStartText = BasalText.text + System.Environment.NewLine;
            BasalText.text = BasalStartText + " 0.0 U/hr";
        }



    }
}
