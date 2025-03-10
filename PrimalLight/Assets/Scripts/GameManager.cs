using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

[DefaultExecutionOrder(-200)]
public class GameManager : MonoBehaviour
{
	//Singleton
	public static GameManager current;

	//Player
	GameObject player;
	GameObject sage;
	GameObject friend;
	GameObject artifact;
	GameObject artifactActivator;
	Vector3 initialPosition;
	Quaternion initialRotation;

	//Handle player dying
	List<DeathObserver> deathObservers = new List<DeathObserver>();
	GameObject[] checkpoints;
	GameObject currCheckpoint = null;

	public float restartTime = 6f;

	//Artifact
	public const int NUMBER_PIECES = 3;
	private HashSet<Reward> rewards = new HashSet<Reward>();

	//Scenes
	public static int MAIN_SCENE_INDEX = 3;
	public static int TEMPLE_SCENE_INDEX = 2;
	bool sequentialCheckpoints = false;

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

	void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
		if(scene.buildIndex == MenuManager.MENU_SCENE_INDEX)
			return;

		if(scene.buildIndex == TEMPLE_SCENE_INDEX)
			sequentialCheckpoints = true;
		else sequentialCheckpoints = false;

		//Remove all observers
		deathObservers.Clear();

		//Find Player
		current.player = GameObject.FindGameObjectWithTag("Player");
		current.initialPosition = current.player.transform.position;
		current.initialRotation = current.player.transform.localRotation;
		current.player.GetComponent<PlayerSound>().SetHardSurface(scene.buildIndex != MAIN_SCENE_INDEX);

		//Find Checkpoints
		current.checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint");

		//Find Sage
		current.sage = GameObject.FindGameObjectWithTag("Sage");

		//Find Friend
		current.friend = GameObject.FindGameObjectWithTag("Friend");

		//Find artifact activator
		current.artifactActivator = GameObject.FindGameObjectWithTag("ArtifactActivator");

		//Find Artifact
		current.artifact = GameObject.FindGameObjectWithTag("Artifact");
		current.artifact.SetActive(false);
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

	public static GameObject GetArtifact() {
		return current.artifact;
	}

	public static GameObject GetArtifactActivator() {
		return current.artifactActivator;
	}

	public static int GetNumberPieces() {
		return current.rewards.Count;
	}

	public static void PlayerDied() {
		foreach (DeathObserver obs in current.deathObservers)
			obs.OnPlayerDeath();

		//Restart
		current.StartCoroutine(current.RestartPlayer());
	}

	private IEnumerator RestartPlayer() {
        yield return new WaitForSeconds(current.restartTime);
		
		if(!sequentialCheckpoints)
			MovePlayerToClosestCheckpoint();
		else
			MovePlayerToCurrentCheckpoint();

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

		SetPlayerPosition(closestCheckpoint);
	}

	private void MovePlayerToCurrentCheckpoint(){
		if(currCheckpoint == null)
			SetPlayerPosition(current.initialPosition);
		else
			SetPlayerPosition(currCheckpoint.transform.position);
	}

	private void SetPlayerPosition(Vector3 pos){
		current.player.transform.position = pos;
		current.player.transform.localRotation = current.initialRotation;
	}

	public void SetCurrentCheckpoint(GameObject checkpoint){
		current.currCheckpoint = checkpoint;
	}

	public static void RegisterDeathObserver(DeathObserver obs) {
		current.deathObservers.Add(obs);
	}

	public static void OnRewardObtained(Reward reward) {
		current.rewards.Add(reward);
	}
}