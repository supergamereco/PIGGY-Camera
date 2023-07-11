using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam : MonoBehaviour
{
    public static Cam Instance;
    public float panSpeed = 15;
    public float scrollZoomSpeed = 20;
    public float zoomSensitivity = 2;
    public float touchZoomSpeed = 0.2f;

    public float maxZoom = 8;
    public float minZoom = 1;

    public float maxPanDistanceX = 10;
    public float minPanDistanceX = 16;
    public float maxPanDistanceY = 4;
    public float minPanDistanceY = 10;

    private Vector3 lastPanPosition;
    private int panFingerId;

    public bool canPan = true;
    private bool flag = false;
    private Vector3 target_position;
    private Vector3 current_position = Vector3.zero;
    private Vector3 camera_position = Vector3.zero;
    private float dragTime = 0;
    public float maxSlideTime = 1.75f;
    private bool isDrag = false;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (canPan && !Input.touchSupported)
        {
            DragTimeCounter(isDrag);
            if (Input.GetMouseButtonDown(0))
            {
                isDrag = true;
                lastPanPosition = Input.mousePosition;
                camera_position = transform.position;
            }
            else if (Input.GetMouseButton(0))
            {
                isDrag = true;
                Vector3 delta = Input.mousePosition - lastPanPosition;
                current_position = Input.mousePosition;
                if (dragTime < maxSlideTime)
                {
                    PanCamera(delta);
                    lastPanPosition = Input.mousePosition;
                    UpdateTargetPositionCamera();
                    flag = true;
                }
                else
                {
                    flag = false;
                    UpdateTargetPositionCamera();
                    PanCamera(delta);
                    lastPanPosition = Input.mousePosition;
                }
            }
            else
            {
                isDrag = false;
                dragTime = 0;
            }
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            ScrollZoomCamera(scroll);
        }

        if (Input.touchSupported && canPan)
        {
            HandleTouch();
        }

        if (flag && !isDrag)
        {
            //transform.position = Vector3.MoveTowards(transform.position, target_position, Time.deltaTime * panSpeed);
            transform.position = Vector3.Lerp(transform.position, target_position, Time.deltaTime * panSpeed);
            if (transform.position == target_position)
            {
                flag = false;// stop moving
            }
        }
    }
    void DragTimeCounter(bool is_drag)
    {
        if (is_drag)
        {
            dragTime = dragTime + Time.deltaTime;
        }
    }
    void UpdateTargetPositionCamera()
    {
        current_position.z = lastPanPosition.z = camera_position.z;

        Vector3 direction = Camera.main.ScreenToWorldPoint(current_position) - Camera.main.ScreenToWorldPoint(lastPanPosition);
        direction = direction * -1;

        Debug.Log("direction x" + direction.x);
        Debug.Log("direction y" + direction.y);

        target_position = camera_position + direction;
    }

    void PanCamera(Vector3 delta)
    {
        float deltaX = -delta.x / Screen.width * panSpeed;
        float deltaY = -delta.y / Screen.height * panSpeed;

        Vector3 panDelta = new Vector3(deltaX, deltaY, 0f);
        Vector3 newPosition = transform.position + transform.TransformDirection(panDelta);

        newPosition.x = Mathf.Clamp(newPosition.x, -minPanDistanceX, maxPanDistanceX);
        newPosition.y = Mathf.Clamp(newPosition.y, -minPanDistanceY, maxPanDistanceY);

        transform.position = newPosition;
    }

    void ScrollZoomCamera(float scroll)
    {
        Camera.main.fieldOfView += scroll * scrollZoomSpeed;
        Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, minZoom, maxZoom);
    }

    void TouchZoomCamera()
    {
        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

        float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
        float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

        float difference = prevMagnitude - currentMagnitude;

        Camera.main.fieldOfView += difference * touchZoomSpeed;
        Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, minZoom, maxZoom);
    }

    void HandleTouch()
    {
        switch (Input.touchCount)
        {
            case 1:
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    lastPanPosition = touch.position;
                    panFingerId = touch.fingerId;
                }
                else if (touch.phase == TouchPhase.Moved && touch.fingerId == panFingerId)
                {
                    Vector3 touchPos = new Vector3(touch.position.x, touch.position.y, 0f);
                    Vector3 lastPos = new Vector3(lastPanPosition.x, lastPanPosition.y, 0f);
                    Vector3 delta = touchPos - lastPos;
                    PanCamera(delta);
                    lastPanPosition = touch.position;
                }
                break;

            case 2:
                TouchZoomCamera();
                break;
        }
    }

    public void CanPanCamera(bool pancamera)
    {
        canPan = pancamera;
    }
}
