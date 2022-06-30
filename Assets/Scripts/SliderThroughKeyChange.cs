using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Class for managing of shortcuts for slider
/// </summary>
public class SliderThroughKeyChange : MonoBehaviour
{
    /// <summary>
    /// Key code of shortcut for decrement value of slider
    /// </summary>
    private KeyCode keyDec;

    /// <summary>
    /// Key code of shortcut for increment value of slider
    /// </summary>
    private KeyCode keyInc;

    /// <summary>
    /// Slider for which are shortcuts defined
    /// </summary>
    private Slider slider;

    /// <summary>
    /// Step of the change of value
    /// </summary>
    private float step;

    // Awake is called when the script instance is being loaded.
    void Awake()
    {
        step = PlayerPrefs.GetFloat("stepSlider");
        string tmpDec = PlayerPrefs.GetString("sliderKeyDec");
        string tmpInc = PlayerPrefs.GetString("sliderKeyInc");
        if (!Enum.TryParse<KeyCode>(tmpInc, out keyInc) || !Enum.TryParse<KeyCode>(tmpDec, out keyDec))
        {
            throw new ArgumentNullException();
        }
        
        slider = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(keyDec))
        {
            slider.value -= step;
            slider.onValueChanged.Invoke(slider.value);
        }

        if (Input.GetKey(keyInc))
        {
            slider.value += step;
            slider.onValueChanged.Invoke(slider.value);
        }
    }
}
