using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unidice.Simulator.UI
{
    public class ButtonZoomDie : UIBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public bool Zoom { get; private set; }
        private bool _lockedIn;

        internal readonly UnityEvent onToggled = new UnityEvent();

        protected override void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Press);
            SetLocked(false);
        }

        private void SetLocked(bool value)
        {
            _lockedIn = value;
            SetZoom(_lockedIn);
            onToggled.Invoke();
        }

        private void SetZoom(bool value)
        {
            if (Zoom == value) return;

            Zoom = value;
            onToggled.Invoke();
        }

        public void Press()
        {
            SetLocked(!_lockedIn);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetZoom(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_lockedIn) SetZoom(false);
        }
    }
}