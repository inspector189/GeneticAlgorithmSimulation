using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float speed = 0.5f;
    [SerializeField]
    private float currentXRotation = 30f;
    [SerializeField]
    private float currentYRotation = -30f;
    [SerializeField]
    private float rotationSpeed = 1.5f;
    [SerializeField]
    private float moveSpeed = 1.5f;
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new(horizontal, 0, vertical);
        transform.Translate(movement * speed, Space.World);
        HandleCameraRotation();
        CameraHeightManager();
    }
    private void HandleCameraRotation()
    {
        if (Input.GetMouseButton(2))
        {
            float moveX = Input.GetAxis("Mouse X") * rotationSpeed;
            float moveY = Input.GetAxis("Mouse Y") * rotationSpeed;

            currentYRotation += moveX;
            currentXRotation += moveY;
            currentXRotation = Mathf.Clamp(currentXRotation, -85f, 85f);
            transform.eulerAngles = new Vector3(currentXRotation, -currentYRotation, transform.eulerAngles.z);
        }
    }
    private void CameraHeightManager()
    {
        Vector3 newY = transform.position;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            newY.y -= moveSpeed * Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.Space))
        {
            newY.y += moveSpeed * Time.deltaTime;
        }

        transform.position = newY;
    }
}