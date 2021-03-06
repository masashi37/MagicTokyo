﻿
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class GameStartDirector : NetworkBehaviour
{

    enum State
    {
        CONNECT,
        READY,
        START,
    }
    State state_ = State.CONNECT;

    public bool IsConnect { get { return state_ == State.CONNECT; } }
    public bool IsReady { get { return state_ == State.READY; } }
    public bool IsStart { get { return state_ == State.START; } }

    float count_ = 3;
    float standby_count_ = 0.0f;
    const float STANBY_LIMIT = 3.0f;
    public int ReadyCount { get { return (int)count_; } }

    bool is_se_player = false;

    Text text_;

    [SerializeField
    , Tooltip("ここに「Ike3ParticleManager」prefabを入れてください\n(プログラマー用)")]
    private GameObject particle_manager_;

    [SerializeField
    , Tooltip("表示させたいパーティクルを入れてください(「「3」」←これ　こ　れ)→「2」→「1」→「Start」")]
    private ParticleSystem particle_;

    void Start()
    {
        var is_tutorial = MyNetworkLobbyManager.s_singleton.IsTutorial;
        if (!is_tutorial)
        {
            if (!isLocalPlayer) return;
        }
        text_ = GameObject.Find("StartText").GetComponent<Text>();

        if (is_tutorial)
        {
            text_.enabled = false;
        }
    }

    public void RpcCountDownLocal()
    {
        GameObject particle_manager = GameObject.Find(particle_manager_.name);
        ParticleSystem game_object = Instantiate(particle_);
        game_object.transform.SetParent(particle_manager.transform);
        game_object.transform.position = new Vector3(0.0f, 3.0f, 1.5f);
        game_object.name = particle_.name;
    }

    //public void RpcCountDownRemote()
    //{
    //    GameObject particle_manager = GameObject.Find(particle_manager_.name);
    //    ParticleSystem game_object = Instantiate(particle_);
    //    game_object.transform.SetParent(particle_manager.transform);
    //    game_object.transform.position = new Vector3(0.0f, 4.0f, 1.5f);
    //    game_object.name = particle_.name;
    //}

    void Update()
    {
        if (!MyNetworkLobbyManager.s_singleton.IsTutorial)
        {
            if (!isLocalPlayer) return;
        }
        if (state_ != State.READY) return;

        if (STANBY_LIMIT > standby_count_)
        {
            standby_count_ += Time.deltaTime;
            return;
        }
        if (count_ == STANBY_LIMIT)
        {
            AudioManager.Instance.PlayBgm(MyNetworkLobbyManager.s_singleton.BGMCount % 3);
            MyNetworkLobbyManager.s_singleton.BGMCount++;
            RpcCountDownLocal();
        }
        //ChangeText();
        text_.text = "";
        count_ += -Time.deltaTime;

        if (count_ > 0) return;
        state_ = State.START;
        CmdTellServerStart();
        text_.enabled = false;
        if (!is_se_player)
        {
            if (MyNetworkLobbyManager.s_singleton.Is1P)
            {
                AudioManager.Instance.PlaySe(22);
            }
            else
            {
                AudioManager.Instance.PlaySe(18);
            }
            //AudioManager.Instance.PlaySe(5);
            is_se_player = true;
        }
    }

    void ChangeText()
    {
        var count = (int)count_;
        text_.text = count.ToString();
        if (count_ > 0.5f) return;
        text_.text = "Start";
    }

    [ClientRpc]
    public void RpcTellClientReady()
    {
        state_ = State.READY;
    }

    [Command]
    public void CmdTellServerStart()
    {
        state_ = State.START;
    }
}
