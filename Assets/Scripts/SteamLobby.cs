using UnityEngine;
using Mirror;
using Steamworks;

public class SteamLobby : MonoBehaviour
{
    private NetworkManager networkManager;
    private const string HostAddressKey = "HostAddress";
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;

    private void Awake()
    {
        networkManager = FindObjectOfType<NetworkManager>();
        if (networkManager == null)
        {
            Debug.LogError("NetworkManager not found in the scene.");
            return;
        }
    }

    private void Start()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("SteamManager is not initialized.");
            return;
        }

        InitializeSteamCallbacks();
    }

    private void InitializeSteamCallbacks()
    {
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            return;
        }

        networkManager.StartHost();
        SetLobbyHostData(callback.m_ulSteamIDLobby);
    }

    private void SetLobbyHostData(ulong steamIDLobby)
    {
        SteamMatchmaking.SetLobbyData(new CSteamID(steamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
    }

    public void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (!NetworkServer.active)        
            ConnectToLobbyHost(callback.m_ulSteamIDLobby);
        
    }

    private void ConnectToLobbyHost(ulong steamIDLobby)
    {
        string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(steamIDLobby), HostAddressKey);
        networkManager.networkAddress = hostAddress;
        networkManager.StartClient();
    }

    public void DisconnectFromLobby()
    {
        if (NetworkServer.active)
        {
            networkManager.StopHost();
        }
        else if (NetworkClient.active)
        {
            networkManager.StopClient();
        }
    }

    public void OnPlayButtonClicked()
    {
        networkManager.ServerChangeScene("InsideLobby");
    }
}
