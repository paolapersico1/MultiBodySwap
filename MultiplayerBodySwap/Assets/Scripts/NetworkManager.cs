using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[System.Serializable]
public enum AvatarType
{
    None,
    Man,
    Woman
}

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private GameObject spawnedPlayerPrefab;
    private GameObject spawnedPlayerPrefabReplica;
    public AvatarType avatarType = AvatarType.None;

    public void SetAvatarType(string type)
    {
        try
        {
            avatarType = (AvatarType)System.Enum.Parse(typeof(AvatarType), type);
        }
        catch (System.Exception)
        {
            Debug.LogErrorFormat("Parse: Can't convert {0} to enum, please check the spell.", type);
        }
    }

    public void ConnectToServer()
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Try connect to server...");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Server");
        base.OnConnectedToMaster();
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;
        PhotonNetwork.JoinOrCreateRoom("Room 1", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined a Room");
        base.OnJoinedRoom();
        if (avatarType != AvatarType.None)
        {
            spawnedPlayerPrefabReplica = PhotonNetwork.Instantiate(avatarType.ToString() + "Replica", transform.position, transform.rotation);
            spawnedPlayerPrefab = PhotonNetwork.Instantiate(avatarType.ToString(), transform.position, transform.rotation);
        }
        else
        {
            Debug.Log("Error: Avatar Type is None");
        }
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        PhotonNetwork.Destroy(spawnedPlayerPrefab);
        PhotonNetwork.Destroy(spawnedPlayerPrefabReplica);
        avatarType = AvatarType.None;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("A new player joined the room");
        base.OnPlayerEnteredRoom(newPlayer);
    }
}
