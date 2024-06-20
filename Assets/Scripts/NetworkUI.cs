using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public CustomNetworkManager networkManager;
    public TMP_InputField roomCodeInputField;

    public void OnCreateRoomButtonPressed()
    {
        networkManager.CreateSteamLobby();
    }

    public void OnJoinRoomButtonPressed()
    {
        string roomCode = roomCodeInputField.text;
        networkManager.JoinSteamLobby(roomCode);
    }
}
