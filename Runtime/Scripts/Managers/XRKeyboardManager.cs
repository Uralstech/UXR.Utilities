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
using UnityEngine.Events;
using UnityEngine.Serialization;
using Uralstech.Utils.Singleton;

namespace Uralstech.UXR.Utilities
{
    /// <summary>
    /// Wrapper class for Meta's Virtual Keyboard.
    /// </summary>
    [AddComponentMenu("Uralstech/UXR/Utilities/XR Keyboard Manager")]
    public class XRKeyboardManager : Singleton<XRKeyboardManager>
    {
        /// <summary>
        /// The object currently bound to the keyboard.
        /// </summary>
        public OVRVirtualKeyboard.ITextHandler CurrentListener { get; private set; }

        /// <summary>
        /// The player <see cref="Transform"/>. The keyboard will spawn relative to this object's position. Defaults to first OVRCameraRig found in the scene.
        /// </summary>
        [Tooltip("The player transform. The keyboard will spawn relative to this object's position. Defaults to first OVRCameraRig found in the scene.")]
        [FormerlySerializedAs("_player")] public Transform PlayerTransform;

        /// <summary>
        /// <see cref="OVRVirtualKeyboard"/> prefab to spawn when needed.
        /// </summary>
        [Tooltip("OVRVirtualKeyboard prefab to spawn when needed.")]
        [FormerlySerializedAs("_keyboardPrefab")] public OVRVirtualKeyboard KeyboardPrefab;

        /// <summary>
        /// The root transform of the left controller, for example: OVRCameraRig &gt; OVRInteraction &gt; OVRControllers &gt; LeftController.
        /// </summary>
        [Header("Controller Input")]
        [Tooltip("The root transform of the left controller, for example: OVRCameraRig > OVRInteraction > OVRControllers > LeftController.")]
        [FormerlySerializedAs("_leftControllerRootTransform")] public Transform LeftControllerRootTransform;

        /// <summary>
        /// The "direct" transform of the left controller, for example: OVRCameraRig &gt; OVRInteraction &gt; OVRControllers &gt; LeftController &gt; ControllerInteractors &gt; ControllerPokeInteractor &gt; PokeLocation.
        /// </summary>
        /// <remarks>
        /// You can add a tracked custom interactor to use as the direct transform:<br/>
        /// - Add a new GameObject with a ControllerRef, ControllerPointerPose and ActiveStateTracker under OVRInteraction &gt; OVRControllers &gt; LeftController &gt; ControllerInteractors.<br/>
        /// - Drag LeftController into the ControllerRef's "Controller" field.<br/>
        /// - Set the "Active State" and "Controller" fields in the ActiveStateTracker and ControllerPointerPose to the ControllerRef.<br/>
        /// - Set the Z value in the "Offset" field of the ControllerPointerPose to 0.01.<br/>
        /// - (Optional) Enable "Include Children As Dependents" in the ActiveStateTracker and add a sphere as a child of the GameObject.
        /// </remarks>
        [Tooltip("The \"direct\" transform of the left controller, for example: OVRCameraRig > OVRInteraction > OVRControllers > LeftController > ControllerInteractors > ControllerPokeInteractor > PokeLocation.")]
        [FormerlySerializedAs("_leftControllerDirectTransform")] public Transform LeftControllerDirectTransform;

        /// <summary>
        /// The root transform of the right controller, for example: OVRCameraRig &gt; OVRInteraction &gt; OVRControllers &gt; RightController.
        /// </summary>
        [Tooltip("The root transform of the right controller, for example: OVRCameraRig > OVRInteraction > OVRControllers > RightController.")]
        [FormerlySerializedAs("_rightControllerRootTransform")] public Transform RightControllerRootTransform;

        /// <summary>
        /// The "direct" transform of the right controller, for example: OVRCameraRig &gt; OVRInteraction &gt; OVRControllers &gt; RightController &gt; ControllerInteractors &gt; ControllerPokeInteractor &gt; PokeLocation.
        /// </summary>
        /// <remarks>
        /// You can add a tracked custom interactor to use as the direct transform:<br/>
        /// - Add a new GameObject with a ControllerRef, ControllerPointerPose and ActiveStateTracker under OVRInteraction &gt; OVRControllers &gt; RightController &gt; ControllerInteractors.<br/>
        /// - Drag RightController into the ControllerRef's "Controller" field.<br/>
        /// - Set the "Active State" and "Controller" fields in the ActiveStateTracker and ControllerPointerPose to the ControllerRef.<br/>
        /// - Set the Z value in the "Offset" field of the ControllerPointerPose to 0.01.<br/>
        /// - (Optional) Enable "Include Children As Dependents" in the ActiveStateTracker and add a sphere as a child of the GameObject.
        /// </remarks>
        [Tooltip("The \"direct\" transform of the right controller, for example: OVRCameraRig > OVRInteraction > OVRControllers > RightController > ControllerInteractors > ControllerPokeInteractor > PokeLocation.")]
        [FormerlySerializedAs("_rightControllerDirectTransform")] public Transform RightControllerDirectTransform;

        /// <summary>
        /// The OVRHand script for left hand input.
        /// </summary>
        [Header("Hand Input")]
        [Tooltip("The OVRHand script for left hand input.")]
        [FormerlySerializedAs("_leftOVRHand")] public OVRHand LeftOVRHand;

