using UnityEngine;

public class ConcussionDetector : MonoBehaviour
{
    public EscapeAgent agent;
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("walkableSurface"))
        {
            agent.HitHead();
        }
    }
}
