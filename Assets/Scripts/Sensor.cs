using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class Sensor : MonoBehaviour
{
    public Text infoText;

    private AndroidJavaObject pluginObject = null;
    private AndroidJavaObject activityContext = null;
    private float alpha = 0, beta = 0, gamma = 0;

    private Vector3 mOrientation;    

	private Quaternion quatFromRotaionMatrix;
	private Quaternion quatOriginalOrientation;
	private Quaternion quatModifiedOrientation;
	private Quaternion quatRotationVector;
	private Quaternion quatGameRotationVector;
	private Quaternion quatGeomagneticRotation;

    void Start()
    {
        mOrientation = new Vector3(0, 0, 0);
        
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                activityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
            }

			using (AndroidJavaClass pluginClass = new AndroidJavaClass("com.example.androidsensorplugin.AndroidPlugin"))
            {
                if (pluginClass != null)
                {
                    pluginObject = pluginClass.CallStatic<AndroidJavaObject>("getInstance");
					pluginObject.Call ("setContext", activityContext);
					if(infoText)
						infoText.text = "" + pluginObject.Call<string>("getPluginName");
                }
            }

        }
    }

    void Update()
    {
        if (pluginObject == null)
            return;

		// Quaternion from Rotation Matrix
		quatFromRotaionMatrix.Set(pluginObject.Call<float>("getQuatX"),
								pluginObject.Call<float>("getQuatY"),
								pluginObject.Call<float>("getQuatZ"),
								pluginObject.Call<float>("getQuatW"));

		// Original Orientation
		alpha = pluginObject.Call<float>("getAzimutOriginal"); // Alfa
		beta = pluginObject.Call<float>("getPitchOriginal"); // beta
		gamma = pluginObject.Call<float>("getRollOriginal"); // Gamma

		quatOriginalOrientation.Set(alpha, beta, gamma, 0);

		// Modified Orientation
		alpha = pluginObject.Call<float>("getAzimut"); // Alfa
		beta = pluginObject.Call<float>("getPitch"); // beta
		gamma = pluginObject.Call<float>("getRoll"); // Gamma

		quatModifiedOrientation.Set(alpha, beta, gamma, 0);

		// Rotation Vector
		alpha = pluginObject.Call<float>("getAzimutRotation"); // Alfa
		beta = pluginObject.Call<float>("getPitchRotation"); // beta
		gamma = pluginObject.Call<float>("getRollRotation"); // Gamma

		quatRotationVector.Set(alpha, beta, gamma, 0);

		// Game Rotation
		alpha = pluginObject.Call<float>("getAzimutGameRotation"); // Alfa
		beta = pluginObject.Call<float>("getPitchGameRotation"); // beta
		gamma = pluginObject.Call<float>("getRollGameRotation"); // Gamma

		quatGameRotationVector.Set(alpha, beta, gamma, 0);

		// Geomagnetic Rotation
		alpha = pluginObject.Call<float>("getAzimutGeomagnetic"); // Alfa
		beta = pluginObject.Call<float>("getPitchGeomagnetic"); // beta
		gamma = pluginObject.Call<float>("getRollGeomagnetic"); // Gamma

		quatGeomagneticRotation.Set(alpha, beta, gamma, 0);

      //  mOrientation.Set((float)(alpha * 180 / Math.PI),(float) (beta * 180 / Math.PI),(float) (gamma * 180 / Math.PI));

//        infoText.text = "alpha: " + alpha * 180 / Math.PI + "\n" +
//            "beta: " + beta * 180 / Math.PI+ "\n" +
//            "gamma: " + gamma * 180 / Math.PI;
    }    

	public Quaternion getOriginalOrientation(){
		//return quatOriginalOrientation;
		return getQuaternion(quatOriginalOrientation.x, quatOriginalOrientation.y, quatOriginalOrientation.z);
	}

	public Quaternion getModifiedOrientation(){
		//return quatModifiedOrientation;
		return getQuaternion(quatModifiedOrientation.x, quatModifiedOrientation.y, quatModifiedOrientation.z);
	}

	public Quaternion getRotationVector(){
		//return quatRotationVector;
		return getQuaternion(quatRotationVector.x, quatRotationVector.y, quatRotationVector.z);
	}

	public Quaternion getGameRotationVector(){
		//return quatGameRotationVector;
		return getQuaternion(quatGameRotationVector.x, quatGameRotationVector.y, quatGameRotationVector.z);
	}

	public Quaternion getGeomagneticRotation(){
		//return quatGeomagneticRotation;
		return getQuaternion(quatGeomagneticRotation.x, quatGeomagneticRotation.y, quatGeomagneticRotation.z);
	}

	public Quaternion getQuaternionFromRotationMatrix(){
		return quatFromRotaionMatrix;
	}

    public Quaternion getQuaternion()
    {
//        return new Quaternion(0, beta, gamma, alpha);
		return normalizeQuat(quatFromRotaionMatrix);
//        return getQuaternion(beta, gamma, alpha);
    }

    public Quaternion getQuaternion(float x, float y, float z)
    {
        float degToRad = (float)Math.PI / 180;

        float _x, _y, _z;
        float _x_2, _y_2, _z_2;
        float cX, cY, cZ, sX, sY, sZ;

//        _z = z * degToRad;
//        _x = x * degToRad;
//        _y = y * degToRad;

        _z = z;
        _x = x;
        _y = y;

        _z_2 = _z / 2;
        _x_2 = _x / 2;
        _y_2 = _y / 2;

        cX = (float)Math.Cos(_x_2);
        cY = (float)Math.Cos(_y_2);
        cZ = (float)Math.Cos(_z_2);
        sX = (float)Math.Sin(_x_2);
        sY = (float)Math.Sin(_y_2);
        sZ = (float)Math.Sin(_z_2);

        Quaternion quaternion = new Quaternion(
            sX * cY * cZ - cX * sY * sZ, // X
            cX * sY * cZ + sX * cY * sZ, // Y
            cX * cY * sZ + sX * sY * cZ, // Z
            cX * cY * cZ - sX * sY * sZ  // W
            );

        return quaternion;
        return normalizeQuat(quaternion);
    }

    private Quaternion normalizeQuat(Quaternion quaternion)
    {
        float len = (float)Math.Sqrt( quaternion.x * quaternion.x + quaternion.y * quaternion.y + quaternion.z * quaternion.z + quaternion.w * quaternion.w );

			if ( len == 0 ) {

				quaternion.x = 0;
				quaternion.y = 0;
				quaternion.z = 0;
				quaternion.w = 1;

			} else {

				len = 1 / len;

				quaternion.x *= len;
				quaternion.y *= len;
				quaternion.z *= len;
				quaternion.w *= len;

			}

			return quaternion;
    }

}
