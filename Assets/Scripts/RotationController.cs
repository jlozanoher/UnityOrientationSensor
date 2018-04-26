using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class RotationController : MonoBehaviour
{
    private Sensor mSensor;
    private Quaternion worldQuat = new Quaternion((float)-Math.Sqrt(0.5), 0f, 0f, (float)Math.Sqrt(0.5));
    private int mWorldChoice = 0;
	private int orientationAlgorithm;

	public bool rotateCamera = false;
	public GameObject cubeGyro;
	public GameObject cubeOriginalOrientation;
	public GameObject cubeModifiedOrientation;
	public GameObject cubeRotationVector;
	public GameObject cubeGameRotationVector;
	public GameObject cubeGeomagneticRotation;

    public Text infoText;
	public Text quatText;
    static bool gyroBool;
    private Gyroscope gyro;
    private Quaternion quatMult;
	private Quaternion quatGyro = new Quaternion(0, 0, 0, 0);
    // camera grandparent node to rotate heading
    private GameObject camGrandparent;
    private float heading = 0;
 
    // mouse/touch input
    public bool touchRotatesHeading = true;
    private Vector2 screenSize;
    private Vector2 mouseStartPoint;
    private float headingAtTouchStart;
    //@script AddComponentMenu ("stereoskopix/s3d Gyro Cam")
 
    void Awake()
    {
        mSensor = GetComponent<Sensor>();       

		gyroBool = SystemInfo.supportsGyroscope;
      
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
   
        if (gyroBool) {
            gyro = Input.gyro;
            gyro.enabled = true;
            
        } else {
            #if UNITY_EDITOR
                print("NO GYRO");
            #endif
            if(infoText)
				infoText.text = infoText.text + "NO GYRO\n";
        }
    }
 
    void Update () {
        if (gyroBool) {
            #if UNITY_IPHONE
				quatGyro = gyro.attitude;
            #endif
            #if UNITY_ANDROID
				quatGyro = GyroToUnity(gyro.attitude);
            #endif
            //transform.localRotation = quatMap * quatMult;
        }
        else
        {
			
//            transform.localRotation = worldQuat * mSensor.getQuaternion();
          /*  transform.localRotation = mSensor.getQuaternion() * quatMult;
            
			quatText.text = "worldChice: " + mWorldChoice
                + "\nworldQuat * mSensor.getQuaternion(): " + worldQuat * mSensor.getQuaternion()
                + "\nmSensor.getQuaternion() * quatMult: " + mSensor.getQuaternion() * quatMult;*/
        }
		if (Screen.orientation == ScreenOrientation.LandscapeLeft)
		{
			quatMult = new Quaternion(0, 0, 0.7071f, -0.7071f);
		}
		else if (Screen.orientation == ScreenOrientation.LandscapeRight)
		{
			quatMult = new Quaternion(0, 0, -0.7071f, -0.7071f);
		}
		else if (Screen.orientation == ScreenOrientation.Portrait)
		{
			quatMult = new Quaternion(0, 0, 0, 1);
		}
		else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown)
		{
			quatMult = new Quaternion(0, 0, 1, 0);
		}

		if (rotateCamera) {
			transform.localRotation = GyroToUnity(mSensor.getQuaternionFromRotationMatrix ());
		}

		// cubeGyro.transform.localRotation = quatGyro * quatMult;
		if(cubeGyro) cubeGyro.transform.localRotation = quatGyro;
		if(cubeOriginalOrientation) cubeOriginalOrientation.transform.localRotation = GyroToUnity(mSensor.getQuaternionFromRotationMatrix ());
		if(cubeModifiedOrientation) cubeModifiedOrientation.transform.localRotation = GyroToUnity(mSensor.getModifiedOrientation ());
		if(cubeRotationVector) cubeRotationVector.transform.localRotation = GyroToUnity(mSensor.getRotationVector ());
		if(cubeGameRotationVector) cubeGameRotationVector.transform.localRotation = GyroToUnity(mSensor.getGameRotationVector ());
		if(cubeGeomagneticRotation) cubeGeomagneticRotation.transform.localRotation = GyroToUnity(mSensor.getGeomagneticRotation ());

		
    }

	private static Quaternion GyroToUnity(Quaternion q)
	{
		return new Quaternion(q.x, q.y, -q.z, -q.w);
	}

//    void Start()
//    {
//        Input.compensateSensors = true;
//        Input.gyro.enabled = true;
//    }
//
//    void FixedUpdate()
//    {
//        transform.Rotate(0, -Input.gyro.rotationRateUnbiased.y, 0);
//    }

}
