using UnityEngine;

public class SmoothCameraFollowIsometric : MonoBehaviour
{
    private Vector3 _offset;
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime;
    private Vector3 _currentVelocity = Vector3.zero;

    
    [SerializeField] private float mouseMoveSpeed = 1f;

    private void Awake()
    {
        _offset = transform.position - target.position;
    }

    private void LateUpdate()
    {
        
        float mouseX = Input.mousePosition.x;
        float screenWidth = Screen.width;
        float mouseOffsetX = (mouseX / screenWidth) * 2f - 1f; 

        float mouseY = Input.mousePosition.y;
        float screenHeight = Screen.height;
        float mouseOffsetY = (mouseY / screenHeight) * 2f - 1f; 

        
        Vector3 desiredPosition = target.position + _offset;
        desiredPosition += transform.right * mouseOffsetX * mouseMoveSpeed;
        desiredPosition += transform.up * mouseOffsetY * mouseMoveSpeed;

        
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _currentVelocity, smoothTime);

        
        transform.rotation = Quaternion.Euler(30, 45, 0);
    }
}
