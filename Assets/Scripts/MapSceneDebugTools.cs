using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSceneDebugTools : MonoBehaviour
{
    public void GoToRelicShopScene()
    {
        Debug.Log("🧭 Loading RelicShopScene from MapScene...");
        SceneManager.LoadScene("RelicShopScene");
    }
}
