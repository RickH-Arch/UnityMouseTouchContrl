using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// this class wrap mouse input and touch input together,
/// and translate to click\double click\long tap\pan drag\orbit drag
/// </summary>
public class MouseTouchInputController : MonoBehaviour
{
    [SerializeField, Range(50, 200)]
    private int ONE_CLICK_THRESHOLD = 180;

    [SerializeField, Range(200, 800)]
    private int DOUBLE_CLICK_THRESHOLD = 500;

    [SerializeField, Range(300, 1000)]
    private int LONG_TAP_TIME_THRESHOLD = 500;

    [SerializeField, Range(0.01f, 0.5f)]
    private float LONG_TAP_DISTANCE_THRESHOLD = 0.1f;

    [SerializeField, Range(0.01f, 0.1f)]
    private float PAN_DRAG_START_THRESHOLD = 0.01f;

    [SerializeField, Range(0.01f, 0.1f)]
    private float ORBIT_DRAG_START_THRESHOLD = 0.05f;

    System.Diagnostics.Stopwatch clickWatch = new System.Diagnostics.Stopwatch();
    System.Diagnostics.Stopwatch longTapWatch = new System.Diagnostics.Stopwatch();

    DeviceInputCache deviceInputCache = new DeviceInputCache();

    Vector2 panCorrectOffset = Vector2.zero;
    private int panFirstFingerNum; 
    private bool fingerOne2Two = false;
    private bool fingerTwo20ne = false;

    private void Awake()
    {
        Input.simulateMouseWithTouches = false;
        if (ONE_CLICK_THRESHOLD > DOUBLE_CLICK_THRESHOLD) DOUBLE_CLICK_THRESHOLD = ONE_CLICK_THRESHOLD;
        clickWatch.Stop();
    }

    bool firstClicked = false;
    bool longTapDown = false;
    bool isPanDragging = false;
    bool isZooming = false;
    float zoomStartDistance;
    bool panDragStartMove = false;
    bool isOrbitDragging = false;
    bool orbitDragMoveStart = false;
    private Vector2 orbitStartPos;
    private Vector2 panStartPos;
    /// <summary>
    /// 拖动事件核心数值，该值能够保证任意姿势下拖动切换时基准点在同一点
    /// </summary>
    private Vector2 panCurPos;
    private Vector2 orbitCurPos;
    private bool isTouchControl;
    private float zoomCurDistance;
    Vector2 twoFingerPosLastFrame;

