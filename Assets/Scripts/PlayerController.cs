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
    public class PlayerController : MonoBehaviour
    {
        private Animator Animator;
        public SCGMS_Game Game { get; set; }

        private float SpeedOfFly;

        //false if it is the first calling of method OnSimulationTick
        private bool isSimulationInitialized = false;

        public bool IsPatientDead { get; set; } = false;

        //basal
        private float BasalRate = 0.0f;
        private float ChangeBasalTime = -1.0f;
        private bool ChangeBasalWaiting = false;

        //scheduled bolus
        private float PendingBolusValue = -1.0f;
        private float PendingBolusTime = -1.0f;
        private float BolusEffectStartTime = -1.0f;


        //scheduled sugar
        private float PendingSugarValue = -1.0f;
        private float PendingSugarTime = -1.0f;
        private float SugarEffectStartTime = -1.0f;

        private float BolusScoreCooldownStartTime = -1;
        private static readonly float PenalizedScoreCoef = 0.35f;
        private static readonly float NormalScoreCoef = 1.0f;
        private static readonly float PenalizationIntervalInSimTicks = 14.5f;

        //position of player - current and future
        private float LastPosY = 0.0f;
        private float FuturePosY = 0.0f;
        private float FuturePosTime = 0.0f;

        //dimensions of game area
        private float MaxGameAreaYCoord, MinGameAreaYCoord, NormalLevel;
        private float NormalLevelRatio = 0.5f;
        private float GameAreaHeight, GameAreaHalfWidth;
        private float GameAreaCoef;

        private Vector2 PlayerSize, BolusEffectSize, SugarEffectSize, MealEffectSize;

        public MealController MealCtrl;
        public TimeManager TimeCtrl;
        public CurveGenerator CurveBG;

        private static readonly float MinMealScale = 1f;
        private static readonly float MaxMealScale = 2f;


        public GameObject Sugar;
        public GameObject Bolus;
        public GameObject Meal;

        //set color vision
        public GameObject Healthy;
        public Image HealthyIM;
        private bool isHealthyActive = false;
        private float BGValue = (float) ScoreManager.TargetMmolL;




        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            InitializeComponents();
            InitializeDimension();
            InitializePositionsOfGameObjects();
        }



        void InitializeComponents()
        {
            Animator = GetComponent<Animator>();
            ComputeSpeedOfFly();
            Animator.speed = SpeedOfFly; //when speed is 1 -> one loop of animation spent 0.06s, switch between two frames of animation is 1/100 s

            MealCtrl = FindObjectOfType<MealController>();
            TimeCtrl = FindObjectOfType<TimeManager>();
            CurveBG = FindObjectOfType<CurveGenerator>();

            HealthyIM = Healthy.GetComponent<Image>();


        }

        void InitializeDimension()
        {
            var colliderSize = FindObjectOfType<BoxCollider2D>().bounds.size.y;
            MaxGameAreaYCoord = FindObjectOfType<Camera>().orthographicSize;
            MinGameAreaYCoord = -MaxGameAreaYCoord + colliderSize;
            GameAreaHeight = MaxGameAreaYCoord - MinGameAreaYCoord;
            GameAreaHalfWidth = FindObjectOfType<Camera>().aspect * MaxGameAreaYCoord;
            NormalLevel = MinGameAreaYCoord + (MaxGameAreaYCoord - MinGameAreaYCoord) * NormalLevelRatio;
            GameAreaCoef = (GameAreaHeight / 2.0f) / (float)ScoreManager.MaxDiffMmolL;

            //initialize of sizes of game objects
            var pRenderer = GetComponent<SpriteRenderer>();
            PlayerSize = new Vector2(pRenderer.bounds.size.x, pRenderer.bounds.size.y);
            var bRenderer = Bolus.GetComponent<SpriteRenderer>();
            BolusEffectSize = new Vector2(bRenderer.bounds.size.x, bRenderer.bounds.size.y);
            var sRenderer = Sugar.GetComponent<SpriteRenderer>();
            SugarEffectSize = new Vector2(sRenderer.bounds.size.x, sRenderer.bounds.size.y);

            var particle = Meal.GetComponent<ParticleSystem>();
            var mRad = particle.shape.radius;
            var mVelocity = new Vector2(particle.velocityOverLifetime.xMultiplier, particle.velocityOverLifetime.yMultiplier);
            MealEffectSize = 2 * (mVelocity + new Vector2(mRad, mRad));

        }

        void InitializePositionsOfGameObjects()
        {
            transform.position = new Vector3(-GameAreaHalfWidth * 0.5f, NormalLevel);
            Bolus.transform.position = new Vector3(-GameAreaHalfWidth * 0.5f, Bolus.transform.position.y);
            Sugar.transform.position = new Vector3(-GameAreaHalfWidth * 0.5f, Sugar.transform.position.y);
        }

            
        public void OnSimulationTick(float gameTime)
        {
            if (!isSimulationInitialized)
            {
                isSimulationInitialized = true;
                return;
            }

            double mealVal = 0, basal, bolusVal = 0, CHOVal = 0;
            double mealTime = 0, basalTime, bolusTime = 0, CHOTime = 0;
            MealCtrl.ScheduleMeal(gameTime);

            if (MealCtrl.GetNextMealTime() <= gameTime && !MealCtrl.IsMealScheduled)
            {
                mealVal = MealCtrl.GetNextMealValue();
                mealTime = 1-((Math.Abs(gameTime - MealCtrl.GetNextMealTime()))/TimeCtrl.GetSimulationTickIntervalSecs());
                if (mealTime < 0) mealTime = 0.0;
                Game.ScheduleCarbohydratesIntake(mealVal, mealTime);
                //Debug.Log("MEAL - Val: " + mealVal + " time: " + mealTime);
                
                MealCtrl.IsMealScheduled = true;
            }

            if (PendingSugarValue > 0)
            {
                //Debug.Log("CHO Pend - Val: " + PendingSugarValue + " time: " + PendingSugarTime + " game time: " + gameTime);
                CHOVal = PendingSugarValue;
                CHOTime = 1 - ((Math.Abs(gameTime - PendingSugarTime)) / TimeCtrl.GetSimulationTickIntervalSecs());
                if (CHOTime < 0) CHOTime = 0.0;
                Game.ScheduleCarbohydratesRescue(CHOVal, CHOTime);
                //Debug.Log("CHO - Val: " + CHOVal + " time: " + CHOTime);
                
                ResetCarb();
            }

            if (ChangeBasalWaiting)
            {
                basal = BasalRate;
                basalTime = 1 - ((Math.Abs(gameTime - ChangeBasalTime)) / TimeCtrl.GetSimulationTickIntervalSecs());
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
                bolusTime = 1 - ((Math.Abs(gameTime - PendingBolusTime)) / TimeCtrl.GetSimulationTickIntervalSecs());
                if (bolusTime < 0) bolusTime = 0.0;
                Game.ScheduleInsulinBolus(bolusVal, bolusTime);
                //Debug.Log("BOLUS - Val: " + bolusVal + " time: " + bolusTime);
                
                ScoreManager.ScoreCoef = PenalizedScoreCoef;
                ResetBolus();
            }
           
            if ((BolusScoreCooldownStartTime + PenalizationIntervalInSimTicks * TimeCtrl.GetSimulationTickIntervalSecs()) <= gameTime)
            {
                ScoreManager.ScoreCoef = NormalScoreCoef;
                BolusScoreCooldownStartTime = -1;
            }
           
            Game.Step();
            SetMMolLValue(Game.BloodGlucose);

        }

        public void OnInterpolationTick(float deltaGameTime)
        {
            FuturePosTime += deltaGameTime;
            var ActualPosY = (LastPosY + (FuturePosTime / TimeCtrl.GetSimulationTickIntervalSecs()) * (FuturePosY - LastPosY));
            ActualPosY = ((ActualPosY + 0.5f * PlayerSize.y) > MaxGameAreaYCoord) ? (MaxGameAreaYCoord - 0.5f * PlayerSize.y) : ActualPosY;
            ActualPosY = ((ActualPosY - 0.5f * PlayerSize.y) < MinGameAreaYCoord) ? (MinGameAreaYCoord + 0.5f * PlayerSize.y) : ActualPosY;
            transform.position = new Vector3(transform.position.x, ActualPosY);


            //meal sprite position - actual time is positioned before bill of bird sprite, time of meal is positioned to begin of meal sprite
            var scaleFactor = MinMealScale + (MaxMealScale-MinMealScale)*(MealCtrl.GetNextMealValue() - MealCtrl.GetMinMealValue()) / (MealCtrl.GetMaxMealValue() - MealCtrl.GetMinMealValue());
            
            var MealXPos = transform.position.x + 0.5f * (PlayerSize.x + MealEffectSize.x * scaleFactor) - (MealCtrl.GetNextMealTime() - TimeCtrl.GetGameTime()) * GameController.VisualSpeedMultiplier;
            var MealYPos = MaxGameAreaYCoord - MealEffectSize.y/2;

            Meal.transform.localScale = new Vector3(scaleFactor, 1f, 1f);
            Meal.transform.position = new Vector3(MealXPos, MealYPos);
            if (Meal.transform.position.x < (-(GameAreaHalfWidth + 0.5f * MealEffectSize.x)))
            {
                MealCtrl.ResetMeal();
            }

            //bolus sprite position
            if (BolusEffectStartTime > 0)
            {
                Bolus.transform.position = transform.position + new Vector3(0, -0.5f * (PlayerSize.y + BolusEffectSize.y));
                Bolus.SetActive(true);
                if (TimeCtrl.GetGameTime() - BolusEffectStartTime > 1.0f)
                {
                    BolusEffectStartTime = -1;
                }
            }
            else
            {
                Bolus.SetActive(false);
            }

            Bolus.transform.position = transform.position + new Vector3(0, -0.5f * (PlayerSize.y + BolusEffectSize.y));
            if (SugarEffectStartTime > 0)
            {
                Sugar.transform.position = transform.position + new Vector3(0, 0.75f * PlayerSize.y  + 0.5f * SugarEffectSize.y);
                Sugar.SetActive(true);
                if (TimeCtrl.GetGameTime() - SugarEffectStartTime > 1.0f)
                {
                    SugarEffectStartTime = -1;
                }

            }
            else
            {
                Sugar.SetActive(false);
            }
            //Debug.Log(TimeCtrl.GetGameTime() + " simulation," + " meal time: " + MealCtrl.GetNextMealTime());
            //Debug.Log(gameTime + " interpolation");
        }


        public void OnHealthyTick()
        {
            if (BGValue <= GameParameters.HYPOGLYCEMIA)
            {
                //White vision
                var coefAlpha = Math.Max(GameParameters.HYPOGLYCEMIA - BGValue, GameParameters.HYPOGLYCEMIA - GameParameters.RANGE_COLOR_GRADIENT);
                var alpha = GameParameters.MIN_COLOR_ALPHA + (GameParameters.MAX_COLOR_ALPHA_HYPO - GameParameters.MIN_COLOR_ALPHA) / GameParameters.RANGE_COLOR_GRADIENT * coefAlpha;
                HealthyIM.color = new Color(1.0f, 1.0f, 1.0f, alpha);
            }
            else if(BGValue >= GameParameters.HYPERGLYCEMIA)
            {
                //Red vision
                var coefAlpha = Math.Min(BGValue - GameParameters.HYPERGLYCEMIA, GameParameters.RANGE_COLOR_GRADIENT);
                var alpha = GameParameters.MIN_COLOR_ALPHA + (GameParameters.MAX_COLOR_ALPHA_HYPER - GameParameters.MIN_COLOR_ALPHA) / GameParameters.RANGE_COLOR_GRADIENT * coefAlpha;
                HealthyIM.color = new Color(1.0f, 0.0f, 0.0f, alpha);

            }
            else
            {
                HealthyIM.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            }

            isHealthyActive = !isHealthyActive;
            Healthy.SetActive(isHealthyActive);
        }


        public void ChangeBasal(float newBasal)
        {
            BasalRate = newBasal;
            ChangeBasalTime = TimeCtrl.GetActualTime();
            ChangeBasalWaiting = true;
            ComputeSpeedOfFly();
            Animator.speed = SpeedOfFly;
        }


        public void ResetBasal()
        {
            ChangeBasalTime = -1;
            ChangeBasalWaiting = false;
        }

        /// <summary>
        /// Event which is done when some bolus button is pressed
        /// </summary>
        /// <param name="value">amount of insulin</param>
        public void AddBolus(int value)
        {
            if(BolusEffectStartTime < 0)
            {
                PendingBolusValue = value;
                PendingBolusTime = TimeCtrl.GetActualTime();
                BolusEffectStartTime = PendingBolusTime;
            }
        }

        public void ResetBolus()
        {
            PendingBolusValue = -1;
            PendingBolusTime = -1;
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

        public void ResetCarb()
        {
            PendingSugarValue = -1;
            PendingSugarTime = -1;
        }

        public void SetMMolLValue(double val)
        {
            if (val <= 0) IsPatientDead = true;
            FuturePosTime = 0.0f;
            LastPosY = FuturePosY;
            BGValue = (float) val;

            Debug.Log(val);

            var PosOfBGPt = CurveBG.GetYValue(TimeCtrl.GetGameTime());
            FuturePosY = PosOfBGPt - (float)(val - ScoreManager.TargetMmolL) * GameAreaCoef;  // note the coordinates are inverted
            ScoreManager.IncreaseScore(val);
        }

        public void SetIOBValue(double val)
        {
        }

        public void SetCOBValue(double val)
        {
        }

        private void ComputeSpeedOfFly()
        {
            SpeedOfFly = GameParameters.MIN_SPEED_OF_FLY + (GameParameters.MAX_SPEED_OF_FLY - GameParameters.MIN_SPEED_OF_FLY) * BasalRate / (GameParameters.MAX_BASAL_RATE - GameParameters.MIN_BASAL_RATE);
        }

        public float GetGameAreaHeight()
        {
            return GameAreaHeight;
        }

        public float GetNormalLevel()
        {
            return NormalLevel;
        }

        public float GetGameAreaHalfWidth()
        {
            return GameAreaHalfWidth;
        }

    }
}
