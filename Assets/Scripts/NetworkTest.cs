using UnityEngine;
using Mirror;
using Steamworks;

public class NetworkTest : MonoBehaviour
{
    private NetworkManager networkManager;
    private SteamLobby steamLobby;
    [SerializeField] ulong fakeLobbyID;

    private void Awake()
    {
        networkManager = FindObjectOfType<NetworkManager>();
        steamLobby = FindObjectOfType<SteamLobby>();

        if (networkManager == null)
        {
            Debug.LogError("NetworkManager not found in the scene.");
        }

        if (steamLobby == null)
        {
            Debug.LogError("SteamLobby not found in the scene.");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            HostGame();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            ConnectToGame();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            StartGame();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            DisconnectFromGame();
        }
    }

    private void HostGame()
    {
        if (steamLobby != null)
        {
            steamLobby.HostLobby();
        }
    }

    private void ConnectToGame()
    {
        // Aquí puedes colocar la lógica para unirte a una sesión específica
        // Por simplicidad, estamos llamando un método en steamLobby que gestiona la conexión.
        if (steamLobby != null)
        {
            // Puedes simular una solicitud de unión a un lobby usando un ID específico            
            steamLobby.OnGameLobbyJoinRequested(new GameLobbyJoinRequested_t { m_steamIDLobby = new CSteamID(fakeLobbyID) });
        }
    }

    private void StartGame()
    {
        if (steamLobby != null)
        {
            steamLobby.OnPlayButtonClicked();
        }
    }

    private void DisconnectFromGame()
    {
        if (steamLobby != null)
        {
            steamLobby.DisconnectFromLobby();
        }
    }
}
