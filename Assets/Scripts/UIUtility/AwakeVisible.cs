

using UnityEngine;

public class AwakeVisible : MonoBehaviour
{
    public bool isVisible = true;
    void Awake()
    {
        gameObject.SetActive(isVisible);
    }
}