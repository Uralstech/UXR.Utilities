using UnityEngine;
using UnityEngine.Serialization;
using Uralstech.Utils.Singleton;

namespace Uralstech.UXR.Utilities
{
    /// <summary>
    /// Simple movement controller using Meta Quest input.
    /// </summary>
    [AddComponentMenu("Uralstech/UXR/Utilities/Movement/Simple Movement Controller"), RequireComponent(typeof(Rigidbody))]
    public class SimpleMovementController : DontCreateNewSingleton<SimpleMovementController>
    {
        /// <summary>
        /// Use this boolean to toggle movement.
        /// </summary>
        [Tooltip("Boolean to toggle movement.")]
        public bool MovementEnabled = true;

        /// <summary>
        /// The movement speed.
        /// </summary>
        [Tooltip("The movement speed.")]
        [FormerlySerializedAs("MoveSpeed")] public float MovementSpeed = 3f;

        /// <summary>
        /// Snap turn angle.
        /// </summary>
        [Tooltip("Snap turn angle.")]
        public float RotationSnapAngle = 30f;

        private Transform _playerHeadTransform;
        private Rigidbody _rigidbody;
        private int _registeredInput = 0;

        private void Start()
        {
            // Find the OVRCameraRig in the scene
            OVRCameraRig cameraRig = FindAnyObjectByType<OVRCameraRig>();
            if (cameraRig != null)
                _playerHeadTransform = cameraRig.centerEyeAnchor;
            else
            {
                Debug.LogError($"{nameof(OVRCameraRig)} not found for {nameof(SimpleCameraFollower)} script!");
                enabled = false; // Disable the script if camera rig is not found
            }

            _rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (MovementEnabled)
            {
                Vector2 primaryThumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
                Vector3 moveDirection = ((_playerHeadTransform.forward * primaryThumbstick.y) + (_playerHeadTransform.right * primaryThumbstick.x)).normalized;
                _rigidbody.linearVelocity = new Vector3(moveDirection.x * MovementSpeed, _rigidbody.linearVelocity.y, moveDirection.z * MovementSpeed);
            }

            Vector2 secondaryThumbstick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
            secondaryThumbstick.x = Mathf.RoundToInt(secondaryThumbstick.x);

            if (secondaryThumbstick.x != _registeredInput)
            {
                _registeredInput = (int)secondaryThumbstick.x;
                if (secondaryThumbstick.x == 0f)
                    return;

                _rigidbody.rotation = Quaternion.Euler(_rigidbody.rotation.eulerAngles + new Vector3(0f, RotationSnapAngle * secondaryThumbstick.x, 0f));
            }
        }
    }
}