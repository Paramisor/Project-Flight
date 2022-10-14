using UnityEngine;

public class MoveWithinBounds : MonoBehaviour
{
    public float roll;
    public float pitch;
    public float yaw;

    public Bounds bounds;
    public MeshRenderer boundingBox;

    public Transform target;

    public bool debugging;

    [Range(0.1f, 20f)]
    public float SpeedModifier = 1;


    void Awake()
    {
        //Debug.Log(Application.platform);
        if (!Input.gyro.enabled)
        {
            Input.gyro.enabled = true;
        }
        bounds = boundingBox.bounds; // = GetMaxBounds(boundingBox.gameObject);
        boundingBox.GetComponent<MeshRenderer>().enabled = debugging;
    }

    void FixedUpdate()
    { 
        MoveTarget();

    }

    public void MoveTarget()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            roll = Input.GetAxis("Roll");
            pitch = Input.GetAxis("Pitch");
            yaw = Input.GetAxis("Yaw");
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            roll = Input.acceleration.x * 2;
            pitch = Input.acceleration.z;
            yaw = Input.acceleration.y;
        }

        //Debug.Log(Input.gyro.attitude);

        if (roll != 0 || pitch != 0 || yaw != 0)
        {

            bounds = boundingBox.bounds;
            Vector3 position = new Vector3(0f, pitch * SpeedModifier, roll * SpeedModifier);
            Vector3 newPosition = target.transform.position + position;
            if (bounds.Contains(newPosition))
            {
                target.transform.position = newPosition;
            }
            else
            {
                target.transform.position = bounds.ClosestPoint(newPosition);
                //Debug.Log("Out of Bounds" + " : " + focus.transform.position);
            }
        }
    }
}
