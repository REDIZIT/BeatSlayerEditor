using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraScript : MonoBehaviour
{
    Camera cam;
    public ModernEditorManager manager;
    
    public Transform center;
    public float roundSpeed;

    public float rotationSpeed = 500;
    public float zoomingSpeed = 4;

    private readonly bool isStandaloneOrEditor;


    public CameraScript()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        isStandaloneOrEditor = true;
#else
        isStandaloneOrEditor = false;
#endif
    }

    private void Start()
    {
        cam = GetComponent<Camera>();
    }
    
    bool isCamRotating;
    private void Update()
    {
        /*if (!Input.GetMouseButton(0) || EventSystem.current.IsPointerOverGameObject()) return;

        float horizontal = Input.GetAxis("Mouse X");
        float vertical = Input.GetAxis("Mouse Y");

        cam.transform.position = center.position;

        cam.transform.eulerAngles += new Vector3(-vertical * roundSpeed * Time.deltaTime, horizontal * roundSpeed * Time.deltaTime);
        cam.transform.Translate(0, 0, -20);*/
        
        
        //if (inspectorTool.swipeEditorLocker.activeSelf) return;

        //if (manager.swipeEditorLocker.activeSelf) return;
        //if (EventSystem.current.IsPointerOverGameObject() && !isCamRotating) return;
        if (manager.IsPointerOverUIObject()) return;
        

        if (isStandaloneOrEditor)
        {
            isCamRotating = Input.GetMouseButton(0);
        }
        else
        {
            isCamRotating = Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject();
        }
        if (isCamRotating)
        {
            if (isStandaloneOrEditor)
            {
                transform.RotateAround(Vector3.zero, Vector3.up, Input.GetAxis("Mouse X") * Time.deltaTime * rotationSpeed);
                transform.RotateAround(Vector3.zero, transform.right, -Input.GetAxis("Mouse Y") * Time.deltaTime * rotationSpeed);
            }
            else
            {
                transform.RotateAround(Vector3.zero, Vector3.up, Input.touches[0].deltaPosition.x * Time.deltaTime * rotationSpeed / 8f);
                transform.RotateAround(Vector3.zero, transform.right, -Input.touches[0].deltaPosition.y * Time.deltaTime * rotationSpeed / 8f);
            }
        }
    }
}
