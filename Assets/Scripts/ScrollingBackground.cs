using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace gpredict3_gaming.Ikaros
{
    public class ScrollingBackground : MonoBehaviour
    {
        private TimeManager TimeCtrl;
        private Vector2 StartPos;
        private BoxCollider2D BackgroundCollider;
        //A float to store the x-axis length of the background
        private float BackgroundHorizontalLenght;

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        void Start()
        {
            TimeCtrl = FindObjectOfType<TimeManager>();
            BackgroundCollider = FindObjectOfType<BoxCollider2D>();
            BackgroundHorizontalLenght = BackgroundCollider.size.x;
            StartPos = transform.position;
            
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        void Update()
        {
            //Repeating and scrolling background
            float newPos = Mathf.Repeat(TimeCtrl.GameTime * GameController.VisualSpeedMultiplier, BackgroundHorizontalLenght);
            transform.position = StartPos + Vector2.right * (newPos - BackgroundHorizontalLenght);

        }
    }
}
