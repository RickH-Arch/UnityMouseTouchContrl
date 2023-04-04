using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceInputCache : MonoBehaviour
{
    #region click
    public bool IsTouchClick { get; private set; }
    public bool IsTouchPanDrag { get; private set; }
    public bool IsTouchOrbitDrag { get; private set; }

    public Vector3 ScreenPos;
    public Vector3 ScreenPos_orbit;
    public Vector3 screenPos_zoom;

    private float zoomFactorLastFrame;

    #endregion

    #region pan drag
    private Vector3 panDragStartPos;
    private Vector3 panDragCurPos;
    private float panDragX;
    private float panDragY;
    #endregion

    #region orbit drag
    public bool IsOrbitting { get; private set; }
    private Vector3 orbitDragStartPos;
    private Vector3 orbitDragCurPos;
    private float orbitDragX;
    private float orbitDragY;
    #endregion

    #region zoom
    private float zoomFactor;
    private float zoomDistanceFactor;
    #endregion

    public void SetScreenPos(Vector2 pos)
    {
        ScreenPos = pos;
    }

    public void SetIsOrbitting(bool state)
    {
        IsOrbitting = state;
    }

    public void SetOrbitDragData(Vector2 startPos,Vector2 curPos,float xAxis,float yAxis)
    {
        orbitDragStartPos = startPos;
        orbitDragCurPos = curPos;
        orbitDragX = xAxis;
        orbitDragY = yAxis;
    }

    public void SetPanDragData(Vector2 startPos,Vector2 curPos,float xAxis,float yAxis)
    {
        panDragCurPos = curPos;
        panDragStartPos = startPos;
        panDragX = xAxis;
        panDragY = yAxis;
    }

    public void SetZoomFactor(float fac)
    {
        zoomFactor = fac;
    }

    public void SetZoomDistanceFactor(float fac)
    {
        zoomDistanceFactor = fac;
    }

    public void SetIsTouchClick(bool state)
    {
        IsTouchClick = state;
    }

    public void SetIsTouchPanDrag(bool state)
    {
        IsTouchPanDrag = state;
    }

    public void SetIsTouchOrbitDrag(bool state)
    {
        IsTouchOrbitDrag = state;
    }

    public PanDragDataCache GetPanData()
    {
        PanDragDataCache data = new PanDragDataCache();
        data.ScreenPos_pan = ScreenPos;
        data.panDragSartPos = panDragStartPos;
        data.panDragCurPos = panDragCurPos;
        data.panDragX = panDragX;
        data.panDragY = panDragY;

        return data;
    }

    /// <summary>
    /// this function will block camera pan data when camera is orbitting
    /// </summary>
    /// <param name="cam"></param>
    /// <returns></returns>
    public PanDragDataCache GetCameraPanData(Camera cam)
    {
        PanDragDataCache data = new PanDragDataCache();
        if (cam.orthographic)
        {
            data.ScreenPos_pan = ScreenPos;
            data.panDragSartPos = panDragStartPos;
            data.panDragCurPos = panDragCurPos;
            data.panDragX = panDragX;
            data.panDragY = panDragY;
        }
        else
        {
            if (IsOrbitting)
            {
                data.ScreenPos_pan = ScreenPos;
                data.panDragSartPos = Vector3.zero;
                data.panDragCurPos = Vector3.zero;
                data.panDragX = 0;
                data.panDragY = 0;
            }
            else
            {
                data.ScreenPos_pan = ScreenPos;
                data.panDragSartPos = panDragStartPos;
                data.panDragCurPos = panDragCurPos;
                data.panDragX = panDragX;
                data.panDragY = panDragY;
            }
        }
        return data;
    }

    public OrbitDragDataCache GetOrbitData()
    {
        OrbitDragDataCache data = new OrbitDragDataCache();
        data.ScreenPos_orbit = ScreenPos_orbit;
        data.orbitDragCurPos = orbitDragCurPos;
        data.orbitDragStartPos = orbitDragStartPos;
        data.orbitDragX = orbitDragX;
        data.orbitDragY = orbitDragY;
        return data;
    }

    /// <summary>
    /// this function will block orbit data when camera is orthographic
    /// </summary>
    /// <param name="cam"></param>
    /// <returns></returns>
    public OrbitDragDataCache GetCameraOrbitData(Camera cam)
    {
        OrbitDragDataCache data = new OrbitDragDataCache();
        if (cam.orthographic)
        {
            data.ScreenPos_orbit = ScreenPos_orbit;
            data.orbitDragCurPos = Vector3.zero;
            data.orbitDragStartPos = Vector3.zero;
            data.orbitDragX = 0;
            data.orbitDragY = 0;
        }
        else
        {
            data.ScreenPos_orbit = ScreenPos_orbit;
            data.orbitDragCurPos = orbitDragCurPos;
            data.orbitDragStartPos = orbitDragStartPos;
            data.orbitDragX = orbitDragX;
            data.orbitDragY = orbitDragY;
        }
        return data;
    }

    public ZoomDataCache GetZoomData()
    {
        ZoomDataCache data = new ZoomDataCache();
        data.ScreenPos_zoom = screenPos_zoom;
        data.zoomDistanceFactor = zoomDistanceFactor;
        data.zoomFactor = zoomFactor;
        return data;
    }

    /// <summary>
    /// this function filter the two finger tremble when camera is orbitting
    /// </summary>
    /// <param name="cam"></param>
    /// <returns></returns>
    public ZoomDataCache GetCameraZoomData(Camera cam)
    {
        ZoomDataCache data = new ZoomDataCache();
        if (cam.orthographic)
        {
            data.ScreenPos_zoom = screenPos_zoom;
            data.zoomDistanceFactor = zoomDistanceFactor;
            data.zoomFactor = zoomFactor;
        }
        else
        {
            if (IsOrbitting)
            {
                data.ScreenPos_zoom = screenPos_zoom;
                data.zoomDistanceFactor = zoomDistanceFactor;
                data.zoomFactor = Mathf.Abs(zoomFactor) > 0.028 ? zoomFactor : 0;
            }
            else
            {
                data.ScreenPos_zoom = screenPos_zoom;
                data.zoomDistanceFactor = zoomDistanceFactor;
                data.zoomFactor = zoomFactor;
            }
        }
        return data;
    }

    public class PanDragDataCache
    {
        public Vector3 ScreenPos_pan;
        public Vector3 panDragSartPos;
        public Vector3 panDragCurPos;
        /// <summary>
        /// same as Input.GetAxis("Mouse X"),not recommend using when touch pan drag
        /// </summary>
        public float panDragX;
        /// <summary>
        /// same as Input.GetAxis("Mouse Y"),not recommend using when touch pan drag
        /// </summary>
        public float panDragY;
    }


    public class OrbitDragDataCache
    {
        public Vector3 ScreenPos_orbit;
        public Vector3 orbitDragStartPos;
        public Vector3 orbitDragCurPos;
        /// <summary>
        /// same as Input.GetAxis("Mouse X")
        /// </summary>
        public float orbitDragX;
        /// <summary>
        /// same as Input.GetAxis("Mouse Y")
        /// </summary>
        public float orbitDragY;
    }

    public class ZoomDataCache
    {
        public Vector3 ScreenPos_zoom;
        /// <summary>
        /// ZoomIndex will be 0 when not zooming, same as Unity.Input.GetAxis("scroll wheel") while using mouse
        /// </summary>
        public float zoomFactor;
        /// <summary>
        /// Better use it when touch,
        /// ZoomDistanceFactor record the distance ratio between touch zoom start distance and touch zoom current distance,
        /// Value will be constant 1 if it is in mouse control
        /// </summary>
        public float zoomDistanceFactor;
    }

}
