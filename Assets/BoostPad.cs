using UnityEngine;

public class BoostPad : MonoBehaviour
{
    [SerializeField] private float boostForce;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) {
            Rigidbody rigidbody = other.GetComponent<Rigidbody>();
            rigidbody.AddForce(transform.forward * boostForce, ForceMode.VelocityChange);
        }
    }
}
