using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;     
    public float smooth = 5f;   

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 pos = transform.position;
        Vector3 goal = new Vector3(target.position.x, target.position.y, pos.z);

        transform.position = Vector3.Lerp(pos, goal, smooth * Time.deltaTime);
    }
}
