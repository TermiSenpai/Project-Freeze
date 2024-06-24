using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerListItemUI : MonoBehaviour
{
    public TextMeshProUGUI usernameText;
    public RawImage userImage;

    public void SetPlayerInfo(string playerName, Texture2D avatar)
    {
        usernameText.text = playerName;
        ApplyAvatarTexture(avatar);
    }

    private void ApplyAvatarTexture(Texture2D texture)
    {
        if (userImage != null && texture != null)
        {
            userImage.texture = texture;
        }
    }
}
