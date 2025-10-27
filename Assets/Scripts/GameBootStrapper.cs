using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    void Awake()
    {
        if (ProgressManager.Instance == null)
        {
            GameObject progressManagerObj = new GameObject("ProgressManager");
            var pm = progressManagerObj.AddComponent<ProgressManager>();
            pm.currentPuzzleId = "test001";
            Debug.Log("🔧 GameBootstrapper created ProgressManager with test puzzle");
        }
    }
}
