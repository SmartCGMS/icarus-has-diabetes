﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Globalization;

namespace gpredict3_gaming.Ikaros

    
{
    public enum TypeGame
    {
        FULL_GAME = 0,
        DEMO_GAME = 1,
        PLAYBACK = 2
    }

    public enum TypeReplay
    {
        PLAYER = 0,
        AI = 1,
        BOTH = 2

    }

    public class GameParameters
    {
        public static readonly float GAME_TIME = 120.0f;
        public static readonly float DEMO_GAME_TIME = 20.0f;

        public static readonly float DIFFICULTY_EASY = 0.2f;
        public static readonly float DIFFICULTY_MEDIUM = 0.4f;
        public static readonly float DIFFICULTY_HARD = 0.6f;

        public static readonly float MAX_BASAL_RATE = 5.0f;
        public static readonly float MIN_BASAL_RATE = 0.0f;

        public static readonly float MAX_SPEED_OF_FLY = 0.2f;
        public static readonly float MIN_SPEED_OF_FLY = 0.05f;

        public static readonly float HYPERGLYCEMIA = 11.1f;
        public static readonly float HYPOGLYCEMIA = 3.9f;
        public static readonly float RANGE_COLOR_GRADIENT = 3;
        public static readonly float MIN_COLOR_ALPHA = 0.0f;
        public static readonly float MAX_COLOR_ALPHA_HYPER = 1.0f/3.0f;
        public static readonly float MAX_COLOR_ALPHA_HYPO = 1.0f / 2.0f;

        public static readonly string GAME_WORKING_DIRECTORY = Directory.GetCurrentDirectory();
        public static readonly string DOWNLOAD_URL = "https://diabetes.zcu.cz/icarus";
        public static readonly string LOG_URL_BASE = "https://diabetes.zcu.cz/api/";
        public static readonly string UPLOAD_META_LOCATION = "icarus-upload-meta";
        public static readonly string UPLOAD_DATA_LOCATION = "icarus-upload-file/";
        public static readonly string GLOBAL_SCOREBOARD_LOCATION = "icarus-scoreboard";
        public static readonly string VERSION_LOCATION = "icarus-version";
        public static readonly long RESPONSE_OK = 200L;

        public static readonly NumberFormatInfo nfi = CultureInfo.GetCultureInfo("en-GB").NumberFormat;

        public static readonly string DIRECTORY = Directory.GetParent(Application.dataPath).FullName; //Directory.GetParent(Application.persistentDataPath).FullName;
        public static readonly string LOGFILE_PREFIX = "log";
        public static readonly string LOGFILE_EXT = ".txt";

        public static readonly string LOCAL_TYPE_SCORE = "local";
        public static readonly string GLOBAL_TYPE_SCORE = "global";

        public static readonly string MAIN_MENU_SCENE = "TitleMenu";
        public static readonly string PROFILE_SCENE = "Profile";
        public static readonly string CONTROLS_SCENE = "Controls";
        public static readonly string SCOREBOARD_SCENE = "HighScore";
        public static readonly string GAME_SCENE = "GameScene";
        public static readonly string GAME_OVER_SCENE = "GameOverMenu";
        public static readonly string PLAYBACK_MENU = "PlaybackMenu";
        public static readonly string LOADING_SCENE = "Loading";
        public static readonly string PLAYBACK_SCENE = "PlaybackScene";

        //Parameters for playback
        public static readonly string OWN_GAME = "Option1";
        public static readonly string OPTIMIZED_GAME = "Option2";
        public static readonly string BOTH_GAME = "Option3";
        public static readonly string OPT_LOGFILE = "optimalGame";
        public static readonly UInt16 DEGREE_OF_OPT = 10;




    }
}

