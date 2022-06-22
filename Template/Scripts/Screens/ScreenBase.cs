using System;
using System.Collections.Generic;
using System.Threading;
using Unidice.SDK.Unidice;
using UnityEngine;

namespace Template.Screens
{
    public abstract class ScreenBase : MonoBehaviour, IScreen
    {
        protected CancellationTokenSource cancellationToken;
        public ScreenEvent OnOpen { get; } = new ScreenEvent();
        public ScreenEvent OnClose { get; } = new ScreenEvent();

        /// <summary>
        /// Return all sequences that are required for this screen.
        /// </summary>
        public virtual IEnumerable<ImageSequence> GetSequences() => Array.Empty<ImageSequence>();

        public void OnDestroy()
        {
            cancellationToken?.Cancel();
        }

        public virtual void Open()
        {
            gameObject.SetActive(true);
            OnOpen.Invoke(this);
            cancellationToken = new CancellationTokenSource();
        }

        public virtual void Close()
        {
            // Set active after onClose, so objects can react while they're still active (for example animator)
            OnClose.Invoke(this);
            gameObject.SetActive(false);
            cancellationToken?.Cancel();
        }
    }
}
