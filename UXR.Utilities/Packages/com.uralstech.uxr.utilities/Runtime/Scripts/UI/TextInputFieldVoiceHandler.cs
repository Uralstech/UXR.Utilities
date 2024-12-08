using Meta.WitAi.Dictation;
using Meta.WitAi.Requests;
using UnityEngine;
using UnityEngine.UI;

namespace Uralstech.UXR.Utilities
{
    [AddComponentMenu("Uralstech/UXR/Utilities/UI/Text Input Field Voice Handler"), RequireComponent(typeof(TextInputField))]
    public class TextInputFieldVoiceHandler : MonoBehaviour
    {
        public bool IsRecording { get; private set; } = false;

        public Button ToggleButton;
        public Image ToggleButtonIcon;

        public Sprite StartRecordingIcon;
        public Sprite StopRecordingIcon;

        private TextInputField _inputField;
        private DictationService _dictation;
        private string _previousPartialTranscription = string.Empty;
        private VoiceServiceRequest _currentSynthesisRequest;

        private void Start()
        {
            ToggleButton.onClick.AddListener(OnToggleRecording);
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
            ToggleButtonIcon.sprite = StopRecordingIcon;
            IsRecording = true;

            VoiceServiceRequestEvents events = new();
            events.OnFullTranscription.AddListener(OnFullTranscription);
            events.OnComplete.AddListener(_ => StopRecording());
            _currentSynthesisRequest = _dictation.ActivateImmediately(events);
        }

        private void StopRecording()
        {
            Debug.Log($"{nameof(TextInputFieldVoiceHandler)}: Stopping dictation.");
            ToggleButtonIcon.sprite = StartRecordingIcon;
            IsRecording = false;

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
