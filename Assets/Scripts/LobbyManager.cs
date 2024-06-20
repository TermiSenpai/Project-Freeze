using Mirror;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{

    public static LobbyManager Instance;
    public CustomNetworkManager networkManager;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        networkManager.StartCustomManager();
    }

    public void StartHost()
    {
        NetworkManager.singleton.StartHost();
    }

    public void StartClient()
    {
        NetworkManager.singleton.StartClient();
    }

    public void StartServer()
    {
        NetworkManager.singleton.StartServer();
    }

    public void StopHost()
    {
        NetworkManager.singleton.StopHost();
    }

    public void StopClient()
    {
        NetworkManager.singleton.StopClient();
    }

    public void StopServer()
    {
        NetworkManager.singleton.StopServer();
    }
}
