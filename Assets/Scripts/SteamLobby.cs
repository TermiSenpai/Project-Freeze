using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.UI;
using TMPro;

public class SteamLobby : MonoBehaviour
{
    [Header("UI References")]
    public GameObject hostButton;
    public GameObject lobbyUI;
    public Transform playerListContainer;
    public GameObject playerListItemPrefab;

    private NetworkManager networkManager;
    private const string HostAddressKey = "HostAddress";
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;

    private void Awake()
    {
        // Find and assign the NetworkManager component in the scene
        networkManager = FindObjectOfType<NetworkManager>();
        if (networkManager == null)
        {
            Debug.LogError("NetworkManager not found in the scene.");
            return;
        }
    }

    private void Start()
    {
        // Check if Steam is initialized
        if (!SteamManager.Initialized)
        {
            Debug.LogError("SteamManager is not initialized.");
            return;
        }

        // Initialize Steam callbacks
        InitializeSteamCallbacks();
    }

    private void InitializeSteamCallbacks()
    {
        // Create Steam callbacks for lobby events
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostLobby()
    {
        // Disable host button and create a Steam lobby
        hostButton.SetActive(false);
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        // Check if lobby creation was successful
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            hostButton.SetActive(true); // Show host button again
            return;
        }

        // Start hosting the game
        networkManager.StartHost();

        // Set host data in Steam lobby
        SetLobbyHostData(callback.m_ulSteamIDLobby);

        // Activate lobby UI
        lobbyUI.SetActive(true);
    }

    private void SetLobbyHostData(ulong steamIDLobby)
    {
        // Set host address data in the Steam lobby
        SteamMatchmaking.SetLobbyData(new CSteamID(steamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        // Join the game lobby requested by another player
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        // Handle actions when entering a lobby
        if (NetworkServer.active)
        {
            // Update lobby UI if the current instance is the host
            UpdateLobbyUI(callback.m_ulSteamIDLobby);
        }
        else
        {
            // Connect to the host's lobby if joining as a client
            ConnectToLobbyHost(callback.m_ulSteamIDLobby);
        }

        // Show lobby UI
        lobbyUI.SetActive(true);

        // Hide host button
        hostButton.SetActive(false);
    }

    private void ConnectToLobbyHost(ulong steamIDLobby)
    {
        // Get host address and connect to the lobby
        string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(steamIDLobby), HostAddressKey);
        networkManager.networkAddress = hostAddress;
        networkManager.StartClient();
    }

    private void UpdateLobbyUI(ulong steamIDLobby)
    {
        // Validate UI components before updating
        if (!ValidateUIComponents())
        {
            return;
        }

        // Clear existing player list
        ClearPlayerList();

        // Populate player list with lobby members
        PopulatePlayerList(steamIDLobby);
    }

    private bool ValidateUIComponents()
    {
        // Check if required UI components are assigned
        if (playerListContainer == null)
        {
            Debug.LogError("playerListContainer is null");
            return false;
        }

        if (playerListItemPrefab == null)
        {
            Debug.LogError("playerListItemPrefab is null");
            return false;
        }

        return true;
    }

    private void ClearPlayerList()
    {
        // Clear the player list in the UI
        foreach (Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void PopulatePlayerList(ulong steamIDLobby)
    {
        // Get lobby ID and the number of members
        CSteamID lobbyID = new CSteamID(steamIDLobby);
        int numMembers = SteamMatchmaking.GetNumLobbyMembers(lobbyID);

        // Populate player list with lobby member information
        for (int i = 0; i < numMembers; i++)
        {
            CSteamID memberSteamID = SteamMatchmaking.GetLobbyMemberByIndex(lobbyID, i);
            string memberName = SteamFriends.GetFriendPersonaName(memberSteamID);
            Texture2D avatarTexture = GetSteamAvatar(memberSteamID);

            // Instantiate player list item prefab
            GameObject playerListItem = Instantiate(playerListItemPrefab, playerListContainer);

            // Get PlayerListItemUI component
            PlayerListItemUI listItemUI = playerListItem.GetComponent<PlayerListItemUI>();

            // Set player information in the UI item
            if (listItemUI != null)
            {
                listItemUI.SetPlayerInfo(memberName, avatarTexture);
            }
            else
            {
                Debug.LogError("PlayerListItemUI component not found in playerListItem.");
            }
        }
    }

    private Texture2D GetSteamAvatar(CSteamID steamID)
    {
        // Get large avatar image for a Steam user
        int avatar = SteamFriends.GetLargeFriendAvatar(steamID);
        if (avatar != -1)
        {
            return GetSteamImageAsTexture(avatar);
        }
        else
        {
            Debug.LogWarning("Failed to get avatar for member: " + SteamFriends.GetFriendPersonaName(steamID));
            return null;
        }
    }

    private Texture2D GetSteamImageAsTexture(int imageId)
    {
        // Load Steam image as a Texture2D
        SteamUtils.GetImageSize(imageId, out uint width, out uint height);

        byte[] image = new byte[width * height * 4];
        SteamUtils.GetImageRGBA(imageId, image, (int)(width * height * 4));

        Texture2D texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
        texture.LoadRawTextureData(image);
        texture.Apply();

        // Correctly orient the texture vertically
        texture = FlipTextureVertically(texture);

        return texture;
    }

    private Texture2D FlipTextureVertically(Texture2D original)
    {
        // Flip the texture vertically
        Texture2D flipped = new Texture2D(original.width, original.height);
        int width = original.width;
        int height = original.height;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                flipped.SetPixel(x, height - 1 - y, original.GetPixel(x, y));
            }
        }

        flipped.Apply();
        return flipped;
    }

    public void DisconnectFromLobby()
    {
        // Disconnect from the lobby (stop hosting or stop client)
        if (NetworkServer.active)
        {
            networkManager.StopHost();
        }
        else if (NetworkClient.active)
        {
            networkManager.StopClient();
        }

        // Hide lobby UI
        lobbyUI.SetActive(false);

        // Show host button
        hostButton.SetActive(true);

        // Clear player list in the UI
        ClearPlayerList();
    }
}
