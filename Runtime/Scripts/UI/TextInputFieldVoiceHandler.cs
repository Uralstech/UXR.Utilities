using Meta.WitAi.Dictation;
using Meta.WitAi.Requests;
using UnityEngine;
using UnityEngine.UI;

namespace Uralstech.UXR.Utilities
{
    /// <summary>
    /// Sister-script to <see cref="TextInputField"/> which adds support for voice typing through Meta's Voice SDK.
    /// </summary>
    /// <remarks>
    /// Requires a <see cref="DictationService"/> somewhere in the scene.
    /// </remarks>
    [AddComponentMenu("Uralstech/UXR/Utilities/UI/Text Input Field Voice Handler"), RequireComponent(typeof(TextInputField))]
    public class TextInputFieldVoiceHandler : MonoBehaviour
    {
        /// <summary>
        /// Is the user currently voice typing?
        /// </summary>
        public bool IsRecording { get; private set; } = false;

        /// <summary>
        /// The button to toggle voice typing.
        /// </summary>
        [Tooltip("The button to toggle voice typing.")]
        public Button ToggleButton;

        /// <summary>
        /// Optional, the icon for <see cref="ToggleButton"/> that will be changed based on the current recording state.
        /// </summary>
        [Tooltip("Optional, the icon for ToggleButton that will be changed based on the current recording state.")]
        public Image ToggleButtonIcon;

        /// <summary>
        /// Optional, icon to set for <see cref="ToggleButtonIcon"/> while not recording the user's audio.
        /// </summary>
        [Tooltip("Optional, icon to set for ToggleButtonIcon while not recording the user's audio.")]
        public Sprite StartRecordingIcon;

        /// <summary>
        /// Optional, icon to set for <see cref="ToggleButtonIcon"/> while recording the user's audio.
        /// </summary>
        [Tooltip("Optional, icon to set for ToggleButtonIcon while recording the user's audio.")]
        public Sprite StopRecordingIcon;

        private TextInputField _inputField;
        private DictationService _dictation;
        private string _previousPartialTranscription = string.Empty;
        private VoiceServiceRequest _currentSynthesisRequest;

        private void Start()
        {
            ToggleButton.onClick.AddListener(OnToggleRecording);

            if (ToggleButtonIcon != null)
                ToggleButtonIcon.sprite = StartRecordingIcon;

            _dictation = FindAnyObjectByType<DictationService>();
            _inputField = GetComponent<TextInputField>();

            _inputField.OnFieldDeselected.AddListener(() =>
            {
                if (IsRecording)
                {
                    _currentSynthesisRequest?.Events.OnFullTranscription.RemoveListener(OnFullTranscription);
                    StopRecording();
                }
            });

            _dictation.DictationEvents.OnPartialTranscription.AddListener(transcription => OnPartialTranscription(transcription, false));
            _dictation.DictationEvents.OnStartListening.AddListener(() =>
            {
                if (!IsRecording)
                    ToggleButton.interactable = false;
            });

            _dictation.DictationEvents.OnComplete.AddListener(_ =>
            {
                if (!IsRecording)
                    ToggleButton.interactable = true;
            });
        }

        private void OnToggleRecording()
        {
            if (!IsRecording)
                StartRecording();
            else
                StopRecording();
        }

        private void StartRecording()
        {
            if (!_inputField.IsFocused)
                _inputField.ToggleListeningState();

            Debug.Log($"{nameof(TextInputFieldVoiceHandler)}: Starting dictation.");
            _previousPartialTranscription = string.Empty;
            IsRecording = true;

            if (ToggleButtonIcon != null)
                ToggleButtonIcon.sprite = StopRecordingIcon;

            VoiceServiceRequestEvents events = new();
            events.OnFullTranscription.AddListener(OnFullTranscription);
            events.OnComplete.AddListener(_ => StopRecording());
            _currentSynthesisRequest = _dictation.ActivateImmediately(events);
        }

        private void StopRecording()
        {
            Debug.Log($"{nameof(TextInputFieldVoiceHandler)}: Stopping dictation.");
            IsRecording = false;

            if (ToggleButtonIcon != null)
                ToggleButtonIcon.sprite = StartRecordingIcon;

            if (_dictation.Active)
                _dictation.Deactivate();
        }

        private void OnPartialTranscription(string transcription, bool overrideRecordingCheck)
        {
            if (!IsRecording && !overrideRecordingCheck)
                return;

            _inputField.AppendText(transcription[_previousPartialTranscription.Length..]);
            if (ReferenceEquals(XRKeyboardManager.Instance.CurrentListener, _inputField))
                XRKeyboardManager.Instance.KeyboardInstance.ChangeTextContext(_inputField.Text);

            _previousPartialTranscription = transcription;
        }

        private void OnFullTranscription(string transcription)
        {
            OnPartialTranscription(transcription, true);
            _previousPartialTranscription = string.Empty;
        }
    }
}
