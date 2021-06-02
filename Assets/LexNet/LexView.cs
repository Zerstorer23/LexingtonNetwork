using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LexView : MonoBehaviour
{
    [SerializeField] private int prViewID = -1;
    private bool requestSceneviewID = true;
    public PhotonView pv;
    public int ViewID
    {
        get { return prViewID; }
        private set { }
    }
    public int ownerActorNr;// InstantiateObject, room이면 마스터id
    public int creatorActorNr;
    public bool IsMine {
        get;
        private set;
    }//씬오브젝트, 개인 오브젝트, 마스터일경우 RoomObject도



    public bool IsRoomView { get; private set; }// 룸오브젝트, 씬오브젝트/ 마스터만 컨트롤
    public bool IsSceneView { get; private set; } = true; // 룸오브젝트, 씬오브젝트/ 마스터만 컨트롤
    public LexPlayer Owner { get; private set; }
    public MonoBehaviourLex[] RpcMonoBehaviours { get; private set; }

    object[] InstantiationData;


    public static LexView Get(Component component)
    {
        return component.transform.GetParentComponent<LexView>();
    }

    public static LexView Get(GameObject gameObj)
    {
        return gameObj.transform.GetParentComponent<LexView>();
    }
    public void RefreshRpcMonoBehaviourCache()
    {
        this.RpcMonoBehaviours = this.GetComponents<MonoBehaviourLex>();
    }

    /*
     씬 :IsMine = true, 
         IsRoomView = false,
         IsSceneView = true,
         Owner = localPlayer

    룸: IsMine = MasterClient
         IsRoomView = true,
         IsSceneView = false,
         Owner  = MasterClient
    개인: IsMine = Creator
        IsRoomView = false
        IsSceneview = false
         Owner = LocalPlayer


    씬뷰 ViewID =>0~ 0???      //ViewIDManager생성 (FindView후 순서대로 대입)
    룸뷰 ViewID =>0??? ~ 09999 //ViewIDManager생성 마지막대입부터 카운터 시작
                                //ViewID카운터는 모두가 동기화 필요
                                //마스터클라이언트 ->자기 데이터기반 다음view전송
                                ->RoomInstantiateReceive시 각자 자기 데이터 업데이트
   
    개인뷰 => OwnerID ~ n9999 //카운터 각자
     */
    //카운터는 HashSet형으로 관리후 삭제시 remove
    //1~9999 Queue에 등록
    //remove된iD queue에 추가
    //dycjdgks queue에서 삭제
    public LexNetwork_SyncVar serializedView;
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        serializedView = GetComponent<LexNetwork_SyncVar>();
        RefreshRpcMonoBehaviourCache();
        if (!Application.isPlaying && requestSceneviewID) {
            prViewID = LexViewManager.RequestSceneViewID();
            IsMine = true;
            IsSceneView = true;
            requestSceneviewID = true;
            //TODO SceneView add to dictionary on start
            //세거나 저장하거나..
            //
            //매번 세서 순서대로 번호를 붙이는게
        }
    }

    public void ReceiveSerializedVariable(params object[] parameters) {
        if (serializedView == null) {
            Debug.LogError("No sync view!!!");
            return;
        }
        serializedView.OnSyncView(parameters);
    }

    public void SetInstantiateData(object[] data) {
        InstantiationData = data;
    }
    public void SetInformation(int viewID, int ownerID, int creatorID, bool roomview) {
        this.prViewID = viewID;
        this.ownerActorNr = ownerID;
        this.creatorActorNr = creatorID;
        this.IsRoomView = roomview;
        this.IsSceneView = false;
        if (IsRoomView)
        {
            IsMine = LexNetwork.IsMasterClient;
            Owner = LexNetwork.MasterClient;
        }
        else
        {
            IsMine = LexNetwork.LocalPlayer.actorID == ownerID;
            if (IsMine)
            {
                Owner = LexNetwork.LocalPlayer;
            }
            else
            {
                Owner = LexNetwork.GetPlayerByID(ownerID);
            }
        }
        LexViewManager.AddViewtoDictionary(this);
    }
    public void UpdateOwnership() {
        if (IsRoomView) {
            IsMine = LexNetwork.IsMasterClient;
            Owner = LexNetwork.MasterClient;
        }
    }
    private bool GetIsMine()
    {
        if (IsSceneView) return true;
        if (IsRoomView) return LexNetwork.IsMasterClient;
        return creatorActorNr == LexNetwork.LocalPlayer.actorID;
    }
    public void RPC(string methodName,RpcTarget target, params object[] parameters) {
        if (!LexNetwork.useLexNet)
        {
            pv.RPC(methodName, target, parameters);
        }
        else {
            LexNetwork.instance.RPC_Send(this, methodName, parameters);
        }
    }

}
