using UnityEngine;
using Mirror;
using System.Linq;

public class NetworkManagerExtension : NetworkManager
{
    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName == "InsideLobby")
        {
            GameObject player = Instantiate(playerPrefab);
            NetworkServer.AddPlayerForConnection(NetworkServer.connections.Values.FirstOrDefault(), player);
        }
    }

    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();

        if (NetworkClient.isConnected && !NetworkClient.ready)
        {
            NetworkClient.Ready();
            NetworkClient.AddPlayer();
        }
    }
}
