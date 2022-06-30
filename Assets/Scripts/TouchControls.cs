using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace gpredict3_gaming.Ikaros
{
    public class TouchControls : MonoBehaviour
    {
        /// <summary>
        /// The controller of player
        /// </summary>
        private PlayerController Player;

        /// <summary>
        /// Prefabs for controls
        /// </summary>
        public GameObject Sugar;
        public GameObject Bolus;
        public GameObject Basal;

        ///Controls
        List<GameObject> BolusBtns = new List<GameObject>();
        List<GameObject> SugarBtns = new List<GameObject>();
        GameObject BasalObject;
        Slider BasalSlider;

        //Parent of controls
        private GameObject Touch;

        //Canvas for UI
        private Canvas Canvas;
        private CanvasScaler ScalerCanvas;
        
        //bolus and sugar values and ranges of bazal
        private static readonly int[] BolusValues = new int[] { 2, 3, 4, 5, 7 };
        private static readonly int[] SugarValues = new int[] { 5, 10, 15, 20, 30 };
        private static readonly KeyCode[] BolusKeys = new KeyCode[] { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T };
        private static readonly KeyCode[] SugarKeys = new KeyCode[] { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G };
        private static readonly float MinBasalRate = 0.0f;
        private static readonly float MaxBasalRate = 3.5f;
        private static readonly float StepBasalInPercent = 0.01f;
        private static readonly KeyCode basalInc = KeyCode.UpArrow;
        private static readonly KeyCode basalDec = KeyCode.DownArrow;

        //names for controls
        private static readonly string BolusLabel = "Bolus_";
        private static readonly string SugarLabel = "Sugar_";
        private static readonly string BasalLabel = "Basal";
        private static readonly string TouchLabel = "TouchControls";

        //Location:
        //Anchors
        private enum Align { Left, Center, Right, StretchH, Bottom, Middle, Top, StretchV };
        //private enum AlignH : int{Left = 0, Center, Right = 1, StretchH};
        //private enum AlignV : int{ Bottom = 0, Middle, Top = 1, StretchV };
        private Dictionary<Align, Vector2> Alignment = new Dictionary<Align, Vector2>();
        

        //constants for alignment
        private static readonly float Margin = 20.0f, Space = 10.0f;
        //dimensions of UI elements
        private static Vector2 DimsBtn, DimsSlider, DimsCanvas, DimsTouch, DimsCanvVsTouch, DimsArrow;



        // Start is called before the first frame update

        void Start()
        {
            Initialization();

            CreateParentGameObject();

            CreateBasalControl();

            CreateBolusControl();

            CreateSugarControl();

        }


        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// Event handling when some bolus button is pressed.
        /// </summary>
        /// <param name="value"></param>
        public void BolusButton_Press(int value)
        {
            Player.AddBolus(value);
        }

        /// <summary>
        /// Event handling when some carbs button is pressed.
        /// </summary>
        /// <param name="value"></param>
        public void SugarButton_Press(int value)
        {
            Player.AddCarb(value);
        }

        /// <summary>
        /// Event handling when the basal slider is changed.
        /// </summary>
        /// <param name="newValue"></param>
        public void BasalSlider_Changed(float newValue)
        {
            Player.ChangeBasal(newValue);
        }



        /// <summary>
        /// Create parent game object for controls
        /// </summary>
        private void CreateParentGameObject()
        {
            Touch = new GameObject();
            Touch.transform.SetParent(Canvas.transform, false);
            RectTransform uiTransform = Touch.AddComponent<RectTransform>();

            DimsTouch = new Vector2(uiTransform.rect.width, uiTransform.rect.height);
            DimsCanvVsTouch = 0.5f * DimsCanvas - 0.5f * DimsTouch;

            //set position due to reference resolution
            uiTransform.offsetMin = new Vector2((DimsCanvas.x - DimsTouch.x) / 2.0f, (DimsCanvas.y - DimsTouch.y) / 2.0f);
            uiTransform.offsetMax = new Vector2(-(DimsCanvas.x - DimsTouch.x) / 2.0f, (DimsCanvas.y + DimsTouch.y) / 2.0f);
            SetAnchorsAndPivot(Touch, Align.StretchH, Align.Bottom, Align.Center, Align.Middle);

            Touch.name = TouchLabel;
            

        }


        /// <summary>
        /// Create slider for control of basal value
        /// </summary>
        void CreateBasalControl()
        {
            BasalObject = (GameObject)Instantiate(Basal);
            BasalObject.transform.SetParent(Touch.transform, false);

            //set position due to reference resolution
            SetAnchorsAndPivot(BasalObject, Align.Right, Align.Bottom, Align.Center, Align.Middle);
            var transSlider = BasalObject.GetComponent<RectTransform>();
            float halfDiff = 0.5f * Math.Abs(DimsSlider.x - DimsSlider.y); //due to rotation of element
            transSlider.offsetMin = new Vector2((DimsCanvVsTouch.x + (halfDiff - Margin) - DimsSlider.x), -(DimsCanvVsTouch.y - Margin - halfDiff));
            transSlider.offsetMax = new Vector2((DimsCanvVsTouch.x + (halfDiff - Margin)), -(DimsCanvVsTouch.y - Margin - halfDiff - DimsSlider.y));

            BasalObject.name = BasalLabel;

            //add shortcut
            PlayerPrefs.SetFloat("stepSlider", StepBasalInPercent * (MaxBasalRate-MinBasalRate));
            PlayerPrefs.SetString("sliderKeyDec", Enum.GetName(typeof(KeyCode), basalDec));
            PlayerPrefs.SetString("sliderKeyInc", Enum.GetName(typeof(KeyCode), basalInc));
            BasalObject.AddComponent<SliderThroughKeyChange>();


            BasalSlider = BasalObject.GetComponent<Slider>();
            BasalSlider.minValue = MinBasalRate;
            BasalSlider.maxValue = MaxBasalRate;
            BasalSlider.onValueChanged.AddListener(delegate {BasalSlider_Changed(BasalSlider.value);});

            var text = BasalObject.GetComponentInChildren<Text>();
            text.text = MinBasalRate.ToString("F0") + "-" + MaxBasalRate.ToString("F1") + " U/hr";


        }

        /// <summary>
        /// Create buttons for control of bolus
        /// </summary>
        void CreateBolusControl()
        {
            for (int i = 0; i < BolusValues.Length; i++)
            {
                
                var btnGO = (GameObject)Instantiate(Bolus);
                btnGO.transform.SetParent(Touch.transform, false);

                //set position due to reference resolution
                SetAnchorsAndPivot(btnGO, Align.Left, Align.Bottom, Align.Left, Align.Bottom);
                var transBtn = btnGO.GetComponent<RectTransform>();
                transBtn.offsetMin = new Vector2(-(DimsCanvVsTouch.x - Margin - i * (Space + DimsBtn.x)), -(DimsCanvVsTouch.y - Margin - Space - DimsBtn.y));
                transBtn.offsetMax = new Vector2(-(DimsCanvVsTouch.x - Margin - i * (Space + DimsBtn.x) - DimsBtn.x), -(DimsCanvVsTouch.y - Margin - Space - 2 * DimsBtn.y));

                btnGO.name = BolusLabel + BolusValues[i].ToString();
                var text = btnGO.GetComponentInChildren<Text>();
                text.text = BolusValues[i].ToString() + "U";

                //add shortcut
                PlayerPrefs.SetString("shortCut", Enum.GetName(typeof(KeyCode), BolusKeys[i]));
                btnGO.AddComponent<ButtonThroughKeyClick>();


                var btn = btnGO.GetComponent<Button>();
                int btnI = i;
                btn.onClick.AddListener(delegate { BolusButton_Press(BolusValues[btnI]); });
                

                BolusBtns.Add(btnGO);
            }

        }



        /// <summary>
        /// Create buttons for control of carbs
        /// </summary>
        void CreateSugarControl()
        {
            for (int i = 0; i < SugarValues.Length; i++)
            {
                var btnGO = (GameObject)Instantiate(Sugar);
                btnGO.transform.SetParent(Touch.transform, false);

                //set position due to reference resolution
                SetAnchorsAndPivot(btnGO, Align.Left, Align.Bottom, Align.Left, Align.Bottom);
                var transBtn = btnGO.GetComponent<RectTransform>();
                transBtn.offsetMin = new Vector2(-(DimsCanvVsTouch.x - Margin - i * (Space + DimsBtn.x)), -(DimsCanvVsTouch.y - Margin));
                transBtn.offsetMax = new Vector2(-(DimsCanvVsTouch.x - Margin - i * (Space + DimsBtn.x) - DimsBtn.x), -(DimsCanvVsTouch.y - Margin - DimsBtn.y));

                btnGO.name = SugarLabel + SugarValues[i].ToString();
                var text = btnGO.GetComponentInChildren<Text>();
                text.text = SugarValues[i].ToString() + "g";

                //add shortcut
                PlayerPrefs.SetString("shortCut", Enum.GetName(typeof(KeyCode), SugarKeys[i]));
                btnGO.AddComponent<ButtonThroughKeyClick>();

                var btn = btnGO.GetComponent<Button>();
                int btnI = i;
                btn.onClick.AddListener(delegate { SugarButton_Press(SugarValues[btnI]); });

                SugarBtns.Add(btnGO);
            }

        }


        /// <summary>
        /// Set anchors of game object.
        /// </summary>
        /// <param name="go">Given game object</param>
        /// <param name="anchorH">horizontal alignment</param>
        /// <param name="anchorV">vertical alignment</param>
        /// <param name="pivotH">horizonal position of pivot</param>
        /// <param name="pivotV">vertical positon of pivot</param>
        private void SetAnchorsAndPivot(GameObject go, Align anchorH, Align anchorV, Align pivotH, Align pivotV)
        {
            Vector2 min = new Vector2(0, 0);
            Vector2 max = new Vector2(0, 0);
            Vector2 pivot = new Vector2(0, 0);

            //determine x-coords for anchors 
            if (anchorH.Equals(Align.StretchH))
            {
                Alignment.TryGetValue(Align.Left, out var tmpMin);
                Alignment.TryGetValue(Align.Right, out var tmpMax);
                min = min + tmpMin;
                max = max + tmpMax;
            }
            else
            {
                Alignment.TryGetValue(anchorH, out var tmpH);
                min = min + tmpH;
                max = max + tmpH;
            }

            //determine y-coords for anchors
            if (anchorV.Equals(Align.StretchV))
            {
                Alignment.TryGetValue(Align.Bottom, out var tmpMin);
                Alignment.TryGetValue(Align.Top, out var tmpMax);
                min = min + tmpMin;
                max = max + tmpMax;
            }
            else
            {
                Alignment.TryGetValue(anchorV, out var tmpV);
                min = min + tmpV;
                max = max + tmpV;
            }

            Alignment.TryGetValue(pivotH, out var pivotHVect);
            Alignment.TryGetValue(pivotV, out var pivotVVect);
            pivot = pivot + pivotHVect + pivotVVect;


            RectTransform trans = go.GetComponent<RectTransform>();
            //set anchors
            trans.anchorMin = min;
            trans.anchorMax = max;
            //set pivot
            trans.pivot = pivot;


            

        }

        /// <summary>
        /// Filling of dictionary for alignment
        /// </summary>
        void FillAlignDictionary()
        {
            //vertical alignment
            Alignment.Add(Align.Bottom, new Vector2(0, 0));
            Alignment.Add(Align.Top, new Vector2(0, 1));
            Alignment.Add(Align.Middle, new Vector2(0, 0.5f));

            //horizontal alignment
            Alignment.Add(Align.Left, new Vector2(0, 0));
            Alignment.Add(Align.Right, new Vector2(1, 0));
            Alignment.Add(Align.Center, new Vector2(0.5f, 0));
        }

        /// <summary>
        /// Initialization
        /// </summary>
        private void Initialization()
        {
            Player = FindObjectOfType<PlayerController>();

            FillAlignDictionary();

            Canvas = GetComponent<Canvas>();
            ScalerCanvas = Canvas.GetComponent<CanvasScaler>();
            ScalerCanvas.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            ScalerCanvas.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            ScalerCanvas.matchWidthOrHeight = 0.5f;
            DimsCanvas = ScalerCanvas.referenceResolution;
            
            

            DimsBtn = new Vector2(Bolus.GetComponent<RectTransform>().rect.width, Bolus.GetComponent<RectTransform>().rect.height);
            DimsSlider = new Vector2(Basal.GetComponent<RectTransform>().rect.width, Basal.GetComponent<RectTransform>().rect.height);
        }
    }
}
