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

	private Quaternion quatFromRotaionMatrix;
	private Quaternion quatOriginalOrientation;
	private Quaternion quatModifiedOrientation;
	private Quaternion quatRotationVector;
	private Quaternion quatGameRotationVector;
	private Quaternion quatGeomagneticRotation;

    void Start()
    {        
        
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
    }    

	public Quaternion getOriginalOrientation(){
		return quatOriginalOrientation;		
	}

	public Quaternion getModifiedOrientation(){
		return quatModifiedOrientation;		
	}

	public Quaternion getRotationVector(){
		return quatRotationVector;		
	}

	public Quaternion getGameRotationVector(){
		return quatGameRotationVector;		
	}

	public Quaternion getGeomagneticRotation(){
		return quatGeomagneticRotation;		
	}

	public Quaternion getQuaternionFromRotationMatrix(){
		return quatFromRotaionMatrix;
	}

    public Quaternion getQuaternion()
    {
		return normalizeQuat(quatFromRotaionMatrix);
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
