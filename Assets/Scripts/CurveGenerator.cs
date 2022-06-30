using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace gpredict3_gaming.Ikaros
{
    /// <summary>
    /// Class for curve generation. This curve represents the ideal BG. 
    /// </summary>
    public class CurveGenerator : MonoBehaviour
    {
        /// <summary>
        /// Time Controller of whole game
        /// </summary>
        private TimeManager TimeCtrl;

        /// <summary>
        /// The controller of player
        /// </summary>
        private PlayerController PlayerCtrl;

        /// <summary>
        /// Line renderer for curve which represents ideal BG
        /// </summary>
        private LineRenderer BGCurveRenderer;

        /// <summary>
        /// Positions of BG in all simulation ticks 
        /// </summary>
        private Vector3[] BGCurveInSimTicks;

        /// <summary>
        /// Position of BG on the start
        /// </summary>
        private Vector3 StartPosition;


        // Parameters of the generated curve
        private float CurveHeight;
        private float CurveWavy;
        private float CurveContour; 


        /// <summary>
        /// The step of simulation
        /// </summary>
        private float SimulationStep;

        /// <summary>
        /// Game object for the finish board
        /// </summary>
        public GameObject Finish;

        /// <summary>
        /// The position of finish board
        /// </summary>
        private Vector3 FinishStartPosition;

        public bool isPlayback;

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        void Start()
        {
            InitializeComponent();
            SimulationStep = TimeCtrl.GetSimulationTickIntervalSecs()/2;
            //Difficulty = PlayerPrefs.GetFloat("difficulty"); // this line of code was used when the curve affected the difficulty of the game

            GeneratePtsPerlinInSimTicks(TimeCtrl.GetMaxGameTime() + 1, SimulationStep);
            GenerateBGCurve(SimulationStep, TimeCtrl.GetInterpolationTickIntervalSecs(), -GameController.VisualSpeedMultiplier);

            DetermineStartPositionOfCurve();
            PlacementFinishBanner();
        }

        /// <summary>
        /// Setting parameters of the generated curve
        /// </summary>
        private void setParameters()
        {
            if (!isPlayback)
            {
                var rnd = new System.Random();
                CurveHeight = 0.4f; //Note: In the original version, the parameter CurveHeight was decribed the difficulty of the game(0.2f - easy; 0.4f - medium; 0.6f - hard), in the future maybe rand
                CurveWavy = 1.0f; //How much the curve should be wavy, in the future maybe random
                CurveContour = (float)rnd.NextDouble(); // Contour in second dimension - work as random seed - TODO: more range
                PlayerPrefs.SetFloat("CurveHeight", CurveHeight);
                PlayerPrefs.SetFloat("CurveWavy", CurveWavy);
                PlayerPrefs.SetFloat("CurveContour", CurveContour);

            }
            else
            {
                CurveHeight = PlayerPrefs.GetFloat("CurveHeight");
                CurveWavy = PlayerPrefs.GetFloat("CurveWavy");
                CurveContour = PlayerPrefs.GetFloat("CurveContour");
            }
        }

        /// <summary>
        /// Update is called once per frame
        /// The curve is shifted so that the "point" for the current game time is in the center of the screen in the horizontal direction.
        /// </summary>
        private void Update()
        {
            transform.position = new Vector3(StartPosition.x + TimeCtrl.GetGameTime() * GameController.VisualSpeedMultiplier, StartPosition.y, StartPosition.z);

            Finish.transform.position = new Vector3(FinishStartPosition.x + TimeCtrl.GetGameTime() * GameController.VisualSpeedMultiplier, FinishStartPosition.y, FinishStartPosition.z);
        }

        /// <summary>
        /// Initialization
        /// </summary>
        void InitializeComponent()
        {
            TimeCtrl = FindObjectOfType<TimeManager>();
            PlayerCtrl = FindObjectOfType<PlayerController>();
            BGCurveRenderer = GetComponent<LineRenderer>();
            setParameters();
        }

        /// <summary>
        /// Determination of start position
        /// </summary>
        void DetermineStartPositionOfCurve()
        {
            StartPosition = new Vector3(-PlayerCtrl.GetGameAreaHalfWidth() * 0.5f, 0f, 0f);
            transform.position = StartPosition;

        }

        /// <summary>
        /// Positioning and placement the finish board
        /// </summary>
        void PlacementFinishBanner()
        {
            var finishRenderer = Finish.GetComponent<SpriteRenderer>();
            var finishSize = new Vector2(finishRenderer.bounds.size.x, finishRenderer.bounds.size.y);
            FinishStartPosition = new Vector3(-PlayerCtrl.GetGameAreaHalfWidth() * 0.5f - (TimeCtrl.GetMaxGameTime() + 1) * GameController.VisualSpeedMultiplier,
                BGCurveInSimTicks[BGCurveInSimTicks.Length - 1].y + finishSize.y * 0.5f);
            Finish.transform.position = FinishStartPosition;

        }

        /// <summary>
        /// Generate the points in simulation ticks using perlin noise
        /// </summary>
        /// <param name="duration">Max game time</param>
        /// <param name="simStep">Simulation step</param>
        //void GeneratePtsPerlinInSimTicks(float duration, float simStep, float wavy, float contour)
        void GeneratePtsPerlinInSimTicks(float duration, float simStep)
        {
            int noPtsSim = (int)Mathf.Round((duration) / simStep) + 1;
            BGCurveInSimTicks = new Vector3[noPtsSim];

            for (int i = 0; i < noPtsSim; i++)
            {
                var simTime = i * simStep;
                var posX = simTime * CurveWavy;
                float posY = PlayerCtrl.GetNormalLevel() + CurveHeight * PlayerCtrl.GetGameAreaHeight() * (Mathf.PerlinNoise(posX, CurveContour)-0.5f);
                BGCurveInSimTicks[i] = new Vector3(simTime, posY);
            }
        }

        /// <summary>
        /// Generate the curve of ideal BG
        /// </summary>
        /// <param name="simStep">simulation interval</param>
        /// <param name="interpStep">interpolation interval</param>
        /// <param name="speed">Speed of visualization</param>
        void GenerateBGCurve(float simStep, float interpStep, float speed)
        {
            List<Vector3> ptsList = new List<Vector3>();
            for (int i = 0; i < BGCurveInSimTicks.Length - 1; i++)
            {
                var interpTime = 0.0f;
                var ptsS = BGCurveInSimTicks[i];
                var ptsE = BGCurveInSimTicks[i + 1];
                while (interpTime < simStep)
                {
                    var posX = interpTime / simStep;
                    var posY = Mathf.SmoothStep(ptsS.y, ptsE.y, posX);
                    var newVector = new Vector3((ptsS.x + interpTime) * speed, posY, ptsS.z);
                    ptsList.Add(newVector);
                    interpTime += interpStep;

                }
            }
            var lastSimPts = BGCurveInSimTicks[BGCurveInSimTicks.Length - 1];
            ptsList.Add(new Vector3(speed * lastSimPts.x, lastSimPts.y, lastSimPts.z));

            BGCurveRenderer.positionCount = ptsList.Count;
            BGCurveRenderer.SetPositions(ptsList.ToArray());

        }


        /// <summary>
        /// Getter for the game value of ideal BG in the given time
        /// </summary>
        /// <param name="time">given simulation time</param>
        /// <returns>the game value of ideal BG</returns>
        public float GetYValue(float time)
        {
            int lowIndex = Mathf.FloorToInt(time / SimulationStep);

            var ptsS = BGCurveInSimTicks[lowIndex];
            var ptsE = BGCurveInSimTicks[lowIndex + 1];

            var posX = (time - SimulationStep * lowIndex) / SimulationStep;
            var posY = Mathf.SmoothStep(ptsS.y, ptsE.y, posX);
            return posY;
        }

    }
}
