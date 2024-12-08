using UnityEngine;
using UnityEngine.Serialization;

namespace Uralstech.UXR.Utilities
{
    /// <summary>
    /// Simple follow script which allows the follower to be offset from the player.
    /// </summary>
    [AddComponentMenu("Uralstech/UXR/Utilities/Movement/Simple Camera Follower")]
    public class SimpleCameraFollower : MonoBehaviour
    {
        /// <summary>
        /// Speed at which the object follows the player.
        /// </summary>
        [Tooltip("Speed at which the object follows the player.")]
        [FormerlySerializedAs("_followSpeed")] public float MovementSpeed = 3f;

        /// <summary>
        /// Speed at which the object rotates to match the player's head rotation.
        /// </summary>
        [Tooltip("Speed at which the object rotates to match the player's head rotation.")]
        [FormerlySerializedAs("_rotationSpeed")] public float RotationSpeed = 5f;

        /// <summary>
        /// Z distance offset from the player.
        /// </summary>
        [Tooltip("Z distance offset from the player.")]
        [FormerlySerializedAs("_distanceOffset")] public float DistanceOffset = 0.35f;

        /// <summary>
        /// Height (Y) offset from the player.
        /// </summary>
        [Tooltip("Height (Y) offset from the player.")]
        [FormerlySerializedAs("_heightOffset")] public float HeightOffset = -0.31f;

        /// <summary>
        /// Rotation offset from the player.
        /// </summary>
        [Tooltip("Rotation offset from the player.")]
        [FormerlySerializedAs("_rotationOffset")] public Vector3 RotationOffset = new(30f, 0f);

        /// <summary>
        /// Delay before starting to follow player rotation.
        /// </summary>
        [Tooltip("Delay before starting to follow player rotation.")]
        [FormerlySerializedAs("_rotationDelayAngle")] public float RotationDelayAngle = 40f;

        /// <summary>
        /// Delay before starting to follow player position.
        /// </summary>
        [Tooltip("Delay before starting to follow player position.")]
        [FormerlySerializedAs("_movementDelayDistance")] public float MovementDelayDistance = 0.07f;

        private Transform _playerHeadTransform; // Reference to the player's head transform

        private float _adjustedUpperHeightOffset;
        private float _adjustedLowerHeightOffset;
        private float _adjustedUpperDistanceOffset;
        private float _adjustedLowerDistanceOffset;

        private Vector3 _targetPosition = Vector3.zero;
        private Quaternion _targetRotation = Quaternion.identity;

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

            _adjustedUpperDistanceOffset = Mathf.Abs(DistanceOffset) + MovementDelayDistance;
            _adjustedLowerDistanceOffset = Mathf.Abs(DistanceOffset) - MovementDelayDistance;
            _adjustedUpperHeightOffset = Mathf.Abs(HeightOffset) + MovementDelayDistance;
            _adjustedLowerHeightOffset = Mathf.Abs(HeightOffset) - MovementDelayDistance;
        }

        private void LateUpdate()
        {
            if (_playerHeadTransform == null)
                return;

            // Calculate the angle between the camera forward direction and the direction towards the follower
            Vector3 transformDirection = transform.position - _playerHeadTransform.position;
            transformDirection.y = 0f; // Ignore vertical component

            Vector3 cameraForward = _playerHeadTransform.forward;
            cameraForward.y = 0f; // Ignore vertical component

            float angle = Vector3.Angle(cameraForward, transformDirection);
            float horizontalSeperationDistance = Vector2.Distance(new(transform.position.x, transform.position.z), new(_playerHeadTransform.position.x, _playerHeadTransform.position.z));
            float verticalSeperationDistance = Vector2.Distance(new(0, transform.position.y), new(0, _playerHeadTransform.position.y));

            if (horizontalSeperationDistance > _adjustedUpperDistanceOffset
                || verticalSeperationDistance > _adjustedUpperHeightOffset
                || horizontalSeperationDistance < _adjustedLowerDistanceOffset
                || verticalSeperationDistance < _adjustedLowerHeightOffset
                || angle > RotationDelayAngle)
            {
                _targetPosition = _playerHeadTransform.position + Quaternion.Euler(0f, _playerHeadTransform.eulerAngles.y, 0f) * Vector3.forward * DistanceOffset + Vector3.up * HeightOffset;
                _targetRotation = Quaternion.Euler(0f, _playerHeadTransform.rotation.eulerAngles.y, 0f) * Quaternion.Euler(RotationOffset);
            }

            transform.SetPositionAndRotation(
                Vector3.Lerp(transform.position, _targetPosition, MovementSpeed * Time.deltaTime),
                Quaternion.Slerp(transform.rotation, _targetRotation, RotationSpeed * Time.deltaTime)
            );
        }
    }
}