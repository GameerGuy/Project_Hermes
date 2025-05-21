using UnityEngine;

public class Path : MonoBehaviour
{
    [Range(0, 1)][SerializeField] private float _clipTo;
    public float clipTo => _clipTo;
    
}
