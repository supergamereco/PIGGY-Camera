using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera2 : MonoBehaviour
{
    public static Camera2 Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    [SerializeField] private float panSpeed = 10;
    public float touchZoomSpeed = 0.2f;
    [SerializeField] private float zoomSensitivity = 2;
    public float scrollZoomSpeed = 20;

    [SerializeField] private float maxZoom = 80;
    [SerializeField] private float minZoom = 45;

    [SerializeField] private float maxPanDistanceX = 10;
    [SerializeField] private float minPanDistanceX = 16;
    [SerializeField] private float maxPanDistanceY = 4;
    [SerializeField] private float minPanDistanceY = 10;

    private Vector3 lastPanPosition;
    [SerializeField] private int panFingerId;

    public bool canPan = true;
    private bool flag = false;
    private Vector3 target_position;
    private Vector3 current_position = Vector3.zero;
    private Vector3 camera_position = Vector3.zero;
    private float dragTime = 0;
    public float maxSlideTime = 1.75f;
    private bool isDrag = false;

    [SerializeField] private Vector3 cameraPosView;

    public void Enable()
    {
        this.gameObject.transform.position = cameraPosView;
        this.gameObject.GetComponent<Camera2>().enabled = true;
    }

    public void Disable()
    {
        this.gameObject.transform.position = new Vector3(0, 0, 0);
        this.gameObject.GetComponent<Camera2>().enabled = false;
    }

    void Update()
    {
        if (canPan && !Input.touchSupported)
        {
            DrageTimeCounter(isDrag);
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
                    lastPanPosition = Input.mousePosition;
                    HoldPanCamera(delta);
                    UpdateTargetPositionCamera();
                    flag = true;
                }
                else
                {
                    flag = false;
                    UpdateTargetPositionCamera();
                    HoldPanCamera(delta);
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
        Debug.Log(dragTime);
    }

    void UpdateTargetPositionCamera()
    {
        current_position.z = lastPanPosition.z = camera_position.z;

        Vector3 direction =  Camera.main.ScreenToWorldPoint(current_position) - Camera.main.ScreenToWorldPoint(lastPanPosition);
        direction = direction * -1;

        Debug.Log("direction x" + direction.x);
        Debug.Log("direction y" + direction.y);

        target_position = camera_position + direction;
    }

    void HoldPanCamera(Vector3 delta)
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

    void DrageTimeCounter(bool is_drag)
    {
        if (is_drag)
        {
            dragTime = dragTime + Time.deltaTime;
        }
    }

    void HandleTouch()
    {
        switch (Input.touchCount)
        {
            case 1:
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    isDrag = true;
                    lastPanPosition = touch.position;
                    panFingerId = touch.fingerId;
                    camera_position = transform.position;
                }
                else if (touch.phase == TouchPhase.Moved && touch.fingerId == panFingerId && dragTime >= maxSlideTime)
                {
                    Vector3 touchPos = new Vector3(touch.position.x, touch.position.y, 0f);
                    Vector3 lastPos = new Vector3(lastPanPosition.x, lastPanPosition.y, 0f);
                    Vector3 delta = touchPos - lastPos;
                    HoldPanCamera(delta);
                    lastPanPosition = touch.position;
                }
                else if (touch.phase == TouchPhase.Moved && touch.fingerId == panFingerId && dragTime < maxSlideTime)
                {
                    lastPanPosition = Input.mousePosition;
                    current_position = touch.position;
                    UpdateTargetPositionCamera();
                    flag = true;
                }
                break;

            case 2:
                TouchZoomCamera();
                break;
        }
    }
}
