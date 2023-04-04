using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoaliInput : MonoBehaviour
{
    [Header("touch convert index")]
    [SerializeField]
    [Tooltip("control the zoom speed made by touch")]
    private float TOUCH_ZOOM_FACTOR = 0.01f;
    [SerializeField]
    [Tooltip("control the drag pan speed made by touch")]
    private float TOUCH_PAN_DRAG_FACTOR = 0.07f;
    [SerializeField]
    [Tooltip("control the orbit pan speed made by touch")]
    private float TOUCH_ORBIT_DRAG_FACTOR = 0.07f;

    [Header("axis trigger threshold")]
    [Tooltip("trigger axis when mouse or touch larger than the threshold below")]
    [SerializeField]
    private float TOUCH_ZOOM_THRESHOLD = 0.01f;
    [SerializeField]
    private float PAN_DRAG_AXIS_THRESHOLD = 0.01f;
    [SerializeField]
    private float ORBIT_DRAG_AXIS_THRESHOLD = 0.01f;

    private static bool[] buttonDown = new bool[3];
    private static bool[] button = new bool[3];
    private static bool[] buttonUp = new bool[3];

    /// <summary>
    /// replacement of Input.GetAxis() function,
    /// 0:panX,1:panY,2:orbitX,3:orbitY,4:zoom
    /// </summary>
    private static float[] axis = new float[5];

    /// <summary>
    /// record the first touch id, sometimes Imput.touches[] arrage will change
    /// </summary>
    public static Touch FirstTouch { get; set; }


    public static Touch Touch0
    {
        get
        {
            if(Input.touchCount > 0)
            {
                for(int i = 0; i < Input.touches.Length; i++)
                {
                    if (FirstTouch.fingerId == Input.touches[i].fingerId
                        && Input.touches[i].phase != TouchPhase.Canceled)
                        return Input.touches[i];
                }
                return Input.touches[0];
            }
            Touch t = new Touch();
            t.position = Input.mousePosition;
            return t;
        }
    }


    public static Vector2 orbitScreenPos
    {
        get
        {
            if (Input.touchCount > 0) return GetFirstTwoTouchesPos();
            else return Input.mousePosition;
        }
    }

    public static Vector2 FirstTwoScreenPos
    {
        get
        {
            if (Input.touchCount > 1) return GetFirstTwoTouchesPos();
            else if (Input.touchCount > 0) return Touch0.position;
            else return Input.mousePosition;
        }
    }

    public static int TouchCount
    {
        get
        {
            if (Input.touchCount > 0) return Input.touchCount;
            else if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)
                || Input.GetMouseButton(0) || Input.GetMouseButton(1))
                return 1;
            else return 0;
        }
    }

    public static bool IsOnTouch
    {
        get { return Input.touchCount > 0; }
    }

    public static Vector2 screenPos
    {
        get
        {
            if (Input.GetMouseButton(0)) return Input.mousePosition;
            if (Input.touchCount > 0 || GetButton(0))
            {
                return Touch0.position;
            }
            else return Input.mousePosition;
        }
    }

    private static Vector2 panScreenPosLastFrame;

    public static Vector2 panScreenPos
    {
        get
        {
            if(Input.touchCount > 0)
            {
                //准确判断一根手指转换成两根手指的时机
                if(GetButton(1)&&Input.touchCount == 2)
                {
                    panScreenPosLastFrame = FirstTwoScreenPos;
                    return FirstTwoScreenPos;
                }
                //避免两根手指转换成一根手指时的跳动
                if (GetButton(1) && Input.touchCount == 1)
                {
                    return panScreenPosLastFrame;
                }

                else return screenPos;
            }
            else
            {
                return Input.mousePosition;
            }
        }
    }

    



    private static Vector2 GetFirstTwoTouchesPos()
    {
        return (Input.GetTouch(0).position + Input.GetTouch(1).position) / 2;
    }

    bool oneFingerDown = false;
    bool oneFingerDownLastFrame = false;
    bool twoFingerDown = false;
    bool twoFingerDownLastFrame = false;

    float pinchDistanceLastFrame = 0;
    float pinchDistance = 0;
    int fingerNum;

    Vector2 panPosCurrent = Vector2.zero;
    Vector2 panPosLastFrame = Vector2.zero;
    Vector2 orbitPosCurrent = Vector2.zero;
    Vector2 orbitPosLastFrame = Vector2.zero;


    private void Awake()
    {
        Input.simulateMouseWithTouches = false;
    }

    private void Update()
    {
        oneFingerDown = Input.touchCount > 0;
        twoFingerDown = Input.touchCount >= 2;

        //update button0
        buttonDown[0] = (Input.GetMouseButtonDown(0) || (oneFingerDown && !oneFingerDownLastFrame));
        button[0] = (Input.GetMouseButton(0) || (oneFingerDown && oneFingerDownLastFrame));
        buttonUp[0] = (Input.GetMouseButtonUp(0) || (!oneFingerDown && oneFingerDownLastFrame));

        //update button1
        buttonDown[1] = (Input.GetMouseButtonDown(1) || (twoFingerDown && !twoFingerDownLastFrame));
        button[1] = (Input.GetMouseButton(1) || (twoFingerDown && twoFingerDownLastFrame));
        buttonUp[1] = (Input.GetMouseButtonUp(1) || (!twoFingerDown && twoFingerDownLastFrame));

        //update button2
        buttonDown[2] = Input.GetMouseButtonDown(2);
        button[2] = Input.GetMouseButton(2);
        buttonUp[2] = Input.GetMouseButtonUp(2);

        //transform first touch when two finger to one finger
        if((buttonDown[0]||buttonUp[1])&& IsOnTouch)
        {
            FirstTouch = Input.touches[0];
        }

        #region update zoom
        axis[4] = Input.GetAxis("Mouse ScrollWheel") != 0 ? Input.GetAxis("Mouse ScrollWheel") : 0;
        if (IsOnTouch)
        {
            if (buttonDown[1])
            {
                pinchDistanceLastFrame = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
            }
            else if (button[1])
            {
                pinchDistance = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
                float zoomFac = (pinchDistance - pinchDistanceLastFrame) * TOUCH_ZOOM_FACTOR;
                axis[4] = Mathf.Abs(zoomFac) > TOUCH_ZOOM_THRESHOLD ? zoomFac : 0;
                pinchDistanceLastFrame = pinchDistance;
            }
            else if (buttonUp[1])
            {
                pinchDistance = 0;
                pinchDistanceLastFrame = 0;
            }
        }
        #endregion

        #region update axis
        RefreshDragAxis();


        //pan axis
        if (GetButtonDown(0))
        {
            panPosCurrent = Input.touchCount > 0 ? panScreenPos : Input.mousePosition;
            fingerNum = TouchCount;
        }
        else if (GetButton(0))
        {
            if (IsOnTouch)
            {
                panPosCurrent = Input.GetTouch(0).position;
                if (!IsOnOneTwoFingerSwitch())
                {
                    if (Vector2.Distance(panPosCurrent, panPosLastFrame) > PAN_DRAG_AXIS_THRESHOLD)
                    {
                        Vector2 dir = panPosCurrent - panPosLastFrame;
                        axis[0] = dir.x * TOUCH_PAN_DRAG_FACTOR;
                        axis[1] = dir.y * TOUCH_PAN_DRAG_FACTOR;
                    }
                }
                else
                {
                    axis[0] = 0;
                    axis[1] = 0;
                }
                panPosLastFrame = panPosCurrent;
            }
            else
            {
                axis[0] = Input.GetAxis("Mouse X");
                axis[1] = Input.GetAxis("Mouse Y");
            }
        }

        //orbit axis
        if (GetButtonDown(1))
        {
            orbitPosLastFrame = IsOnTouch ? GetFirstTwoTouchesPos() : Input.mousePosition;
        }
        else if (GetButton(1))
        {
            if (IsOnTouch)
            {
                orbitPosCurrent = GetFirstTwoTouchesPos();
                if (Vector2.Distance(orbitPosCurrent, orbitPosLastFrame) > ORBIT_DRAG_AXIS_THRESHOLD)
                {
                    Vector2 dir = orbitPosCurrent - orbitPosLastFrame;
                    axis[2] = dir.x * TOUCH_ORBIT_DRAG_FACTOR;
                    axis[3] = dir.y * TOUCH_ORBIT_DRAG_FACTOR;
                }
                orbitPosLastFrame = orbitPosCurrent;
            }
            else
            {
                axis[2] = Input.GetAxis("Mouse X");
                axis[3] = Input.GetAxis("Mouse Y");
            }
        }else if (GetButtonUp(1))
        {
            orbitPosCurrent = Vector2.zero;
            orbitPosLastFrame = Vector2.zero;
        }


        #endregion

        panPosLastFrame = panPosCurrent;
        oneFingerDownLastFrame = oneFingerDown;
        twoFingerDownLastFrame = twoFingerDown;


    }

    /// <summary>
    /// panX,panY,orbitX,orbitY,zoom
    /// </summary>
    /// <param name="axisName"></param>
    /// <returns></returns>
    public static float GetAxis(string axisName)
    {
        switch (axisName)
        {
            case "panX":
                return axis[0];
            case "panY":
                return axis[1];
            case "orbitX":
                return axis[2];
            case "orbitY":
                return axis[3];
            case "zoom":
                return axis[4];
        }
        return 0;
    }

    public static bool GetButtonDown(int buttonID)
    {
        return buttonDown[buttonID];
    }

    public static bool GetButton(int buttonID)
    {
        return button[buttonID];
    }

    public static bool GetButtonUp(int buttonID)
    {
        return buttonUp[buttonID];
    }

    private static void RefreshDragAxis()
    {
        for(int i = 0; i < axis.Length - 1; i++)
        {
            axis[i] = 0;
        }
    }

    private bool IsOnOneTwoFingerSwitch()
    {
        if (fingerNum != TouchCount)
        {
            fingerNum = TouchCount;
            return true;
        }
        else return false;
    }

    public static float GetFirstTwoTouchDistance()
    {
        if (Input.touchCount == 2)
            return Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
        else
            return 1;
    }



}
