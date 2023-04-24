using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVR : MonoBehaviour
{
    public void UpdateTransforms(Player playerData)
    {
        // Assuming your player prefab has child objects for head, left hand, and right hand.
        Transform head = transform.Find("OculusInteractionSampleRig/OVRCameraRig/TrackingSpace/CenterEyeAnchor");
        Transform leftHand = transform.Find("OculusInteractionSampleRig/OVRCameraRig/TrackingSpace/LeftHandAnchor");
        Transform rightHand = transform.Find("OculusInteractionSampleRig/OVRCameraRig/TrackingSpace/RightHandAnchor");

        head.position = new Vector3(playerData.head["posX"], playerData.head["posY"], playerData.head["posZ"]);
        head.rotation = new Quaternion(playerData.head["rotX"], playerData.head["rotY"], playerData.head["rotZ"], playerData.head["rotW"]);

        leftHand.position = new Vector3(playerData.lHand["posX"], playerData.lHand["posY"], playerData.lHand["posZ"]);
        leftHand.rotation = new Quaternion(playerData.lHand["rotX"], playerData.lHand["rotY"], playerData.lHand["rotZ"], playerData.lHand["rotW"]);

        rightHand.position = new Vector3(playerData.rHand["posX"], playerData.rHand["posY"], playerData.rHand["posZ"]);
        rightHand.rotation = new Quaternion(playerData.rHand["rotX"], playerData.rHand["rotY"], playerData.rHand["rotZ"], playerData.rHand["rotW"]);
    }

}
