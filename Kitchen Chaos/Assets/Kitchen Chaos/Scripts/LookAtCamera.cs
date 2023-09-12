using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    public class LookAtCamera : MonoBehaviour
    {
        private enum Mode { LookAtRegular, CameraForwardRegular, LookAtInverted,  CameraForwardInverted}
        [SerializeField] private Mode mode;
        [Tooltip("Set true to automatically handle \"Regular\" and \"Inverted\" modes based on the rotation ranges provided")]
        [SerializeField] private bool autoHandleInversion = true;
        [SerializeField] private Vector2Int parentRegularYRotationRange = new Vector2Int(90, 270);
        [SerializeField] private bool autoHandleInversionEveryFrame = false;
        // if the global rotation is between this value, then it will under regular, or else inverted

        private void Start()
        {
            if (!autoHandleInversion) return;

            transform.localRotation = Quaternion.identity; // reset 
            var yRot = transform.eulerAngles.y;

            if (yRot >= parentRegularYRotationRange.x && yRot < parentRegularYRotationRange.y)
            {
                if (mode == Mode.LookAtRegular || mode == Mode.LookAtInverted)
                {
                    mode = Mode.LookAtInverted;
                    // look at make the transform.forward (local z) to face away from camera (i.e. ui elemments looks normal)
                }
                else if (mode == Mode.CameraForwardRegular || mode == Mode.CameraForwardInverted)
                {
                    mode = Mode.CameraForwardRegular;
                }
            }
            else
            {
                if (mode == Mode.LookAtRegular || mode == Mode.LookAtInverted)
                {
                    mode = Mode.LookAtRegular; 
                    // look at make the transform.forward (local z) to face towards camera (i.e. ui elemments looks inverted)
                }
                else if (mode == Mode.CameraForwardRegular || mode == Mode.CameraForwardInverted)
                {
                    mode = Mode.CameraForwardInverted;
                }
            }
        }

        private void LateUpdate()
        {
            if (autoHandleInversionEveryFrame)
                Start();

            switch (mode)
            {
                case Mode.LookAtRegular:
                    // dirTowardsCam = Camera.main.transform.position - transform.position;
                    transform.LookAt(Camera.main.transform); // transform.position + direction towards camera
                    break;
                case Mode.LookAtInverted:
                    var dirFromCam = transform.position - Camera.main.transform.position;
                    transform.LookAt(transform.position + dirFromCam);
                    break;
                case Mode.CameraForwardRegular:
                    transform.forward = Camera.main.transform.forward;
                    // face the direction camera is facing  (so looks kinda facing towards the camera's perspective)
                    break;
                case Mode.CameraForwardInverted:
                    transform.forward = -Camera.main.transform.forward;
                    // face the opposite direction camera is facing  (so looks kinda inverted from camera's perspective)
                    break;
            }
            /* why accessing main-camera through static everytime is bad:
                
            intially this field did not used to be cached in the memory,
            so every time this instance was accessed, unity will have to cycle 
            through every single gameobject in the active scene until it found
            the camera, which is obviously really bad for performance, but nowadays
            (ig versions 2022 and futher) this field is cached by default by unity backend
            so thats not a problem here and we no longer need to keep local cache of it...
             */
        }
    }
}