// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.UI;

namespace Microsoft.MixedReality.Toolkit.Experimental.UI
{
    /// <summary>
    /// This class toggles the Caps Lock image based on the NonNativeKeyboard's IsCapsLocked state 
    /// </summary>
    public class CapsLockHighlight : MonoBehaviour
    {
        /// <summary>
        /// The highlight image to turn on and off.
        /// </summary>
        //[Experimental]
        [SerializeField]
        private Image m_Highlight = null;

        /// <summary>
        /// The keyboard to check for caps locks
        /// </summary>
        private NonNativeKeyboard m_Keyboard;

        /// <summary>
        /// Unity Start method.
        /// </summary>
        private void Start()
        {
            m_Keyboard = GetComponentInParent<NonNativeKeyboard>();
            NonNativeKeyboard.Instance.OnKeyboardShifted += Instance_OnKeyboardShifted;
            UpdateState();
        }

        private void Instance_OnKeyboardShifted(bool obj)
        {
            UpdateState();
        }

        /// <summary>
        /// Updates the visual state of the shift highlight.
        /// </summary>
        private void UpdateState()
        {
            if (m_Keyboard != null && m_Highlight != null)
            {
                m_Highlight.enabled = m_Keyboard.IsShifted;
                //Debug.Log(m_Keyboard.IsCapsLocked);
            }
        }
    }
}
