using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace gpredict3_gaming.Ikaros
{
    public class ProfileManager : MonoBehaviour
    {
        //List of possible qualifications
        public static Enumeration<string, int> Qualification { get; private set; }
        //List of possible diseases
        public static Enumeration<string, string> Disease { get; private set; }
        //List of possible game modes
        public static Enumeration<string, float> Difficulty { get; private set; }

        //objects for the form
        public GameObject QualificationGO;
        public GameObject DiseaseGO;
        public GameObject DifficultyGO;
        public Text Nickname;

        private Dropdown QualificationDD;
        private Dropdown DiseaseDD;
        private Dropdown DifficultyDD; 


        // Start is called before the first frame update
        void Start()
        {
            CreateEnums();
            SetDropdowns();
        }

        /// <summary>
        /// Reaction on the "Start" button click.
        /// Information about the player is archived and the game is started.
        /// </summary>
        public void OnStartClick()
        {
            PlayerPrefs.SetString("nickname", Nickname.text);
            
            int qualificationChoice = QualificationDD.value;
            int diseaseChoice = DiseaseDD.value;
            int difficultyChoice = DifficultyDD.value;

            PlayerPrefs.SetInt("player_qualification", Qualification.CodesToArray()[qualificationChoice]);
            PlayerPrefs.SetString("disease_code", Disease.CodesToArray()[diseaseChoice]);
            PlayerPrefs.SetFloat("difficulty", Difficulty.CodesToArray()[difficultyChoice]);
            PlayerPrefs.SetString("difficulty_name", Difficulty.NamesToArray()[difficultyChoice]);
            PlayerPrefs.SetInt("difficulty_id", difficultyChoice);
            SceneManager.LoadScene(GameParameters.GAME_SCENE);

        }


        /// <summary>
        /// Dropdowns are created and the appropriate lists of options are assigned.
        /// </summary>
        void SetDropdowns()
        {
            QualificationDD = QualificationGO.GetComponent<Dropdown>();
            DiseaseDD = DiseaseGO.GetComponent<Dropdown>();
            DifficultyDD = DifficultyGO.GetComponent<Dropdown>();

            SetOptionsToDropdown(QualificationDD, Qualification.Names.ToArray());
            SetOptionsToDropdown(DiseaseDD, Disease.Names.ToArray());
            SetOptionsToDropdown(DifficultyDD, Difficulty.Names.ToArray());



        }

        /// <summary>
        /// The list of options is assigned to the dropdown.
        /// </summary>
        /// <param name="dropdown">dropdown</param>
        /// <param name="optionsName"> list of options</param>
        void SetOptionsToDropdown(Dropdown dropdown, string[] optionsName){
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            for(int i = 0; i < optionsName.Length; i++)
            {
                options.Add(new Dropdown.OptionData(optionsName[i]));
            }
            dropdown.options = options;
        }

        /// <summary>
        /// Creates lists of options including appropriate codes for the user's form 
        /// </summary>
        void CreateEnums()
        {
            FillQualificationEnumeration();
            FillDiseaseEnumeration();
            FillDifficultyEnumeration();
        }

        /// <summary>
        /// Filling the list of qualifications
        /// </summary>
        void FillQualificationEnumeration()
        {
            Qualification = new Enumeration<string, int>();
            Qualification.Add("Physician", 0);
            Qualification.Add("Diabetologist", 1);
            Qualification.Add("Diabetes Nurse", 2);
            Qualification.Add("Patient with diabetes", 3);
            Qualification.Add("No diabetic experience", 4);
        }

        /// <summary>
        /// Filling the list of diseases
        /// </summary>
        void FillDiseaseEnumeration()
        {
            Disease = new Enumeration<string, string>();
            Disease.Add("None", null);
            Disease.Add("E10 - Type 1", "E10");
            Disease.Add("E11 - Type 2", "E11");
            Disease.Add("O24 - Gestational", "O24");
            Disease.Add("E13 - Other", "E13");
        }

        /// <summary>
        /// Filling the list of game modes
        /// </summary>
        void FillDifficultyEnumeration()
        {
            Difficulty = new Enumeration<string, float>();
            Difficulty.Add("Easy", GameParameters.DIFFICULTY_EASY);
            Difficulty.Add("Medium", GameParameters.DIFFICULTY_MEDIUM);
            Difficulty.Add("Hard", GameParameters.DIFFICULTY_HARD);
        }
    }
}
