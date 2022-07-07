using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unidice.Simulator.UI
{
    public class ButtonLock : UIBehaviour
    {
        [SerializeField] private GameObject objectLocked;
        [SerializeField] private GameObject objectUnlocked;
        public bool Locked { get; private set; }

        internal readonly UnityEvent onToggled = new UnityEvent();

        protected override void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Press);
            SetLocked(false);
        }

        private void SetLocked(bool value)
        {
            objectUnlocked.SetActive(!value);
            objectLocked.SetActive(value);
            Locked = value;
            onToggled.Invoke();
        }

        public void Press()
        {
            SetLocked(!Locked);
        }
    }
}