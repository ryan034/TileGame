using static Globals;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController globalInstance;
    Vector3 offset;

    void Awake()
    {
        if (globalInstance == null)
        {
            globalInstance = this;
        }
        else if (globalInstance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Camera.main.fieldOfView =25;
        transform.rotation = globalRotation;
        Plane mapPlane = new Plane(new Vector3(0, 1, 0), new Vector3(0, 0, 0));
        Ray ray = new Ray(transform.position, transform.forward);
        if (mapPlane.Raycast(ray, out float enter))
        {
            //Get the point that is clicked
            Vector3 intersect = ray.GetPoint(enter);
            //Move your cube GameObject to the point where you clicked
            Vector3 move = intersect - new Vector3(0, 0, 0);
            transform.position -= move;
            offset = transform.position;
        }
    }

    //public void UpdateCamera(Vector3 pos)
    //{
        //transform.position = pos + offset;
        /*
        if(Tilemanager.globalinstance != null)
        {
            Tilemanager.globalinstance.Refreshperspective(key);
        }
        else
        {
            Mapcanvas.globalinstance.Refreshperspective(key);
        }*/
    //}
}
