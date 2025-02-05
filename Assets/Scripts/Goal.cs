using UnityEngine;

public class Goal : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {  
        if (collider.gameObject.CompareTag("Player")) {
            TimeManager.Instance.StopwatchPause();
        }

    }
}
