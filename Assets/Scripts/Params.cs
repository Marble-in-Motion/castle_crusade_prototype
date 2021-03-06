﻿/**
 * Game parameter class
 **/
public class Params
{

    public static float STARTING_TOWER_HEALTH = 100f;
    public static float MIN_TIME_BETWEEN_SHOTS = 0.1f;
    public static int STARTING_COIN = 100;
    public static float COIN_DELAY = 1f;
    public static float COIN_INCREASE_INTERVAL = 15f;
    public static int STARTING_COIN_INCREASE_AMOUNT = 5;
    public static int COIN_BOOST = 1;
    public static float DESTROY_COOL_DOWN = 30f;
    public static int DESTROY_COST = 100;
    public static int[] NPC_COST = { 10 };
    public static float[] NPC_HEALTH = { 200f };
    public static float[] NPC_SPEED = { 12f };
    public static int[] NPC_REWARD = { 5 };
    public static int[] NPC_DAMAGE = { 10 };

    //Input Params
    public static string SHOOT_KEY = "space";
    public static string LEFT_KEY = "left";
    public static string RIGHT_KEY = "right";
    public static string VOLLEY_KEY = "s";
    public static string VOLLEY_KEY_ALT = "v";
    public static string SK_KEY = "d";
    public static string SK_KEY_ALT = "return";
    public static string START_RECORDING_KEY = "1";
    public static string STOP_RECORDING_KEY = "2";
    public static string PLAYBACK_KEY = "3";
    public static string TEST_KEY = "4";

    //Tests
    public static string[] TESTS = { "Aim_Test", "Shoot_Test", "Spawn_Test", "Send_Shoot_Test", "Volley_Test", "End_Game_Test" };

    //AI Params
    public static int TROOP_COUNT_PER_DANGER_INDEX = 4;
    public static float TROOP_CLOSE_DISTANCE = 0.55f;
    public static int TROOP_RATIO_MULTIPLYER = 12;
    public static int[] TIME_BETWEEN_TROOP_SEND = { 5, 10 };
    public static int COINS_DIVISOR_FOR_TROOPS_UPPER_BOUND = 40;
    public static float TROOP_SEND_DELAY_PER_TROOP = 0.5f;
    public static string MULTI_IMAGE_INFERENCE_SCRIPT_PATH = @"C:\Users\SP\Documents\WORK\GP\tensorflow\tensorflow-for-poets-2\scripts\label_image_spesh.py";
    public static string SINGLE_IMAGE_INFERENCE_SCRIPT_PATH = @"C:\Users\SP\Documents\WORK\GP\tensorflow\tensorflow-for-poets-2\scripts\label_image_single.py";
    public static string TEST_SCRIPT_PATH = @"C:\Users\SP\Documents\WORK\GP\castle_crusade_prototype\Assets\Scripts\Python\test.py";
    public static float TRAIN_SCREENSHOT_DELAY = 2;
    public static int MIN_DANGER_SCORE_SCREENSHOT = 0;

    //OLD AI
    public static float TIME_PER_SHOT = 0.2f;
    public static float MAX_TIME_AT_SCREEN = 5;
    public static float CHANGE_DIRECTION_TIME = 0.3f;

    //NEURAL
    public static float TIME_PER_SHOT_NEURAL = 0.1f;
    public static float MAX_TIME_AT_SCREEN_NEURAL = 2.5f;
    public static float CHANGE_DIRECTION_TIME_NEURAL = 0.15f;

    internal static float SEND_TROOP_ALERT_DELAY = 10f;

    //Audio Params
    public static string AMBIENCE = "ambience";
    public static string KLAXON = "klaxon";
    public static string COINS = "coins";
    public static string HORN = "horn";
    public static string VOLLEY = "volley";
    public static string GONG = "gong";
    public static string SWORD = "sword";
    public static string MAIN_MUSIC = "mainmusic";
    public static string MORE_TROOPS = "moretroops";
    public static string LOBBY_MUSIC = "lobbymusic";

    public static int PLAY_RANDOM = 808;

    //Troop Params
    public static int KING_TROOP_ID = 0;
    public static int RAM_TROOP_ID = 1;

    public static float TROOP_SEND_INTERVAL_SAND_BOX = 1;

    public class Bolt
    {
        public static float DAMAGE = 100f;
        public static float RANGE = 400f;
    }

}
