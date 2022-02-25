using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace gpredict3_gaming.Ikaros
{
    /// <summary>
    /// Class for managing of shortcuts for buttons
    /// </summary>
    public class ButtonThroughKeyClick : MonoBehaviour
    {
        /// <summary>
        /// Key code for shortcut
        /// </summary>
        private KeyCode key;

        /// <summary>
        /// Button for which is shortcut defined
        /// </summary>
        private Button button;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            string tmpKey = PlayerPrefs.GetString("shortCut");
            if (!Enum.TryParse<KeyCode>(tmpKey, out key))
            {
                throw new ArgumentNullException();
            }
            button = GetComponent<Button>();

        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        void Update()
        {
            if (Input.GetKeyDown(key))
            {
                FadeToColor(button.colors.pressedColor);
                button.onClick.Invoke();
            }
            else if (Input.GetKeyUp(key))
            {
                FadeToColor(button.colors.normalColor);
            }
        }

        /// <summary>
        /// Marking of button as active
        /// </summary>
        /// <param name="color">color for marking</param>
        void FadeToColor(Color color)
        {
            Graphic graphic = GetComponent<Graphic>();
            graphic.CrossFadeColor(color, button.colors.fadeDuration, true, true);
        }
    }
}
