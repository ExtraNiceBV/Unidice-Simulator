using Unidice.SDK.Interfaces;
using Unidice.SDK.Unidice;
using Unidice.Simulator.UI;
using UnityEngine;

namespace Unidice.Simulator.Unidice
{
    [SelectionBase]
    public class UnidiceSimulator : UnidiceStub, IUnidice
    {
        [SerializeField] private ImageDatabase images = new ImageDatabase();
        [SerializeField] private UnidiceSides sides = new UnidiceSides();
        [Header("References")] 
        [SerializeField] private Transform moveTargetSecret;
        [SerializeField] private Transform moveTargetNormal;
        [SerializeField] private Transform moveTargetSecretZoom;
        [SerializeField] private Transform moveTargetNormalZoom;
        [SerializeField] private GameObject objSecretZone;
        [SerializeField] private UnidiceRotator rotator;

        [Header("Settings")]
        [SerializeField] private float zoomSpeed = 2;

        private ButtonZoomDie _buttonZoom;
        private float _zoomPercentage;
        private float _zoomVelocity;

        public void Awake()
        {
            images.Initialize();
            sides.Initialize(transform, images);
            MoveToSecret(false);

            _buttonZoom = FindObjectOfType<ButtonZoomDie>();
        }

        public void Update()
        {
            sides.Update();

            UpdateZoom();
        }

        private void UpdateZoom()
        {
            _zoomPercentage = Mathf.SmoothDamp(_zoomPercentage, _buttonZoom.Zoom ? 1 : 0, ref _zoomVelocity, 1 / zoomSpeed);

            var start = InSecret ? moveTargetSecret : moveTargetNormal;
            var end = InSecret ? moveTargetSecretZoom : moveTargetNormalZoom;
            var transformBase = transform.parent;
            transformBase.position = Vector3.Lerp(start.position, end.position, _zoomPercentage);
            transformBase.rotation = Quaternion.Lerp(start.rotation, end.rotation, _zoomPercentage);
            var gravity = transformBase.up * -10;
            rotator.Gravity = gravity;
            sides.Gravity = gravity;
        }

        public void OnDestroy()
        {
            sides.Clear();
        }

        public IImageDatabase Images => images;
        public IUnidiceSides Sides => sides;
        public IUnidiceRotator Rotator => rotator;
        public bool IsValid => this;
        public bool InSecret { get; private set; }

        public void MoveToSecret(bool secret)
        {
            InSecret = secret;
            //var target = secret ? moveTargetSecret : moveTargetNormal;
            //transform.parent.SetPositionAndRotation(target.position, target.rotation);
            objSecretZone.SetActive(secret);
            Rotator.RollInSecret = secret;
        }

        public override IUnidice GetUnidice() => this;
    }
}