        /// <summary>
        /// The OVRHand script for right hand input.
        /// </summary>
        [Tooltip("The OVRHand script for right hand input.")]
        [FormerlySerializedAs("_rightOVRHand")] public OVRHand RightOVRHand;

        /// <summary>
        /// Should the hand tracking material be changed depending on if the keyboard is being used?
        /// </summary>
        [Header("Hand Material Switching")]
        [Tooltip("Should the hand tracking material be changed depending on if the keyboard is being used?")]
        [FormerlySerializedAs("_useDifferentMaterialsForHandsWhenTyping")] public bool UseDifferentMaterialsForHandsWhenTyping;

        /// <summary>
        /// The hand tracking material when the keyboard is not being used.
        /// </summary>
        [Tooltip("The hand tracking material when the keyboard is not being used.")]
        [FormerlySerializedAs("_normalHandMaterial")] public Material NormalHandMaterial;

        /// <summary>
        /// The hand tracking material when the keyboard is being used.
        /// </summary>
        [Tooltip("The hand tracking material when the keyboard is being used.")]
        [FormerlySerializedAs("_typingHandMaterial")] public Material TypingHandMaterial;

        /// <summary>
        /// Called when the keyboard is created.
        /// </summary>
        [Header("Callbacks")]
        [Tooltip("Called when the keyboard is created.")]
        [FormerlySerializedAs("OnKeyboardCreated")] public UnityEvent OnKeyboardShown = new();

        /// <summary>
        /// Called when the keyboard is destroyed.
        /// </summary>
        [Tooltip("Called when the keyboard is destroyed.")]
        [FormerlySerializedAs("OnKeyboardDestroyed")] public UnityEvent OnKeyboardHidden = new();

        /// <summary>
        /// The current instance of the <see cref="OVRVirtualKeyboard"/>.
        /// </summary>
        [HideInInspector] public OVRVirtualKeyboard KeyboardInstance;

        private SkinnedMeshRenderer _leftHandRenderer;
        private SkinnedMeshRenderer _rightHandRenderer;

        protected void Start()
        {
            Debug.Log("Creating new keyboard!");

            KeyboardPrefab.leftControllerRootTransform = LeftControllerRootTransform;
            KeyboardPrefab.leftControllerDirectTransform = LeftControllerDirectTransform;

            KeyboardPrefab.rightControllerRootTransform = RightControllerRootTransform;
            KeyboardPrefab.rightControllerDirectTransform = RightControllerDirectTransform;

            KeyboardPrefab.handLeft = LeftOVRHand;
            KeyboardPrefab.handRight = RightOVRHand;

            KeyboardPrefab.gameObject.SetActive(false);

            if (PlayerTransform == null)
                PlayerTransform = FindAnyObjectByType<OVRCameraRig>().transform;

            KeyboardInstance = Instantiate(KeyboardPrefab, PlayerTransform.position, Quaternion.identity);

            KeyboardInstance.KeyboardHiddenEvent.AddListener(HideKeyboard);
            KeyboardInstance.EnterEvent.AddListener(HideKeyboard);

            Debug.Log("Keyboard created!");

            if (!UseDifferentMaterialsForHandsWhenTyping)
                return;

            _leftHandRenderer = LeftOVRHand.GetComponent<SkinnedMeshRenderer>();
            _rightHandRenderer = RightOVRHand.GetComponent<SkinnedMeshRenderer>();
        }

        private void SetHandMaterial(Material material)
        {
            if (!UseDifferentMaterialsForHandsWhenTyping)
                return;

            _leftHandRenderer.sharedMaterial = material;
            _rightHandRenderer.sharedMaterial = material;
        }

        private void ShowKeyboard()
        {
            KeyboardInstance.gameObject.SetActive(true);
            KeyboardInstance.TextHandler = CurrentListener;

            Debug.Log("Text handler set in keyboard!");

            if (SimpleMovementController.HasInstance)
                SimpleMovementController.Instance.MovementEnabled = false;

            SetHandMaterial(TypingHandMaterial);
            OnKeyboardShown.Invoke();
        }

        private void HideKeyboard()
        {
            if (KeyboardInstance == null)
                return;

            CurrentListener?.Submit();
            KeyboardInstance.TextHandler = CurrentListener = null;

            KeyboardInstance.gameObject.SetActive(false);

            Debug.Log("Keyboard hidden!");

            if (SimpleMovementController.HasInstance)
                SimpleMovementController.Instance.MovementEnabled = true;

            SetHandMaterial(NormalHandMaterial);
            OnKeyboardHidden.Invoke();
        }

        /// <summary>
        /// Binds the given object to the keyboard.
        /// </summary>
        /// <param name="listener">The object to bind to the keyboard.</param>
        public void SetListener(OVRVirtualKeyboard.ITextHandler listener)
        {
            if (listener == null)
                return;

            Debug.Log("Keyboard listener set.");
            CurrentListener?.Submit();

            CurrentListener = listener;
            ShowKeyboard();
        }

        /// <summary>
        /// Unbinds the given object from the keyboard.
        /// </summary>
        /// <param name="listener">The object to unbind from the keyboard. Must be the same as <see cref="CurrentListener"/>.</param>
        public void RemoveListener(OVRVirtualKeyboard.ITextHandler listener)
        {
            if (!ReferenceEquals(CurrentListener, listener) || listener == null)
                return;

            CurrentListener = null;
            Debug.Log("Keyboard listener removed.");

            HideKeyboard();
        }
    }
}
