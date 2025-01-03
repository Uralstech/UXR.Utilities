// Copyright 2024 URAV ADVANCED LEARNING SYSTEMS PRIVATE LIMITED
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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

        protected void Start()
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

        protected void FixedUpdate()
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