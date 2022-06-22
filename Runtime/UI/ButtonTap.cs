using Unidice.SDK.Unidice;
using Unidice.SDK.Utilities;
using Unidice.Simulator.Unidice;
using UnityEngine;
using UnityEngine.UI;

namespace Unidice.Simulator.UI
{
    public class ButtonTap : MonoBehaviour
    {
        [SerializeField, Inject] private UnidiceSimulator unidice;
        [SerializeField, Inject] private UnidiceRotator rotator;
        [SerializeField] private Button buttonTap;

        public void Start()
        {
            rotator.OnRotated.AddListener(OnTapChanged);
            buttonTap.onClick.AddListener(TapTop);
            unidice.Sides.OnTapDisabled.AddListener(_ => OnTapChanged());
            unidice.Sides.OnTapEnabled.AddListener(_ => OnTapChanged());
            OnTapChanged();
        }

        private void OnTapChanged()
        {
            buttonTap.interactable = unidice.Sides.CanTap(SideWorld.Top);
        }

        public void TapTop()
        {
            unidice.Sides.Tap(SideWorld.Top);
        }
    }
}