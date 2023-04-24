using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player : MonoBehaviour
{
    public string id;
    public GameObject headobj;
    public GameObject lefthand;
    public GameObject righthand;
    public Dictionary<string, float> lHand;
    public Dictionary<string, float> rHand;
    public Dictionary<string, float> head;
    public void Initialize(string PlayerId)
    {
        id = PlayerId;

        headobj = GameObject.Find("OculusInteractionSampleRig/OVRCameraRig/TrackingSpace/CenterEyeAnchor");
        lefthand = GameObject.Find("OculusInteractionSampleRig/OVRCameraRig/TrackingSpace/LeftHandAnchor");
        righthand = GameObject.Find("OculusInteractionSampleRig/OVRCameraRig/TrackingSpace/RightHandAnchor");

        lHand = new Dictionary<string, float>
        {
            {"posX", 0f},
            {"posY", 0f},
            {"posZ", 0f},
            {"rotX", 0f},
            {"rotY", 0f},
            {"rotZ", 0f},
            {"rotW", 0f}
        };
        rHand = new Dictionary<string, float>
        {
            {"posX", 0f},
            {"posY", 0f},
            {"posZ", 0f},
            {"rotX", 0f},
            {"rotY", 0f},
            {"rotZ", 0f},
            {"rotW", 0f}
        };
        head = new Dictionary<string, float>
        {
            {"posX", 0f},
            {"posY", 0f},
            {"posZ", 0f},
            {"rotX", 0f},
            {"rotY", 0f},
            {"rotZ", 0f},
            {"rotW", 0f}
        };
    }
    public void UpdatePosition(Dictionary<string, float> lHandData, Dictionary<string, float> rHandData, Dictionary<string, float> headData)
    {
        // Update the position and rotation of head, lHand, and rHand using the received data
        headobj.transform.position = new Vector3(headData["posX"], headData["posY"], headData["posZ"]);
        headobj.transform.rotation = new Quaternion(headData["rotX"], headData["rotY"], headData["rotZ"], headData["rotW"]);

        lefthand.transform.position = new Vector3(lHandData["posX"], lHandData["posY"], lHandData["posZ"]);
        lefthand.transform.rotation = new Quaternion(lHandData["rotX"], lHandData["rotY"], lHandData["rotZ"], lHandData["rotW"]);

        righthand.transform.position = new Vector3(rHandData["posX"], rHandData["posY"], rHandData["posZ"]);
        righthand.transform.rotation = new Quaternion(rHandData["rotX"], rHandData["rotY"], rHandData["rotZ"], rHandData["rotW"]);
    }

}
