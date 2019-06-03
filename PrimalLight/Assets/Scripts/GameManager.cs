using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
	//Singleton
	static GameManager current;

	//Player
	GameObject player;
	GameObject sage;
	GameObject friend;
	GameObject artifactActivator;
	Vector3 initialPosition;
	Quaternion initialRotation;

	//Handle player dying
	List<DeathObserver> deathObservers = new List<DeathObserver>();
	GameObject[] checkpoints;
	public float restartTime = 6f;

	//Artifact
	public const int NUMBER_PIECES = 3;
	private int rewards = 0;

	void Awake()
	{
		//If a Game Manager exists and this isn't it...
		if (current != null && current != this)
		{
			//...destroy this and exit. There can only be one Game Manager
			Destroy(gameObject);
			return;
		}

		//Set this as the current game manager
		current = this;

		//Persist this object between scene reloads
		DontDestroyOnLoad(gameObject);
	}

	void Start() {
		Cursor.lockState = CursorLockMode.Locked;
	}

	void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
		//Find Player
		current.player = GameObject.FindGameObjectWithTag("Player");
		current.initialPosition = current.player.transform.position;
		current.initialRotation = current.player.transform.localRotation;

		//Find Checkpoints
		current.checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint");

		//Find Sage
		current.sage = GameObject.FindGameObjectWithTag("Sage");

		//Find Friend
		current.friend = GameObject.FindGameObjectWithTag("Friend");

		//Find artifact activator
		current.artifactActivator = GameObject.FindGameObjectWithTag("ArtifactActivator");
    }

	public static GameObject GetPlayer() {
		return current.player;
	}

	public static GameObject GetSage() {
		return current.sage;
	}

	public static GameObject GetFriend() {
		return current.friend;
	}

	public static GameObject GetArtifactActivator() {
		return current.artifactActivator;
	}

	public static int GetNumberPieces() {
		return current.rewards;
	}

	public static void PlayerDied() {
		foreach (DeathObserver obs in current.deathObservers)
			obs.OnPlayerDeath();

		//Restart
		current.StartCoroutine(current.RestartPlayer());
	}

	private IEnumerator RestartPlayer() {
        yield return new WaitForSeconds(current.restartTime);
		
		MovePlayerToClosestCheckpoint();

		//Notify observers that the player is alive
		foreach (DeathObserver obs in current.deathObservers)
			obs.OnPlayerAlive();
    }

	public void MovePlayerToClosestCheckpoint()
	{
		//If no checkpoints in the scene, move to initial position
		Vector3 closestCheckpoint = current.initialPosition;
		float currentDistance = Vector3.Distance(closestCheckpoint, current.player.transform.position);
		
		foreach(GameObject checkpoint in current.checkpoints) {
			float checkpointDistance = Vector3.Distance(checkpoint.transform.position, current.player.transform.position);
			if(checkpointDistance < currentDistance) {
				closestCheckpoint = checkpoint.transform.position;
				currentDistance = checkpointDistance;
			}
		}

		current.player.transform.position = closestCheckpoint;
		current.player.transform.localRotation = current.initialRotation;
	} 

	public static void RegisterDeathObserver(DeathObserver obs) {
		current.deathObservers.Add(obs);
	}

	public static void OnRewardObtained() {
		current.rewards++;
	}
}