using UnityEngine;

public class CameraControll : MonoBehaviour
{
    public float speed = 10f;
    public float turnSpeed = 10f;

    private void Update()
    {
        var deltaSpeed = Time.deltaTime * speed;
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * deltaSpeed;
        }

        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * deltaSpeed;
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * deltaSpeed;
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= transform.right * deltaSpeed;
        }

        if (Input.GetMouseButton(0))
        {
            var deltaTurnSpeed = Time.deltaTime * turnSpeed;
            var rotation = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0f);
            transform.eulerAngles += deltaTurnSpeed * rotation;
        }
    }
}