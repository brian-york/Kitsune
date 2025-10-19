using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    void Awake()
    {
        if (ProgressManager.Instance == null)
        {
            GameObject progressManagerObj = new GameObject("ProgressManager");
            progressManagerObj.AddComponent<ProgressManager>();
            Debug.Log("🔧 GameBootstrapper created ProgressManager for testing");
        }
    }
}
