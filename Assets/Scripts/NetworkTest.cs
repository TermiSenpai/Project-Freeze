using UnityEngine;
using Mirror;
using Steamworks;

public class NetworkTest : MonoBehaviour
{
    private NetworkManager networkManager;
    private SteamLobby steamLobby;

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
        // Aqu� puedes colocar la l�gica para unirte a una sesi�n espec�fica
        // Por simplicidad, estamos llamando un m�todo en steamLobby que gestiona la conexi�n.
        if (steamLobby != null)
        {
            // Puedes simular una solicitud de uni�n a un lobby usando un ID espec�fico
            ulong fakeLobbyID = 1234567890; // Reemplazar con un ID de lobby v�lido
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
