using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace gpredict3_gaming.Ikaros
{
    
    [System.Serializable]
    public class Version
    {
        // current version of the game
        public string current;
        // minimum required version of the game
        public string minimal;
    }

    /// <summary>
    /// This class checks if a sufficient version of the game is installed.
    /// </summary>
    public class VersionController : MonoBehaviour
    {
        public GameObject Message;
        private void Awake()
        {
            StartCoroutine(CheckMinimalVersion());
           
        }

        /// <summary>
        /// Checks if the version of the game is valid. If the version is out-of-date, the message with the link on the current version is shown.
        /// </summary>
        /// <returns></returns>
        IEnumerator CheckMinimalVersion()
        {
            UnityWebRequest request = UnityWebRequest.Get(GameParameters.LOG_URL_BASE + GameParameters.VERSION_LOCATION);
            request.certificateHandler = new BypassCertificate();
            yield return request.SendWebRequest();

            if (request.responseCode == GameParameters.RESPONSE_OK)
            {
                Version version = JsonUtility.FromJson<Version>(request.downloadHandler.text);
                if(IsNeededUpdate(Application.version, version.minimal))
                {
                    Message.SetActive(true);
                }
            }
            else
            {
                Debug.Log("It is not possible to determine required version of game.");
            }

        }

        /// <summary>
        /// Conversion of version string (in the form yyyy-MM-dd-HH-mm) to DateTime
        /// </summary>
        /// <param name="version">version to parse</param>
        /// <returns>DateTime for version</returns>
        DateTime ParseVersionToDateTime(string version)
        {
            string[] parseVersion = version.Trim().Split('-');
            DateTime dateTime =new DateTime();

            if (parseVersion.Length == 5)
            {
                dateTime = new DateTime(Int32.Parse(parseVersion[0]), Int32.Parse(parseVersion[1]), Int32.Parse(parseVersion[2]), 
                    Int32.Parse(parseVersion[3]), Int32.Parse(parseVersion[4]), 0);
            }
            else
            {
                Debug.Log("The wrong format of version");
            } 

            return dateTime;
        }

        /// <summary>
        /// Is the update needed?
        /// </summary>
        /// <param name="usedVersion">used version of game</param>
        /// <param name="requiredVersion">required version of game</param>
        /// <returns>true if the update is needed, false otherwise</returns>
        bool IsNeededUpdate(string usedVersion, string requiredVersion)
        {
            DateTime myVersion = ParseVersionToDateTime(usedVersion);
            DateTime minimalVersion = ParseVersionToDateTime(requiredVersion);

            return (myVersion < minimalVersion);

        }


        /// <summary>
        /// Download new version of game
        /// </summary>
        public void Download()
        {
            Application.OpenURL(GameParameters.DOWNLOAD_URL);

        }



    }
}
