﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	public float levelStartDelay = 2f;									// Time to wait before starting level, in seconds
	public float turnDelay = 0.1f;										// Delay between each Player turn
	public int playerFoodPoints = 100;									// Starting value for Player food points
	public static GameManager instance = null;							// Static instance of GameManager which allows it to be accessed by any other script
	[HideInInspector] public bool playersTurn = true;					// Boolean to check if it's players turn, hidden in inspector but public

	private Text levelText;
	private GameObject levelImage;
	private BoardManager boardScript;									// Store a reference to our BoardManager which will
	private int level = 1;												// Current level number, expressed in game as "Day 1"
	private List<Enemy> enemies;										// List of all Enemy units, used to issue them move commands
	private bool enemiesMoving;											// Boolean to check if enemies are moving
	private bool doingSetup = true;										// Boolean to check if we're setting up board, prevent Player from moving during setup

	void Awake()
	{
		// Check if instance already exists
		if (instance == null)
			// If not, set instance to this
			instance = this;

		// If instance already exists and it's not this
		else if (instance != this)
			// Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameMananger
			Destroy(gameObject);

		// Sets this to not be destroyed when reloading scene
		DontDestroyOnLoad(gameObject);

		// Assign enemies to a new list of Enemy objects
		enemies = new List<Enemy>();

		// Get a component reference to the attached Board script
		boardScript = GetComponent<BoardManager>();

		// Call the InitGame funciton to initialize the first level
		InitGame();
	}

	// This is called each time a scene is loaded
	void OnLevelWasLoaded (int index)
	{
		// Add one to our levle number
		level++;
		
		// Call InitGame to initialize our level
		InitGame();
	}

	// Initializes the game for each level
	void InitGame()
	{
		// While doingSetup is true the player can't move, prevent player from moving while title card is up
		doingSetup = true;

		// Get a reference to our image LevelImage by finding it by name
		levelImage = GameObject.Find("LevelImage");

		// Get a reference to our text LevelText's text component by finding it by name and calling GetComponent
		levelText = GameObject.Find ("LevelText").GetComponent<Text>();

		// Set the text of levelText to the string "Day" and append the current level number
		levelText.text = "Day " + level;

		// Set levelImage to active blocking player's view of the game board during setup
		levelImage.SetActive(true);

		// Call the HideLevelImage function with a delay in seconds of levelStartDelay
		Invoke ("HideLevelImage", levelStartDelay);

		// Clear any Enemy objects in our List to prepare for next level
		enemies.Clear ();
		
		// Call the SetupScene of the BoardManager script, pass it current levle number
		boardScript.SetupScene(level);
	}

	// Hides black image used between levels
	private void HideLevelImage()
	{
		// Disable the levelImage gameObject
		levelImage.SetActive(false);

		// Setup doingSetup to false allowing player to move again
		doingSetup = false;
	}

	void Update()
	{
		// Check that playerTurn or enemiesMoving or doingSetup are not currently true
		if (playersTurn || enemiesMoving || doingSetup)
			// If any of these are true, return and do not start MoveEnemies
			return;
		
		// Start moving enemies
		StartCoroutine(MoveEnemies());
	}

	// Call this to add the passed in Enemy to the List of Enemy objects
	public void AddEnemyToList(Enemy script)
	{
		// Add Enemy to List enemies
		enemies.Add (script);
	}

	// GameOver is called when the player raeches 0 food points
	public void GameOver()
	{	
		// Set levelText to display number of levels passed and game over message
		levelText.text = "After " + level + " days, you starved";

		// Enable black background image gameObject
		levelImage.SetActive(true);

		// Disable this GameManager
		enabled = false;
	}

	// Coroutine to move enemies in sequence
	IEnumerator MoveEnemies()
	{
		// While enemiesMoving is true player is unable to move
		enemiesMoving = true;

		// Wait for turnDelay seconds, defaults to .1 (100 ms)
		yield return new WaitForSeconds (turnDelay);

		// If there are no enemies spawned
		if (enemies.Count == 0)
		{
			// Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none
			yield return new WaitForSeconds (turnDelay);
		}

		// Loop through List of Enemy objects
		for (int i = 0; i < enemies.Count; i++)
		{
			// Call the MoveEnemy function of Enemy at index i in the enemies List
			enemies[i].MoveEnemy();

			// Wait for Enemy's moveTime before moving next Enemy
			yield return new WaitForSeconds(enemies[i].moveTime);
		}
		// Once Enemies are done moving, st playersTurn to true so player can move
		playersTurn = true;

		// Enemies are done moving, set enemiesMoving to false
		enemiesMoving = false;
	}
}
