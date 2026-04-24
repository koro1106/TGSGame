using UnityEngine;

public class CrosshairFollow : MonoBehaviour
{
    public Camera cam;

    void Start()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        Vector3 pos = Input.mousePosition;
        pos.z = 0;
        transform.position = pos;
    }
}