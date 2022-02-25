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

        /// <summary>
        /// Difficulty of the game (0.2f - easy; 0.4f - medium; 0.6f - hard)
        /// </summary>
        private float CurveHeight = 0.4f;
        private float Wavy = 1.0f;

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

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        void Start()
        {
            InitializeComponent();
            SimulationStep = TimeCtrl.GetSimulationTickIntervalSecs()/2;
            //Difficulty = PlayerPrefs.GetFloat("difficulty");

            //random generator for contour - TODO: more range
            var rnd = new System.Random();
            float contour = (float) rnd.NextDouble();
            
            GeneratePtsPerlinInSimTicks(TimeCtrl.GetMaxGameTime() + 1, SimulationStep, Wavy, contour);
            GenerateBGCurve(SimulationStep, TimeCtrl.GetInterpolationTickIntervalSecs(), -GameController.VisualSpeedMultiplier);

            DetermineStartPositionOfCurve();
            PlacementFinishBanner();
        }

        // Update is called once per frame
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
        /// <param name="wavy">How much the curve should be wavy</param>
        /// <param name="contour">Contour in second dimension - work as random seed</param>
        void GeneratePtsPerlinInSimTicks(float duration, float simStep, float wavy, float contour)
        {
            int noPtsSim = (int)Mathf.Round((duration) / simStep) + 1;
            BGCurveInSimTicks = new Vector3[noPtsSim];

            for (int i = 0; i < noPtsSim; i++)
            {
                var simTime = i * simStep;
                var posX = simTime * wavy;
                float posY = PlayerCtrl.GetNormalLevel() + CurveHeight * PlayerCtrl.GetGameAreaHeight() * (Mathf.PerlinNoise(posX, contour)-0.5f);
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
