using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Uralstech.UXR.Utilities
{
    /// <summary>
    /// Text input field compatible with <see cref="OVRVirtualKeyboard"/> input.
    /// </summary>
    [AddComponentMenu("Uralstech/UXR/Utilities/UI/Text Input Field")]
    public class TextInputField : Selectable, IPointerClickHandler, OVRVirtualKeyboard.ITextHandler
    {
#pragma warning disable IDE1006 // Naming Styles
        [Obsolete("Use TextInputField.Text instead.")]
        public string text => Text;

        [Obsolete("Use TextInputField.OnFieldEdited instead.")]
        public UnityEvent<string> onValueChanged => OnFieldEdited;
#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// The text entered by the user.
        /// </summary>
        public string Text => _input != null && _input.text != null ? _input.text : string.Empty;

        /// <inheritdoc/>
        public Action<string> OnTextChanged { get; set; }

        /// <inheritdoc/>
        [field: SerializeField] public bool SubmitOnEnter { get; set; } = true;

        /// <inheritdoc/>
        public bool IsFocused { get; private set; }

        /// <summary>
        /// Called when the input field has been edited.
        /// </summary>
        [Tooltip("Called when the input field has been edited.")]
        public UnityEvent<string> OnFieldEdited = new();

        /// <summary>
        /// Called when the input field has been selected.
        /// </summary>
        [Tooltip("Called when the input field has been selected.")]
        public UnityEvent OnFieldSelected = new();

        /// <summary>
        /// Called when the input field has been deselected.
        /// </summary>
        [Tooltip("Called when the input field has been deselected.")]
        public UnityEvent OnFieldDeselected = new();

        [SerializeField, Tooltip("The text which will contain the player's input.")] private TMP_Text _input;
        [SerializeField, Tooltip("Placeholder text for when the field is empty.")] private TMP_Text _placeHolder;

        /// <inheritdoc/>
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (!IsInteractable())
                return;

            ToggleListeningState();
        }

        /// <summary>
        /// Toggles the focused and keyboard listener states.
        /// </summary>
        internal void ToggleListeningState()
        {
            if (IsFocused)
                XRKeyboardManager.Instance.RemoveListener(this);
            else
                XRKeyboardManager.Instance.SetListener(this);

            UpdateFocus(!IsFocused);
        }

        private void UpdateFocus(bool focus)
        {
            if (IsFocused = focus)
                OnFieldSelected.Invoke();
            else
                OnFieldDeselected.Invoke();
        }

        private void ManagePlaceholder()
        {
            bool isTextEmpty = string.IsNullOrEmpty(_input.text);
            _placeHolder.gameObject.SetActive(isTextEmpty);
            _input.gameObject.SetActive(!isTextEmpty);
        }

        /// <summary>
        /// Sets the text in the field, without calling <see cref="OnFieldEdited"/>.
        /// </summary>
        /// <param name="text">The new text.</param>
        public void SetTextWithoutNotify(string text)
        {
            _input.text = text;
            ManagePlaceholder();
        }

        /// <summary>
        /// Sets the text in the field.
        /// </summary>
        /// <param name="text">The new text.</param>
        public void SetText(string text)
        {
            SetTextWithoutNotify(text);
            OnFieldEdited.Invoke(text);
        }

        /// <inheritdoc/>
        public void AppendText(string addedText)
        {
            SetText(_input.text + addedText);
        }

        /// <inheritdoc/>
        public void ApplyBackspace()
        {
            if (_input.text.Length > 0)
                SetText(_input.text[..^1]);
        }

        /// <inheritdoc/>
        public void Submit()
        {
            UpdateFocus(false);
        }

        /// <inheritdoc/>
        public void MoveTextEnd() { }

        /// <inheritdoc/>
        protected override void OnCanvasGroupChanged()
        {
            base.OnCanvasGroupChanged();

            if (IsFocused)
                ToggleListeningState();
        }

        /// <inheritdoc/>
        protected override void OnDisable()
        {
            base.OnDisable();

            if (IsFocused)
                ToggleListeningState();
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (IsFocused)
                ToggleListeningState();
        }
    }
}
