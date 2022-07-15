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
    public abstract class PlayerCharacter : MonoBehaviour
    {
        protected Animator Animator;
        public SCGMS_Game Game { get; set; }

        protected float SpeedOfFly;

        //false if it is the first calling of method OnSimulationTick
        protected bool isSimulationInitialized = false;

        //true if the patient dead
        public bool IsPatientDead { get; set; } = false;

        //basal
        protected float BasalRate = 0.0f;

        //scheduled bolus
        protected float BolusEffectStartTime = -1.0f;

        //scheduled sugar
        protected float SugarEffectStartTime = -1.0f;


        //position of player - current and future
        protected float LastPosY = 0.0f;
        protected float FuturePosY = 0.0f;
        protected float FuturePosTime = 0.0f;

        //dimensions of game area
        protected static readonly float NORMAL_LEVEL_RATIO = 0.5f;
        protected float MaxGameAreaYCoord { get; private set; }
        protected float MinGameAreaYCoord { get; private set; }
        public float NormalLevel { get; private set; }
        public float GameAreaHeight { get; private set; }
        public float GameAreaHalfWidth { get; private set; }
        protected float GameAreaCoef { get; private set; }



        protected Vector2 PlayerSize, BolusEffectSize, SugarEffectSize, MealEffectSize;

        public MealController MealCtrl;
        public TimeManager TimeCtrl;
        public CurveGenerator CurveBG;

        protected static readonly float MinMealScale = 1f;
        protected static readonly float MaxMealScale = 2f;


        public GameObject Sugar;
        public GameObject Bolus;
        public GameObject Meal;

        //set color vision
        public GameObject Healthy; 
        public Image HealthyIM;
        protected bool isHealthyActive = false;
        protected float BGValue = (float)ScoreManager.TargetMmolL;


        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        public void Awake()
        {
            InitializeComponents();
            InitializeDimension();
            InitializePositionsOfGameObjects();
        }



        private void InitializeComponents()
        {
            Animator = GetComponent<Animator>();
            ComputeSpeedOfFly();
            Animator.speed = SpeedOfFly; //when speed is 1 -> one loop of animation spent 0.06s, switch between two frames of animation is 1/100 s

            MealCtrl = FindObjectOfType<MealController>();
            TimeCtrl = FindObjectOfType<TimeManager>();
            CurveBG = FindObjectOfType<CurveGenerator>();

            HealthyIM = Healthy.GetComponent<Image>();


        }

        private void InitializeDimension()
        {
            var colliderSize = FindObjectOfType<BoxCollider2D>().bounds.size.y;
            MaxGameAreaYCoord = FindObjectOfType<Camera>().orthographicSize;
            MinGameAreaYCoord = -MaxGameAreaYCoord + colliderSize;
            GameAreaHeight = MaxGameAreaYCoord - MinGameAreaYCoord;
            GameAreaHalfWidth = FindObjectOfType<Camera>().aspect * MaxGameAreaYCoord;
            NormalLevel = MinGameAreaYCoord + (MaxGameAreaYCoord - MinGameAreaYCoord) * NORMAL_LEVEL_RATIO;
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

        private void InitializePositionsOfGameObjects()
        {
            transform.position = new Vector3(-GameAreaHalfWidth * 0.5f, NormalLevel);
            Bolus.transform.position = new Vector3(-GameAreaHalfWidth * 0.5f, Bolus.transform.position.y);
            Sugar.transform.position = new Vector3(-GameAreaHalfWidth * 0.5f, Sugar.transform.position.y);
        }



        public virtual void ChangeBasal(float newBasal)
        {
            BasalRate = newBasal;
            ComputeSpeedOfFly();
            Animator.speed = SpeedOfFly;
        }




        protected abstract void ResetBolusEffect();
        protected abstract void ResetCarbEffect();




        private void ComputeSpeedOfFly()
        {
            SpeedOfFly = GameParameters.MIN_SPEED_OF_FLY + (GameParameters.MAX_SPEED_OF_FLY - GameParameters.MIN_SPEED_OF_FLY) * BasalRate / (GameParameters.MAX_BASAL_RATE - GameParameters.MIN_BASAL_RATE);
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
            else if (BGValue >= GameParameters.HYPERGLYCEMIA)
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

        public virtual void SetMMolLValue(double val)
        {
            if (val <= 0) IsPatientDead = true;
            FuturePosTime = 0.0f;
            LastPosY = FuturePosY;
            BGValue = (float)val;

            //Debug.Log(val);

            var PosOfBGPt = CurveBG.GetYValue(TimeCtrl.GameTime);
            FuturePosY = PosOfBGPt - (float)(val - ScoreManager.TargetMmolL) * GameAreaCoef;  // note the coordinates are inverted
            //ScoreManager.IncreaseScore(val);
        }

        public void SetIOBValue(double val)
        {
            //TODO - maybe in the future
        }

        public void SetCOBValue(double val)
        {
            //TODO - maybe in the future
        }


        public abstract void OnSimulationTick(float gameTime);

        public void OnInterpolationTick(float deltaGameTime)
        {
            FuturePosTime += deltaGameTime;
            var ActualPosY = (LastPosY + (FuturePosTime / TimeManager.SimulationTickIntervalSecs) * (FuturePosY - LastPosY));
            ActualPosY = ((ActualPosY + 0.5f * PlayerSize.y) > MaxGameAreaYCoord) ? (MaxGameAreaYCoord - 0.5f * PlayerSize.y) : ActualPosY;
            ActualPosY = ((ActualPosY - 0.5f * PlayerSize.y) < MinGameAreaYCoord) ? (MinGameAreaYCoord + 0.5f * PlayerSize.y) : ActualPosY;
            transform.position = new Vector3(transform.position.x, ActualPosY);


            //meal sprite position - actual time is positioned before bill of bird sprite, time of meal is positioned to begin of meal sprite
            var scaleFactor = MinMealScale + (MaxMealScale - MinMealScale) * (MealCtrl.NextMealValue - MealController.MinMealValue) / (MealController.MaxMealValue - MealController.MinMealValue);

            var MealXPos = transform.position.x + 0.5f * (PlayerSize.x + MealEffectSize.x * scaleFactor) - (MealCtrl.NextMealTime - TimeCtrl.GameTime) * GameController.VisualSpeedMultiplier;
            var MealYPos = MaxGameAreaYCoord - MealEffectSize.y / 2;

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
                if (TimeCtrl.GameTime - BolusEffectStartTime > 1.0f)
                {
                    ResetBolusEffect();
                }
            }
            else
            {
                Bolus.SetActive(false);
            }


            if (SugarEffectStartTime > 0)
            {
                Sugar.transform.position = transform.position + new Vector3(0, 0.75f * PlayerSize.y + 0.5f * SugarEffectSize.y);
                Sugar.SetActive(true);
                if (TimeCtrl.GameTime - SugarEffectStartTime > 1.0f)
                {
                    ResetCarbEffect();
                }

            }
            else
            {
                Sugar.SetActive(false);
            }
            //Debug.Log(TimeCtrl.GetGameTime() + " simulation," + " meal time: " + MealCtrl.GetNextMealTime());
            //Debug.Log(gameTime + " interpolation");
        }


        

        

    }


}
