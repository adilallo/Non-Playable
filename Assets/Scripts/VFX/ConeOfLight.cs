using UnityEngine;

public class ConeOfLight : MonoBehaviour
{

    void Update()
    {
        transform.Rotate(Vector3.up, 2 * Time.deltaTime, 0);
    }
}
