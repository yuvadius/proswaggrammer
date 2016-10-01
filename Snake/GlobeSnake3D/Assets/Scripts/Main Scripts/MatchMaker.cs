﻿using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Photon;

enum skins { Red, Green, Blue, Yellow };

public class MatchMaker : PunBehaviour
{
    public SnakeSync mySync;
    public static MatchMaker instance;
    private static GameObject snake;
    private static bool isSnake = false;
    private static int playerNumber;
    private static string skin = null;

    void Awake()
    {
        if(instance)
            DestroyImmediate(gameObject);
        else
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
    }

    void Start()
    {
        if(PhotonNetwork.connectionState != ConnectionState.Connected)
            PhotonNetwork.ConnectUsingSettings("0.1");
    }

    void OnGUI()
    {
        GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString() + "/" + PhotonNetwork.GetPing().ToString());
    }

    public override void OnJoinedLobby()
    {
        PhotonNetwork.JoinOrCreateRoom("Europe", null, null);
    }

    public static bool CreatePlayer()
    {
        if (!isSnake && PhotonNetwork.connectionStateDetailed == ClientState.Joined)
        {
            if (skin == null)
            {
                int skinNumber = GetSkin();
                skin = Enum.GetName(typeof(skins), skinNumber);
                ExitGames.Client.Photon.Hashtable style = new ExitGames.Client.Photon.Hashtable();
                style.Add("Skin", skinNumber);
                PhotonNetwork.player.SetCustomProperties(style);
            }
            Debug.Log("Your skin is: " + skin);
            snake = PhotonNetwork.Instantiate("Remote Snake " + skin, new Vector3(), Quaternion.identity, 0);
            foreach (Transform child in snake.transform)
                GameObject.Destroy(child.gameObject);
            snake.name = "Snake Syncer";
            instance.mySync = snake.GetComponent<SnakeSync>();
            isSnake = true;
            return true;
        }
        return false;
    }

    private static int GetSkin()
    {
        return 1;
        int[] colors = new int[Enum.GetValues(typeof(skins)).Length];
        foreach (var player in PhotonNetwork.otherPlayers)
            colors[(int)player.customProperties["Skin"]]++;
        return colors.ToList().IndexOf(colors.Min());
    } 

    public static void DestroyPlayer()
    {
        if (isSnake)
        {
            PhotonNetwork.Destroy(snake);
            isSnake = false;
        }
    }

    public override void OnCreatedRoom()
    {
        // PhotonNetwork.InstantiateSceneObject("Globe", new Vector3(), Quaternion.identity, 0, null);
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer other)
    {
        Debug.Log("OnPhotonPlayerConnected() " + other.name); // not seen if you're the player connecting
        if (SnakeController.instance.trail.segmentList.Count != 0 && isSnake)
        {
            mySync.syncTrail(other);
        }
    }
}