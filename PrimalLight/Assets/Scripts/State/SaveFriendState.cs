using UnityEngine;

public class SaveFriendState : State {

    private bool enteredTrigger;
    private GameObject player;
    private GameObject friend;
    private bool isNearFriend;
    private BoxCollider friendTrigger;
    private Material friendMaterial;
    private Animator playerAnimator;
    private float lightTimer = 0f;

    public SaveFriendState() {
        enteredTrigger = false;
        isNearFriend = false;
        player = GameManager.GetPlayer();
        friend = GameManager.GetFriend();
        friendMaterial = friend.GetComponentInChildren<SkinnedMeshRenderer>().material;
        friendTrigger = friend.GetComponentInChildren<BoxCollider>();
        friendTrigger.enabled = true;
        playerAnimator = player.GetComponent<Animator>();
    }

    public override void Update() {
        if(enteredTrigger) {
            if(isNearFriend) {
                playerAnimator.SetBool("isHelping", true);

                //Light friend color
                lightTimer += 0.1f * Time.deltaTime;
                Color newColor = Color.Lerp(Color.white * 0f, Color.white, lightTimer);
                friendMaterial.SetColor("_EmissionColor", newColor);

                if(Utils.ColorEquals(Color.white, newColor)) {
                    GameState.Next();
                    return;
                }
            } else {
                
                //Walk towards the friend
                Vector3 targetDir = friend.transform.position - player.transform.position;
                targetDir.y = 0f;
                if(targetDir.magnitude > 1.5f) {
                    float step = 50f * Time.deltaTime;
                    Vector3 newDir = Vector3.RotateTowards(player.transform.forward, targetDir, step, 0.0f);
                    GameInput.cameraEulerAngles = Quaternion.LookRotation(newDir).eulerAngles;
                    GameInput.vertical = 1f;
                    GameInput.cameraForward = newDir;
                } else {
                    GameInput.SimulateInput(false);
                    GameInput.CaptureInput(true);
                    isNearFriend = true;
                    playerAnimator.SetTrigger("startKneeling");
                }
                
            }
        }
    }

    public void OnFriendTrigger() {
        friendTrigger.enabled = false;
        enteredTrigger = true;
        GameInput.SimulateInput(true);
    }

    public override State Next() {
        if(!enteredTrigger) {
            OnFriendTrigger();
            return this;
        } else {
            playerAnimator.SetTrigger("stopKneeling");
            playerAnimator.SetBool("isHelping", false);
            GameInput.SimulateInput(false);
            GameInput.CaptureInput(false);
            return new NullState();
        }
    }

    public override string GetMessage() {
        if(!enteredTrigger) 
            return "Go back to your friend";
        else return "Use the artifact";
    }
}
