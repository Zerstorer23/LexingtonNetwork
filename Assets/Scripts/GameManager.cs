using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;

public class GameManager : MonobehaviourLexCallbacks
{

    public ConnectedPlayer connectedPlayer;


    public GameObject UIcanvas;
    public GameObject leaveScreen;
    public GameObject playerPrefab;
    public GameObject sceneCam;
    public Text pingText;



    public Text respawnText;
    public Button respawnButton;
    public GameObject respawnPanel;

    public GameObject feedText;
    public GameObject feedTextBox;


    [HideInInspector] public GameObject LocalPlayer;
    public static GameManager instance;
    float respawnDelay = 5f;
    bool startCountDown = false;

    private void Awake()
    {
        UIcanvas.SetActive(true);
        respawnButton.gameObject.SetActive(false);
        instance = this;
        LexNetwork.ConnectUsingSettings();
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        AssignTeam();
        respawnButton.gameObject.SetActive(true);
        Debug.Log("Joined room!");
    }
    private void AssignTeam()
    {
        LexHashTable hash = new LexHashTable();
        int size = LexNetwork.PlayerList.GetLength(0);
        if (size % 2 == 0)
        {
            hash.Add(PlayerProperty.Team, "0");
        }
        else
        {
            hash.Add(PlayerProperty.Team, "0");
        }
        LexNetwork.LocalPlayer.SetCustomProperties(hash);
    }
    private void Start()
    {
        connectedPlayer.AddLocalPlayer();
       // connectedPlayer.gameObject.GetComponent<PhotonView>().RPC("UpdatePlayerList", RpcTarget.OthersBuffered, PhotonNetwork.NickName);
    }
    private void Update()
    {
        pingText.text = "PING: " + LexNetwork.GetPing();

        if (Input.GetKeyDown(KeyCode.Escape)) {
            ToggleLeaveScreen();
        }

        if (startCountDown) {
            StartRespawn();
        }

    
    }

    public void ToggleLeaveScreen() {
        leaveScreen.SetActive(!leaveScreen.activeSelf);
    }

    public void OnClick_LeaveRoom() {
        LexNetwork.Disconnect();
    }

    void StartRespawn() {
        respawnDelay -= Time.deltaTime;
        respawnText.text = "Respawn in ... " + respawnDelay.ToString("F0");

        if (respawnDelay <= 0) {
            respawnPanel.SetActive(false);
            startCountDown = false;
            RelocatePlayer();
           LocalPlayer.GetComponent<LexView>().RPC("Resurrect", RpcTarget.AllBuffered);
        }
    }

    public void RelocatePlayer() {
        float randomPos = Random.Range(-5f, 5f);
        LocalPlayer.transform.localPosition = new Vector3(randomPos, 2);
    }
  public void EnableRespawn() {
        respawnDelay = 5f;
        startCountDown = true;
        respawnPanel.SetActive(true);
    }

    public override void OnPlayerEnteredRoom(LexPlayer newPlayer)
    {
        connectedPlayer.UpdatePlayerList(newPlayer.NickName);
        GameObject textObj = Instantiate(feedText, Vector2.zero, Quaternion.identity);
        textObj.transform.SetParent(feedTextBox.transform);
        textObj.GetComponent<Text>().text = "Player joined game " + newPlayer.NickName;
        Destroy(textObj, 5f);
    }
    public override void OnPlayerLeftRoom(LexPlayer newPlayer)
    {
        connectedPlayer.RemovePlayerList(newPlayer.NickName);

        GameObject textObj = Instantiate(feedText, Vector2.zero, Quaternion.identity);
        textObj.transform.SetParent(feedTextBox.transform);
        textObj.GetComponent<Text>().text = "Player left game " + newPlayer.NickName;
        Destroy(textObj, 5f);
    }


    public void SpawnPlayer() {
        float random = Random.Range(-5f, 5f);
        LexNetwork.Instantiate(playerPrefab.name,
            new Vector2(playerPrefab.transform.position.x + random,
            playerPrefab.transform.position.y),
            Quaternion.identity
            ,0);
        UIcanvas.SetActive(false);
        //sceneCam.SetActive(false);


    }
}
