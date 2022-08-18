using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Mapbox.Unity.Map;

public class RotateCamera : MonoBehaviour
{
    [SerializeField]
    private AbstractMap map;

    [SerializeField]
    private float lookSpeedH = 5f;

    [SerializeField]
    private float lookSpeedV = 5f;

    [SerializeField]
    private float zoomSpeed = 10f;

    [SerializeField]
    private float dragSpeed = 5f;

    [SerializeField]
    private float dragSpeedY = 5f;

    [SerializeField]
    private float MapZoomSpeed = 1.5f;

    [SerializeField]
    private float MapMoveSpeed = 0.001f;

    private float yaw;
    private float pitch;
    private float MapZoom;
    private double MapGeoLng;
    private double MapGeoLat;

    void Awake()
    {
        Assert.IsNotNull(map);
    }

    void Start()
    {
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;

        MapZoom = map.Zoom;
        MapGeoLng = map.CenterLatitudeLongitude.x;
        MapGeoLat = map.CenterLatitudeLongitude.y;
    }

    void Update()
    {
        KeyboardMovement();

        if (Input.GetKey(KeyCode.LeftControl))
        {
            bool updateMap = false;
            MapZoom += Input.GetAxis("Mouse ScrollWheel") * MapZoomSpeed;
            if (map.Zoom != MapZoom)
            {
                map.SetZoom(MapZoom);
                updateMap = true;

            }
            if (Input.GetMouseButton(2))
            {
                if (MapGeoLng == 0)
                    MapGeoLng = map.CenterLatitudeLongitude.x;
                if (MapGeoLat == 0)
                    MapGeoLat = map.CenterLatitudeLongitude.y;
                MapGeoLat -= Input.GetAxisRaw("Mouse X") * Time.deltaTime * MapMoveSpeed / Mathf.Pow(2, MapZoom);
                MapGeoLng -= Input.GetAxisRaw("Mouse Y") * Time.deltaTime * MapMoveSpeed / Mathf.Pow(2, MapZoom);
                if ((map.CenterLatitudeLongitude.x != MapGeoLng) || (map.CenterLatitudeLongitude.y != MapGeoLat))
                {
                    map.SetCenterLatitudeLongitude(new Mapbox.Utils.Vector2d(MapGeoLng, MapGeoLat));
                    updateMap = true;
                }
            }
            if (updateMap)
            {
                map.UpdateMap();
            }
        } else {
            MouseRotation();
        }
    }

    private void KeyboardMovement()
    {
        if (Input.GetKey(KeyCode.D)) {
            transform.Translate(new Vector3(this.dragSpeed * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.A)) {
            transform.Translate(new Vector3(-this.dragSpeed * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.S)) {
            transform.Translate(new Vector3(0, 0, -this.dragSpeed * Time.deltaTime));
        }
        if (Input.GetKey(KeyCode.W)) {
            transform.Translate(new Vector3(0, 0, this.dragSpeed * Time.deltaTime));
        }
        if (Input.GetKey(KeyCode.Q)) {
            transform.Translate(new Vector3(0, -this.dragSpeedY * Time.deltaTime, 0));
        }
        if (Input.GetKey(KeyCode.E)) {
            transform.Translate(new Vector3(0, this.dragSpeedY * Time.deltaTime, 0));
        }
    }

    private void MouseRotation()
    {
        if (Input.GetMouseButton(1)) {
            yaw += lookSpeedH * Input.GetAxis("Mouse X");
            pitch -= lookSpeedV * Input.GetAxis("Mouse Y");

            transform.eulerAngles = new Vector3(pitch, yaw, 0f);
        }

        if (Input.GetMouseButton(2)) {
            transform.Translate(-Input.GetAxisRaw("Mouse X") * Time.deltaTime * dragSpeed, -Input.GetAxisRaw("Mouse Y") * Time.deltaTime * dragSpeed, 0);
        }

        this.transform.Translate(0, 0, Input.GetAxis("Mouse ScrollWheel") * this.zoomSpeed, Space.Self);
    }
}