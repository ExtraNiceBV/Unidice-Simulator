using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Unidice.Simulator.UI
{
    public class ButtonLock : MonoBehaviour
    {
        [SerializeField] private GameObject objectLocked;
        [SerializeField] private GameObject objectUnlocked;
        public bool Locked { get; private set; }

        internal readonly UnityEvent onToggled = new UnityEvent();

        public void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Press);
            SetLocked(false);
        }

        private void SetLocked(bool value)
        {
            objectUnlocked.SetActive(!value);
            objectLocked.SetActive(value);
            var canvasGroup = GetComponent<CanvasGroup>();
            //if (canvasGroup) canvasGroup.alpha = value ? 1 : 0.2f;
            Locked = value;
            onToggled.Invoke();
        }

        public void Press()
        {
            SetLocked(!Locked);
        }
    }
}