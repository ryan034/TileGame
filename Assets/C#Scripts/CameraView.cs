using static GlobalData;
using UnityEngine;

public class CameraView : MonoBehaviour
{

    private void Start()
    {
        Camera.main.fieldOfView = cameraView;
        transform.rotation = globalRotation;
        //Plane mapPlane = new Plane(new Vector3(0, 1, 0), new Vector3(0, 0, 0));
        //Ray ray = new Ray(transform.position, transform.forward);
        /*if (mapPlane.Raycast(ray, out float enter))
        {
            //Get the point that is clicked
            Vector3 intersect = ray.GetPoint(enter);
            //Move your cube GameObject to the point where you clicked
            Vector3 move = intersect - new Vector3(0, 0, 0);
            transform.position -= move;
            //offset = transform.position;
        }*/
    }
}
