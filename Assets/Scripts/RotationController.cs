using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class RotationController : MonoBehaviour
{
    private Sensor mSensor;

    // Logs
    public int totalLogs = 30;
    private int contLogs = 0;
    public float timebetweenLogs = 0.1f;
    private float lastTime;
    public bool logging = true;    

	// STATE
    private float _initialYAngle = 0f;
    private float _appliedGyroYAngle = 0f;
    private float _calibrationYAngle = 0f;
    private Transform _rawGyroRotation;
	private float _tempSmoothing;

	// SETTINGS
	[SerializeField] private float _smoothing = 0.1f;
	public bool useGyroIfExist = true;

	// Rotate this camera with the mobile orientation	
	public bool rotateCamera = false;
	// Move camera forward on touch
	public bool moveOnTouch = false;
	public GameObject cubeGyro;
	public GameObject cubeOriginalOrientation;
	public GameObject cubeModifiedOrientation;
	public GameObject cubeRotationVector;
	public GameObject cubeGameRotationVector;
	public GameObject cubeGeomagneticRotation;

	public float sensitivity = 10;
    public Text infoText;	
    static bool gyroBool;
    private Gyroscope gyro;    
	private Quaternion quatGyro = Quaternion.identity;

 
    void Awake()
    {
        mSensor = GetComponent<Sensor>();       

		gyroBool = SystemInfo.supportsGyroscope;
      
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        lastTime = Time.time;
   
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

        if(logging && infoText && (contLogs < totalLogs)){
        	infoText.text = "Logging";
        }
    }
 
    void Update () {
    	if(Input.touchCount > 0 && moveOnTouch){
       		transform.Translate(Vector3.forward * sensitivity);
    	}

        if (gyroBool) {
            #if UNITY_IPHONE
				quatGyro = gyro.attitude;
            #endif
            #if UNITY_ANDROID
				quatGyro = GyroToUnity(gyro.attitude);
            #endif            
        }

		if (rotateCamera) {
			if(gyroBool && useGyroIfExist)
				_rawGyroRotation.rotation = GyroToUnity(quatGyro);
			else
				_rawGyroRotation.rotation = GyroToUnity(mSensor.getQuaternion ());			
			
			// Rotate to make sense as a camera pointing out the back of your device.
			_rawGyroRotation.Rotate(90f, 180f, 0f, Space.World);
			// Save the angle around y axis for use in calibration.
			_appliedGyroYAngle = _rawGyroRotation.eulerAngles.y; 

			// Rotates y angle back however much it deviated when calibrationYAngle was saved.
			_rawGyroRotation.Rotate(0f, -_calibrationYAngle, 0f, Space.World); 

			transform.rotation = Quaternion.Slerp(transform.rotation, _rawGyroRotation.rotation,  Time.time * _smoothing);			
		}
		
		if(cubeGyro) cubeGyro.transform.localRotation = quatGyro;
		if(cubeOriginalOrientation) cubeOriginalOrientation.transform.localRotation = GyroToUnity(mSensor.getQuaternion ());
		if(cubeModifiedOrientation) cubeModifiedOrientation.transform.localRotation = GyroToUnity(mSensor.getModifiedOrientation ());
		if(cubeRotationVector) cubeRotationVector.transform.localRotation = GyroToUnity(mSensor.getRotationVector ());
		if(cubeGameRotationVector) cubeGameRotationVector.transform.localRotation = GyroToUnity(mSensor.getGameRotationVector ());
		if(cubeGeomagneticRotation) cubeGeomagneticRotation.transform.localRotation = GyroToUnity(mSensor.getGeomagneticRotation ());

		// Logging
		if(logging && infoText){
			if(Time.time - lastTime >= timebetweenLogs && contLogs < totalLogs){
				lastTime = Time.time;								
				infoText.text = infoText.text + "\n" +  Quaternion.Angle(cubeOriginalOrientation.transform.localRotation, quatGyro).ToString();
				++contLogs;
			}
		}
    }

    public void resetLogs(){
    	infoText.text = "";
		contLogs = 0;
    }

    private Quaternion quatDifference(Quaternion q1, Quaternion q2){
    	return Quaternion.Inverse(q1) * q2;
    }

    private float quatNorm(Quaternion q){
    	return (float)Math.Sqrt( q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w );
    }

    private float quatsAngle(Quaternion q1, Quaternion q2){
    	float q1dotq2 = q1.x;
    	return 1 - q1dotq2 * q1dotq2;
    }

	// Swap "handedness" of quaternion
	private static Quaternion GyroToUnity(Quaternion q)
	{
		return new Quaternion(q.x, q.y, -q.z, -q.w);
	}

	 private IEnumerator Start()
    {
        Input.gyro.enabled = true;
        Application.targetFrameRate = 60;
        _initialYAngle = transform.eulerAngles.y;

        _rawGyroRotation = transform;
        _rawGyroRotation.position = transform.position;
        _rawGyroRotation.rotation = transform.rotation;

        // Wait until gyro is active, then calibrate to reset starting rotation.
        yield return new WaitForSeconds(1);

	    StartCoroutine(CalibrateYAngle());
	}

	private IEnumerator CalibrateYAngle()
    {
        _tempSmoothing = _smoothing;
        _smoothing = 1;
        _calibrationYAngle = _appliedGyroYAngle - _initialYAngle; // Offsets the y angle in case it wasn't 0 at edit time.
        yield return null;
        _smoothing = _tempSmoothing;
	}

	public void changeSmoothing(float newSmoothing){
		_smoothing = newSmoothing;
	}

}
