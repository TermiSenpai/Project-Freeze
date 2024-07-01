using UnityEngine;
using Mirror;
using Steamworks;
using TMPro;

public class SteamLobby : MonoBehaviour
{
    #region Variables
    // Variables
    private NetworkManager networkManager;
    private const string HostAddressKey = "HostAddress";
    #endregion

    #region Callbacks
    // Callbacks
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    protected Callback<AvatarImageLoaded_t> avatarImageLoaded;
    #endregion

    #region References
    // References
    [SerializeField] GameObject lobby;
    [SerializeField] GameObject items;
    [SerializeField] TextMeshProUGUI lobbyTxt;
    [SerializeField] GameObject hostBtn;
    [SerializeField] GameObject playerUI;
    [SerializeField] GameObject playBtn;

    #endregion

    #region Unity Methods

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

    #endregion

    #region Steam Callbacks Initialization

    private void InitializeSteamCallbacks()
    {
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
    }

    #endregion

    #region Lobby Management

    public void HostLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
            return;

        networkManager.StartHost();
        SetLobbyHostData(callback.m_ulSteamIDLobby);
        //Debug.LogError(callback.m_ulSteamIDLobby);
    }

    private void SetLobbyHostData(ulong steamIDLobby)
    {
        SteamMatchmaking.SetLobbyData(new CSteamID(steamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(steamIDLobby), "Name", SteamFriends.GetPersonaName().ToString() + "`s lobby");
    }

    public void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        // Everyone
        lobby.SetActive(true);
        lobbyTxt.text = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "Name");
        hostBtn.SetActive(false);

        // Instantiate the UI prefab and make it a child of the lobby object
        GameObject uiInstance = Instantiate(playerUI, items.transform);
        PlayerListItemUI playerListItemUI = uiInstance.GetComponent<PlayerListItemUI>();

        if (playerListItemUI != null)
        {
            string playerName = SteamFriends.GetPersonaName();
            int imageId = SteamFriends.GetLargeFriendAvatar(SteamUser.GetSteamID());

            if (imageId != -1)
            {
                Texture2D avatarTexture = GetSteamImageAsTexture(imageId);
                playerListItemUI.SetPlayerInfo(playerName, avatarTexture);
            }
        }

        // Clients
        if (!NetworkServer.active)
            ConnectToLobbyHost(callback.m_ulSteamIDLobby);
    }

    private void ConnectToLobbyHost(ulong steamIDLobby)
    {
        playBtn.SetActive(false);
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

        hostBtn.SetActive(true);
        playBtn.SetActive(true);
        lobby.SetActive(false);

        // Eliminar todos los hijos de items
        foreach (Transform child in items.transform)
        {
            Destroy(child.gameObject);
        }

    }

    public void OnPlayButtonClicked()
    {
        networkManager.ServerChangeScene("InsideLobby");
    }

    #endregion

    #region Avatar Handling

    private void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
    {
        if (callback.m_steamID == SteamUser.GetSteamID())
        {
            // Handle avatar update if necessary
        }
    }

    private Texture2D GetSteamImageAsTexture(int imageId)
    {
        bool isValid = SteamUtils.GetImageSize(imageId, out uint width, out uint height);
        if (!isValid) return null;

        byte[] image = new byte[width * height * 4];
        isValid = SteamUtils.GetImageRGBA(imageId, image, (int)(width * height * 4));
        if (!isValid) return null;

        Texture2D texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
        texture.LoadRawTextureData(image);
        texture.Apply();

        FlipTextureVertically(texture);

        return texture;
    }

    private void FlipTextureVertically(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();
        Color[] flippedPixels = new Color[pixels.Length];
        int width = texture.width;
        int height = texture.height;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                flippedPixels[x + y * width] = pixels[x + (height - 1 - y) * width];
            }
        }

        texture.SetPixels(flippedPixels);
        texture.Apply();
    }

    #endregion
}
