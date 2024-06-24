using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.UI;
using TMPro;

public class SteamLobby : MonoBehaviour
{
    public GameObject hostButton = null;
    private NetworkManager networkManager;
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    private const string HostAddressKey = "HostAddress";
    public GameObject lobbyUI = null;
    public Transform playerListContainer = null;
    public GameObject playerListItemPrefab = null;

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
        if (!SteamManager.Initialized) { return; }

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
        hostButton.SetActive(false);
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            hostButton.SetActive(true);
            return;
        }

        networkManager.StartHost();
        SetLobbyHostData(callback.m_ulSteamIDLobby);
        lobbyUI.SetActive(true);
    }

    private void SetLobbyHostData(ulong steamIDLobby)
    {
        SteamMatchmaking.SetLobbyData(new CSteamID(steamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        // If host
        if (NetworkServer.active)
        {
            UpdateLobbyUI(callback.m_ulSteamIDLobby);
        }
        // If client joining
        else
        {
            ConnectToLobbyHost(callback.m_ulSteamIDLobby);
        }

        // Show lobby UI for all clients
        lobbyUI.SetActive(true);
        hostButton.SetActive(false);
    }

    private void ConnectToLobbyHost(ulong steamIDLobby)
    {
        string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(steamIDLobby), HostAddressKey);
        networkManager.networkAddress = hostAddress;
        networkManager.StartClient();
    }

    private void UpdateLobbyUI(ulong steamIDLobby)
    {
        if (!ValidateUIComponents()) { return; }

        ClearPlayerList();
        PopulatePlayerList(steamIDLobby);
    }

    private bool ValidateUIComponents()
    {
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
        foreach (Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void PopulatePlayerList(ulong steamIDLobby)
    {
        CSteamID lobbyID = new CSteamID(steamIDLobby);
        int numMembers = SteamMatchmaking.GetNumLobbyMembers(lobbyID);

        for (int i = 0; i < numMembers; i++)
        {
            CSteamID memberSteamID = SteamMatchmaking.GetLobbyMemberByIndex(lobbyID, i);
            string memberName = SteamFriends.GetFriendPersonaName(memberSteamID);

            GameObject playerListItem = Instantiate(playerListItemPrefab, playerListContainer);
            SetPlayerListItem(playerListItem, memberSteamID, memberName);
        }
    }

    private void SetPlayerListItem(GameObject playerListItem, CSteamID memberSteamID, string memberName)
    {
        SetPlayerName(playerListItem, memberName);
        SetPlayerAvatar(playerListItem, memberSteamID);
    }

    private void SetPlayerName(GameObject playerListItem, string memberName)
    {
        TextMeshProUGUI usernameText = playerListItem.transform.Find("Username").GetComponent<TextMeshProUGUI>();
        if (usernameText != null)
        {
            usernameText.text = memberName;
        }
        else
        {
            Debug.LogError("username TextMeshProUGUI component not found in playerListItem");
        }
    }

    private void SetPlayerAvatar(GameObject playerListItem, CSteamID memberSteamID)
    {
        int avatar = SteamFriends.GetLargeFriendAvatar(memberSteamID);
        if (avatar != -1)
        {
            Texture2D avatarTexture = GetSteamImageAsTexture(avatar);
            if (avatarTexture != null)
            {
                ApplyAvatarTexture(playerListItem, avatarTexture);
            }
        }
        else
        {
            Debug.LogWarning("Failed to get avatar for member: " + SteamFriends.GetFriendPersonaName(memberSteamID));
        }
    }

    private Texture2D GetSteamImageAsTexture(int imageId)
    {
        SteamUtils.GetImageSize(imageId, out uint width, out uint height);

        byte[] image = new byte[width * height * 4];
        SteamUtils.GetImageRGBA(imageId, image, (int)(width * height * 4));

        Texture2D texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
        texture.LoadRawTextureData(image);
        texture.Apply();

        return FlipTextureVertically(texture);
    }

    private void ApplyAvatarTexture(GameObject playerListItem, Texture2D texture)
    {
        RawImage userImage = playerListItem.transform.Find("UserImage").GetComponent<RawImage>();
        if (userImage != null)
        {
            userImage.texture = texture;
        }
        else
        {
            Debug.LogError("UserImage RawImage component not found in playerListItem");
        }
    }

    private Texture2D FlipTextureVertically(Texture2D original)
    {
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
        // Stop the client or host
        if (NetworkServer.active)
        {
            networkManager.StopHost();
        }
        else if (NetworkClient.active)
        {
            networkManager.StopClient();
        }

        // Hide the lobby UI
        lobbyUI.SetActive(false);

        // Reactivate the host button if necessary
        hostButton.SetActive(true);

        // Clear the player list (optional, depends on your implementation)
        ClearPlayerList();
    }
}
