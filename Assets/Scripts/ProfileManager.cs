using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace gpredict3_gaming.Ikaros
{
    public class ProfileManager : MonoBehaviour
    {
        public static Enumeration<string, int> Qualification { get; private set; }
        public static Enumeration<string, string> Disease { get; private set; }
        public static Enumeration<string, float> Difficulty { get; private set; }


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


        void SetDropdowns()
        {
            QualificationDD = QualificationGO.GetComponent<Dropdown>();
            DiseaseDD = DiseaseGO.GetComponent<Dropdown>();
            DifficultyDD = DifficultyGO.GetComponent<Dropdown>();

            SetOptionsToDropdown(QualificationDD, Qualification.Names.ToArray());
            SetOptionsToDropdown(DiseaseDD, Disease.Names.ToArray());
            SetOptionsToDropdown(DifficultyDD, Difficulty.Names.ToArray());



        }

        void SetOptionsToDropdown(Dropdown dropdown, string[] optionsName){
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            for(int i = 0; i < optionsName.Length; i++)
            {
                options.Add(new Dropdown.OptionData(optionsName[i]));
            }
            dropdown.options = options;
        }

        void CreateEnums()
        {
            FillQualificationEnumeration();
            FillDiseaseEnumeration();
            FillDifficultyEnumeration();
        }

        void FillQualificationEnumeration()
        {
            Qualification = new Enumeration<string, int>();
            Qualification.Add("Physician", 0);
            Qualification.Add("Diabetologist", 1);
            Qualification.Add("Diabetes Nurse", 2);
            Qualification.Add("Patient with diabetes", 3);
            Qualification.Add("No diabetic experience", 4);
        }

        void FillDiseaseEnumeration()
        {
            Disease = new Enumeration<string, string>();
            Disease.Add("None", null);
            Disease.Add("E10 - Type 1", "E10");
            Disease.Add("E11 - Type 2", "E11");
            Disease.Add("O24 - Gestational", "O24");
            Disease.Add("E13 - Other", "E13");
        }

        void FillDifficultyEnumeration()
        {
            Difficulty = new Enumeration<string, float>();
            Difficulty.Add("Easy", GameParameters.DIFFICULTY_EASY);
            Difficulty.Add("Medium", GameParameters.DIFFICULTY_MEDIUM);
            Difficulty.Add("Hard", GameParameters.DIFFICULTY_HARD);
        }
    }
}
