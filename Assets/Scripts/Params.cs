﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Params {

	public static float STARTING_TOWER_HEALTH = 100f;
	public static int STARTING_COIN = 100;
	public static float COIN_DELAY = 1f;
	public static float COIN_INCREASE_INTERVAL = 15f;
	public static int STARTING_COIN_INCREASE_AMOUNT = 5;
	public static int COIN_BOOST = 1;
	public static float DESTROY_COOL_DOWN = 30f;
	public static int DESTROY_COST = 100;
	public static int[] NPC_COST = {10,400,40};
	public static float[] NPC_HEALTH = {200f,3000f,800f};
	public static float[] NPC_SPEED = {12f,4f,12f};
	public static int[] NPC_REWARD = {5,20,20};
    public static int[] NPC_DAMAGE = { 10, 30, 10 };

    //AI Params
    public static int TROOP_COUNT_PER_DANGER_INDEX = 8;
    public static float TROOP_CLOSE_DISTANCE = 0.55f;
    public static int TROOP_RATIO_MULTIPLYER = 5;
    public static float CHANGE_DIRECTION_TIME = 0.3f;
    public static float TIME_PER_SHOT = 0.2f;
    public static int[] TIME_BETWEEN_TROOP_SEND = { 5, 12 };
    public static int COINS_DIVISOR_FOR_TROOPS_UPPER_BOUND = 40;

    public class Bolt
	{
		public static float DAMAGE = 100f;
		public static float RANGE = 400f;
	}

}
