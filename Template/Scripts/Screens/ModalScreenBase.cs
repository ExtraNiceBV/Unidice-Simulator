using System;
using System.Collections.Generic;
using Unidice.SDK.Unidice;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Template.Screens
{
    public class ScreenEvent : UnityEvent<IScreen> { }

    public interface IScreen
    {
        ScreenEvent OnOpen { get; }
        ScreenEvent OnClose { get; }
        void Open();
        void Close();
        IEnumerable<ImageSequence> GetSequences();
    }

    public class ModalScreenBase : MonoBehaviour, IScreen
    {
        public ScreenEvent OnOpen { get; } = new ScreenEvent();
        public ScreenEvent OnClose { get; } = new ScreenEvent();
        private GraphicRaycaster[] _graphicRaycasters;

        public virtual void Open()
        {
            _graphicRaycasters ??= GetComponentsInChildren<GraphicRaycaster>(true);
            gameObject.SetActive(true);
            OnOpen.Invoke(this);
        }

        /// <summary>
        /// Return all sequences that are required for this screen.
        /// </summary>
        public virtual IEnumerable<ImageSequence> GetSequences() => Array.Empty<ImageSequence>();

        public virtual void Close()
        {
            // Set active after onClose, so objects can react while they're still active (for example animator)
            OnClose.Invoke(this);
            gameObject.SetActive(false);
        }

        public void MakeNonInteractable(bool value)
        {
            foreach (var raycaster in _graphicRaycasters)
            {
                raycaster.enabled = !value;
            }
        }
    }
}