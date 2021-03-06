using Unidice.SDK.Unidice;
using Unidice.Simulator.Unidice;
using UnityEngine;
using UnityEngine.UI;

namespace Unidice.Simulator.UI
{
    public class ButtonTap : MonoBehaviour
    {
        private UnidiceSimulator _unidice;
        private UnidiceRotator _rotator;
        [SerializeField] private Button button;

        public void Start()
        {
            _unidice = FindObjectOfType<UnidiceSimulator>();
            _rotator = FindObjectOfType<UnidiceRotator>();
            _rotator.OnRotated.AddListener(OnTapChanged);
            button.onClick.AddListener(TapTop);
            _unidice.Sides.OnTapDisabled.AddListener(_ => OnTapChanged());
            _unidice.Sides.OnTapEnabled.AddListener(_ => OnTapChanged());
            OnTapChanged();
        }

        private void OnTapChanged()
        {
            button.interactable = _unidice.Sides.CanTap(SideWorld.Top);
        }

        public void TapTop()
        {
            _unidice.Sides.Tap(SideWorld.Top);
        }
    }
}