    private void Update()
    {
        #region click/double click/longtap control
        if (GoaliInput.GetButtonDown(0))
        {
            isTouchControl = GoaliInput.IsOnTouch;
            longTapWatch.Start();
            deviceInputCache.SetScreenPos(GoaliInput.screenPos);
            //if is first click down
            if (!clickWatch.IsRunning)
            {
                clickWatch.Start();
            }
        }
        else if (GoaliInput.GetButtonUp(0))
        {
            longTapWatch.Reset();
            if (longTapDown)
            {
                longTapDown = false;
                LongTapOnStop();
            }

            //check if it is a first click up
            if(clickWatch.IsRunning && clickWatch.ElapsedMilliseconds < ONE_CLICK_THRESHOLD && !firstClicked)
            {
                click();
                firstClicked = true;
            }
            //check if it is a second click up
            else if(clickWatch.IsRunning && clickWatch.ElapsedMilliseconds < DOUBLE_CLICK_THRESHOLD && firstClicked)
            {
                DoubleClick();
                firstClicked = false;
                clickWatch.Reset();
            }
        }

        //reset double click watch after a period of time
        if(clickWatch.ElapsedMilliseconds >= DOUBLE_CLICK_THRESHOLD)
        {
            clickWatch.Reset();
            firstClicked = false;
        }

        //active longTap if longTapWatch reach the threshold
        if(!longTapDown && longTapWatch.ElapsedMilliseconds > LONG_TAP_TIME_THRESHOLD)
        {
            if (Vector3.Distance(deviceInputCache.ScreenPos, GoaliInput.screenPos) < LONG_TAP_DISTANCE_THRESHOLD)
            {
                longTapDown = true;
                LongTapOnStart();
            }
        }

        #endregion

        #region pan drag control
        if (!isPanDragging)
        {
            if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
            {
                deviceInputCache.SetIsTouchPanDrag(GoaliInput.IsOnTouch);
                panStartPos = GoaliInput.panScreenPos;
                isPanDragging = true;
                panCorrectOffset = Vector2.zero;
                fingerOne2Two = false;
                fingerTwo20ne = false;
                PanDragOnStart();
            }
        }
        else
        {
            if((Input.touchCount == 0 && deviceInputCache.IsTouchPanDrag) || Input.GetMouseButtonUp(0))
            {
                isPanDragging = false;
                panDragStartMove = false;
                PanDragOnStop();
            }
            else
            {
                if (GoaliInput.IsOnTouch)
                {
                    AdjustPanCorrectOffset();
                }

                panCurPos = GoaliInput.panScreenPos + panCorrectOffset;

                if (!panDragStartMove)
                {
                    if (Vector2.Distance(panCurPos, panStartPos) > PAN_DRAG_START_THRESHOLD)
                    {
                        panDragStartMove = true;
                        panFirstFingerNum = GoaliInput.TouchCount;
                        PanDrag();
                    }
                }
                else
                {
                    PanDrag();
                }
            }
        }

        #endregion

        #region orbit drag control
        if (!isOrbitDragging)
        {
            if (GoaliInput.GetButtonDown(1))
            {
                deviceInputCache.SetIsTouchOrbitDrag(GoaliInput.IsOnTouch);
                isOrbitDragging = true;
                orbitStartPos = GoaliInput.FirstTwoScreenPos;
                OrbitDragOnStart();
            }
        }
        else
        {
            if((Input.touchCount != 2 && deviceInputCache.IsTouchOrbitDrag) || Input.GetMouseButtonUp(1))
            {
                isOrbitDragging = false;
                orbitDragMoveStart = false;
                deviceInputCache.SetIsOrbitting(false);
                OrbitDragOnStop();
            }
            else
            {

                orbitCurPos = GoaliInput.FirstTwoScreenPos;
                if (!orbitDragMoveStart)
                {
                    if (Vector2.Distance(orbitStartPos, orbitCurPos) > ORBIT_DRAG_START_THRESHOLD)
                    {
                        orbitDragMoveStart = true;
                        deviceInputCache.SetIsOrbitting(true);
                        OrbitDrag();
                    }
                }
                else
                {
                    OrbitDrag();
                }
            }
        }
        #endregion

        #region zoom control
        if (!isZooming)
        {
            if (GoaliInput.GetButtonDown(1))
            {
                isZooming = true;
                zoomStartDistance = GoaliInput.GetFirstTwoTouchDistance();
                ZoomOnStart();
            }
        }
        else
        {
            if(Input.touchCount < 2 || GoaliInput.GetButtonUp(1))
            {
                isZooming = false;
                zoomStartDistance = 0;
                zoomCurDistance = 0;
                ZoomOnStop();
            }
            else
            {
                zoomCurDistance = GoaliInput.GetFirstTwoTouchDistance();
                deviceInputCache.SetZoomDistanceFactor(zoomStartDistance / Mathf.Max(zoomCurDistance, 0.0001f));
                Zoom();
            }
        }
        if(Input.GetAxis("Mouse ScrollWheel")!= 0)
        {
            Zoom();
        }
        #endregion
    }

    private void AdjustPanCorrectOffset()
    {
        if(panFirstFingerNum == 1)
        {
            if(GoaliInput.GetButton(1) && fingerOne2Two == false)
            {
                fingerOne2Two = true;
                Vector2 oneFingerCurPos = GoaliInput.screenPos;
                Vector2 twoFingerCurPos = GoaliInput.FirstTwoScreenPos;
                panCorrectOffset += (oneFingerCurPos - twoFingerCurPos);
            }

            if(fingerOne2Two && GoaliInput.GetButtonUp(1))
            {
                fingerOne2Two = false;
                Vector2 oneFingerCurPos = GoaliInput.screenPos;
                panCorrectOffset += (twoFingerPosLastFrame - oneFingerCurPos);
            }
            if(fingerOne2Two && GoaliInput.TouchCount > 1)
            {
                twoFingerPosLastFrame = GoaliInput.FirstTwoScreenPos;
            }
        }

        if(panFirstFingerNum >= 2)
        {
            if(fingerTwo20ne == false && GoaliInput.TouchCount > 1)
            {
                twoFingerPosLastFrame = GoaliInput.FirstTwoScreenPos;
            }

            if(fingerTwo20ne == false && GoaliInput.GetButtonUp(1))
            {
                fingerTwo20ne = true;
                Vector2 oneFingerCurPos = GoaliInput.screenPos;
                panCorrectOffset += (twoFingerPosLastFrame - oneFingerCurPos);
            }

            if(fingerTwo20ne == true && GoaliInput.GetButton(1))
            {
                fingerTwo20ne = false;
                Vector2 oneFingerCurPos = GoaliInput.screenPos;
                Vector2 twoFingerCurPos = GoaliInput.FirstTwoScreenPos;
                panCorrectOffset += (oneFingerCurPos - twoFingerCurPos);
            }
        }
    }


