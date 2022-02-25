using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace gpredict3_gaming.Ikaros
{
    

    public class VersionDescription : MonoBehaviour
    {
        private Text VersionText;
        private string Version;

        // Start is called before the first frame update
        void Start()
        {
            VersionText = GetComponent<Text>();
            Version = Application.version;
            VersionText.text = "Version: " + Version;
        }

    }
}
