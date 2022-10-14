using UnityEngine;

public partial class FollowTarget : MonoBehaviour
{
    public bool LookAt;
    public bool MatchPos;
    public bool RollModel;

    //public MeshFilter boundingBox;
    public Transform Player;
    public Transform Model;
    public Transform target;

    public Vector3 difference;
    public Vector3 lastPos;
    public float deadband = 10;
    public float lastAngle;

    public Quaternion rotation;


    public float smoothTime = 0.3F;
    private Vector3 velocity = Vector3.zero;

    public int interpolationFramesCount = 30; 
    int elapsedFrames = 0;

    Rigidbody rb;

    Rigidbody RB
    { 
        get {
            if (rb == null)
            { rb = this.GetComponent<Rigidbody>(); }
            return rb; }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (MatchPos)
        {
            MatchPosOfTarget(target);
        }

        if (LookAt)
        {
            LookAtTarget(target);
        }
    }

    private void MatchPosOfTarget(Transform target)
    {
        //float value = (RB.rotation.eulerAngles.z + (Input.GetAxis("Yaw") * boundsTarget.SpeedModifier));
        //Debug.Log(value);
        //rb.rotation.ToAngleAxis(out value, out rotation);
        //rb.rotation = Quaternion.AngleAxis(value, axis);

        //acceleration.text = Input.gyro.attitude.ToString();

        Debug.DrawRay(this.transform.position, this.transform.forward * 10, Color.green);

        Vector3 targetPosition = new Vector3(0, target.position.y, target.position.z);
        if (targetPosition != Vector3.zero)
        {
            //Try not setting position directly. 
            Player.transform.position = Vector3.SmoothDamp(Player.position, targetPosition, ref velocity, smoothTime);
        }
    }

    private void LookAtTarget(Transform target)
    {
        lastPos = difference;
        difference = target.position - Player.transform.position;
        rotation = Quaternion.LookRotation(difference);

        Player.transform.rotation = rotation;

        if (RollModel)
        {
            Roll();
        }
    }


    private void Roll()
    {
        //Model.RotateAround(target.transform.position, Vector3.right, Input.GetAxis("Roll") * -15);
        Debug.Log(difference.z + " : " + target.position.z  + " : " + Player.position.z);
        if (Input.GetButton("Roll") && Mathf.Abs(difference.z) > Mathf.Abs(lastPos.z))
        {
            int maxLeftAngle = 45; 
            int maxRightAngle = 315;

            float minScaler = 0.1f;
            float maxScaler = 0.5f;

            //Debug.Log("Roll");
            float input = Input.GetAxis("Roll");
            float rotation = 0;

            //Debug.Log(Model.localRotation.eulerAngles.z);

            if (Model.localRotation.eulerAngles.z < maxLeftAngle)
            {
                Debug.Log("Left");
                if ((int)Mathf.Sign(input) == -1)
                {
                    rotation = maxScaler + maxScaler;
                Debug.Log("Cross Right");
                }
                else
                {
                    rotation = Mathf.Lerp(maxScaler, minScaler, Mathf.InverseLerp(0, maxLeftAngle, Model.localRotation.eulerAngles.z));
                }
            }
            else if (Model.localRotation.eulerAngles.z > maxRightAngle)
            {
                Debug.Log("Right");
                if ((int)Mathf.Sign(input) == 1)
                {
                    rotation = maxScaler + maxScaler;
                Debug.Log("Cross Left");
                }
                else
                {
                    rotation = Mathf.Lerp(maxScaler, minScaler, Mathf.InverseLerp(360, maxRightAngle, Model.localRotation.eulerAngles.z));
                }
            }
            

            //Debug.Log(rotation);

            if (input != 0)
            {
                Model.Rotate(0, 0, rotation * input);
                lastAngle = Model.localRotation.eulerAngles.z;
                //Debug.Log("Roll : " + Mathf.Round(lastAngle * 100));
            }
        }
        else
        {
            //Debug.Log(Model.localRotation.eulerAngles.z);

            if (Model.localRotation.eulerAngles.z < 1 && Model.localRotation.eulerAngles.z > -1)
            {
                //Debug.Log("Zero");
                Model.localRotation = Quaternion.Euler(Vector3.zero);
                elapsedFrames = 0;
            }
            else
            {
                //Debug.Log("Scale");

                //Debug.Log(Model.localRotation.eulerAngles.z);
                //float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;

                //Vector3 interpolatedRotation = Vector3.Lerp( Vector3.zero, -lastRotation, interpolationRatio);

                //elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1);  // reset elapsedFrames to zero after it reached (interpolationFramesCount + 1)

                //Model.Rotate(interpolatedRotation);


                //float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;

                //float z = Mathf.Lerp(lastRotation.z, 0, interpolationRatio);

                //elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1);  // reset elapsedFrames to zero /after /it reached (interpolationFramesCount + 1)

                //Model.Rotate(new Vector3(0, 0, z));

                int direction = (int)Mathf.Sign(lastAngle);

                float rot = 0;
                float scalar;


                if (lastAngle > 180)
                {
                    lastAngle -= 360;
                }


                //Debug.Log(direction);

                if (direction > 0)
                {
                    //Debug.Log("Straighten Rightways : " + Mathf.Round(lastAngle * 100)/100);

                    scalar = Mathf.Lerp(0.5f, 9, Mathf.InverseLerp(0, 179, lastAngle));
                    lastAngle -= scalar;
                    rot = lastAngle;
                }
                else
                {
                    //Debug.Log("Straighten Leftways : " + Mathf.Round(lastAngle * 100)/100);

                    scalar = Mathf.Lerp(0.5f, 9, Mathf.InverseLerp(0, -179, lastAngle));
                    lastAngle += scalar;
                    rot = lastAngle;
                }


                Model.localRotation = Quaternion.Euler(new Vector3(0, 0, rot));

                //Model.Rotate(new Vector3(0, 0, -rot));
            }
        }
    }

   

    bool ColliderContainsPoint(Transform ColliderTransform, Vector3 Point, bool Enabled)
    {
        Vector3 localPos = ColliderTransform.InverseTransformPoint(Point);
        if (Enabled && Mathf.Abs(localPos.x) < 1f && Mathf.Abs(localPos.y) < 1f && Mathf.Abs(localPos.z) < 1f)
            return true;
        else
            return false;
    }

    Bounds GetMaxBounds(GameObject g)
    {
        var b = new Bounds(g.transform.position, Vector3.forward);
        foreach (Renderer r in g.GetComponentsInChildren<Renderer>())
        {
            b.Encapsulate(r.bounds);
        }
        return b;
    }
}