    //////////////////////////////////////////////////////////////////////////////////

    private void click()
    {
        Debug.Log("trigger click");
        deviceInputCache.SetScreenPos(GoaliInput.screenPos);
        deviceInputCache.SetIsTouchClick(isTouchControl);
        UICmdMessage msg = new UICmdMessage(UICmd.Input_click);
        msg.Sender = deviceInputCache;
        UIEventManager.TriggerEvent(msg);
    }

    private void DoubleClick()
    {
        Debug.Log("trigger double click");
        deviceInputCache.SetScreenPos(GoaliInput.screenPos);
        UICmdMessage msg = new UICmdMessage(UICmd.Input_doubleClick);
        msg.Sender = deviceInputCache;
        UIEventManager.TriggerEvent(msg);
    }

    private void LongTapOnStart()
    {
        Debug.Log("trigger longTapStart");
        deviceInputCache.SetScreenPos(GoaliInput.screenPos);
        UICmdMessage msg = new UICmdMessage(UICmd.Input_longTapStart);
        msg.Sender = deviceInputCache;
        UIEventManager.TriggerEvent(msg);
    }

    private void LongTapOnStop()
    {
        Debug.Log("trigger longTapStop");
        deviceInputCache.SetScreenPos(GoaliInput.screenPos);
        UICmdMessage msg = new UICmdMessage(UICmd.Input_longTapStop);
        msg.Sender = deviceInputCache;
        UIEventManager.TriggerEvent(msg);
    }

    private void PanDrag()
    {
        Debug.Log("trigger panDrag");
        deviceInputCache.SetPanDragData(panStartPos, panCurPos, GoaliInput.GetAxis("panX"), GoaliInput.GetAxis("panY"));
        UICmdMessage msg = new UICmdMessage(UICmd.Input_pan);
        msg.Sender = deviceInputCache;
        UIEventManager.TriggerEvent(msg);
    }

    private void PanDragOnStart()
    {
        Debug.Log("trigger panDragStart");
        deviceInputCache.SetScreenPos(GoaliInput.screenPos);
        UICmdMessage msg = new UICmdMessage(UICmd.Input_panStart);
        msg.Sender = deviceInputCache;
        UIEventManager.TriggerEvent(msg);
    }

    private void PanDragOnStop()
    {
        Debug.Log("trigger panDragStop");
        UIEventManager.TriggerEvent(new UICmdMessage(UICmd.Input_panStop));
    }

    private void OrbitDrag()
    {
        Debug.Log("trigger orbitDrag");
        deviceInputCache.SetOrbitDragData(orbitStartPos, orbitCurPos, GoaliInput.GetAxis("orbitX"), GoaliInput.GetAxis("orbitY"));
        UICmdMessage msg = new UICmdMessage(UICmd.Input_orbit);
        msg.Sender = deviceInputCache;
        UIEventManager.TriggerEvent(msg);
    }

    private void OrbitDragOnStart()
    {
        Debug.Log("trigger orbitDragStart");
        deviceInputCache.ScreenPos_orbit = GoaliInput.FirstTwoScreenPos;
        UICmdMessage msg = new UICmdMessage(UICmd.Input_orbitStart);
        msg.Sender = deviceInputCache;
        UIEventManager.TriggerEvent(msg);
    }

    private void OrbitDragOnStop()
    {
        Debug.Log("trigger orbitDragStop");
        UIEventManager.TriggerEvent(new UICmdMessage(UICmd.Input_orbitStop));
    }

    private void Zoom()
    {
        Debug.Log("trigger zoom");
        deviceInputCache.SetZoomFactor(GoaliInput.GetAxis("zoom"));
        deviceInputCache.screenPos_zoom = GoaliInput.FirstTwoScreenPos;
        if(Input.GetAxis("Mouse ScrollWheel")!= 0)
        {
            deviceInputCache.SetZoomFactor(Input.GetAxis("Mouse ScrollWheel"));
            deviceInputCache.SetScreenPos(Input.mousePosition);
        }
        UICmdMessage msg = new UICmdMessage(UICmd.Input_zoom);
        msg.Sender = deviceInputCache;
        UIEventManager.TriggerEvent(msg);
    }

    private void ZoomOnStart()
    {
        Debug.Log("trigger zoomStart");
        UIEventManager.TriggerEvent(new UICmdMessage(UICmd.Input_zoomStart));
    }

    private void ZoomOnStop()
    {
        Debug.Log("trigger zoomStop");
        UIEventManager.TriggerEvent(new UICmdMessage(UICmd.Input_zoomStop));
    }
}
