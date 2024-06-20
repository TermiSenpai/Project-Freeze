using Mirror;
using Steamworks;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    private Callback<LobbyCreated_t> lobbyCreated;
    private Callback<LobbyEnter_t> lobbyEntered;
    private Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    private CallResult<LobbyMatchList_t> lobbyMatchList;

    public void StartCustomManager()
    {
        if (!SteamManager.Initialized) return;

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyMatchList = CallResult<LobbyMatchList_t>.Create(OnLobbyMatchList);
    }

    public void CreateSteamLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, maxConnections);
    }

    private void OnLobbyCreated(LobbyCreated_t result)
    {
        if (result.m_eResult == EResult.k_EResultOK)
        {
            // Start hosting Mirror server
            StartHost();

            // Generate a unique room code
            string roomCode = GenerateRoomCode();
            Debug.Log("Room Code: " + roomCode);

            // Set lobby data with room code and host address
            SteamMatchmaking.SetLobbyData(new CSteamID(result.m_ulSteamIDLobby), "RoomCode", roomCode);
            SteamMatchmaking.SetLobbyData(new CSteamID(result.m_ulSteamIDLobby), "HostAddress", SteamUser.GetSteamID().ToString());
        }
        else
        {
            Debug.LogError("Failed to create Steam lobby.");
        }
    }

    private void OnLobbyEntered(LobbyEnter_t result)
    {
        if (NetworkServer.active) return; // Ignore if we're the host

        // Connect to the host
        networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(result.m_ulSteamIDLobby), "HostAddress");
        StartClient();
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t result)
    {
        SteamMatchmaking.JoinLobby(result.m_steamIDLobby);
    }

    public void JoinSteamLobby(string roomCode)
    {
        // Request the list of lobbies
        SteamAPICall_t handle = SteamMatchmaking.RequestLobbyList();
        lobbyMatchList.Set(handle);
        // Save the room code to match it in the callback
        currentRoomCode = roomCode;
    }

    private string currentRoomCode;

    private void OnLobbyMatchList(LobbyMatchList_t result, bool bIOFailure)
    {
        if (bIOFailure || result.m_nLobbiesMatching == 0)
        {
            Debug.LogError("No lobbies found or there was an IO failure.");
            return;
        }

        for (int i = 0; i < result.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            if (SteamMatchmaking.GetLobbyData(lobbyID, "RoomCode") == currentRoomCode)
            {
                SteamMatchmaking.JoinLobby(lobbyID);
                break;
            }
        }
    }

    private string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] stringChars = new char[6];
        System.Random random = new System.Random();

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(stringChars);
    }
}
