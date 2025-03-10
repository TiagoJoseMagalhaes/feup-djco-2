using UnityEngine;

public class FriendDeathState : State {

    public float stateDuration = 7f;
    private float cryingWaitTime = 2f;
    public float afterStateDelay = 2f;
    private bool crying;
    private bool stopKneeling;
    public float startTime = Time.time;
    private GameObject player;
    private GameObject friend;
    private Material friendMaterial;
    private float fadeTimer = 0f;
    private Color initialFriendColor;
    
    public FriendDeathState() {
        crying = false;
        stopKneeling = false;
        GameInput.CaptureInput(true);
        player = GameManager.GetPlayer();
        friend = GameManager.GetFriend();
        friend.GetComponent<Animator>().SetTrigger("isDead");
        friend.GetComponent<Rigidbody>().isKinematic = true;
        friend.GetComponent<FriendSound>().DeathSound();
        player.GetComponent<Animator>().SetTrigger("startKneeling");
        friendMaterial = friend.GetComponentInChildren<SkinnedMeshRenderer>().material;
        initialFriendColor = friendMaterial.GetColor("_EmissionColor");
    }

    public override void Update() {
        float elapsed = Time.time - startTime;
        if(elapsed >= cryingWaitTime && !crying) {
            player.GetComponent<Animator>().SetTrigger("startCrying");
            player.GetComponent<PlayerSound>().CrySound();
            crying = true;
        }

        if(elapsed >= stateDuration && !stopKneeling) {
            player.GetComponent<Animator>().SetTrigger("stopKneeling");
            stopKneeling = true;
        }

        if(elapsed >= stateDuration + afterStateDelay) {
            GameState.Next();
            return;
        }

        //Rotate player towards friend
        Vector3 targetDir = friend.transform.position - player.transform.position;
        targetDir.y = 0f;
        float step = 250f * Time.deltaTime;
        Vector3 newDir = Vector3.RotateTowards(player.transform.forward, targetDir, step, 0.0f);
        player.transform.rotation = Quaternion.LookRotation(newDir);

        //Fade friend color
        fadeTimer += 0.5f * Time.deltaTime;
        friendMaterial.SetColor("_EmissionColor", Color.Lerp(initialFriendColor, Color.white * 0f, fadeTimer));
    }

    public override State Next() {
        GameInput.CaptureInput(false);
        return new SageState();
    }
}
