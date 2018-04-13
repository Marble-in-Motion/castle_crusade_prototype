using System.Collections;
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

	public class Bolt
	{
		public static float DAMAGE = 100f;
		public static float RANGE = 400f;
	}

}
