using Unidice.SDK.Interfaces;
using Unidice.SDK.Unidice;
using UnityEngine;

namespace Unidice.Simulator.Unidice
{
    [SelectionBase]
    public class UnidiceSimulator : UnidiceStub, IUnidice
    {
        [SerializeField] private ImageDatabase images = new ImageDatabase();
        [SerializeField] private UnidiceSides sides = new UnidiceSides();
        [Header("References"), SerializeField]
         private Transform moveTargetSecret;
        [SerializeField] private Transform moveTargetNormal;
        [SerializeField] private GameObject objSecretZone;

        public void Awake()
        {
            images.Initialize();
            sides.Initialize(transform, images);
            Rotator = GetComponent<UnidiceRotator>();
            MoveToSecret(false);
        }

        public void Update()
        {
            sides.Update();
        }

        public void OnDestroy()
        {
            sides.Clear();
        }

        public IImageDatabase Images => images;
        public IUnidiceSides Sides => sides;
        public IUnidiceRotator Rotator { get; private set; }

        public bool IsValid => this;

        public void MoveToSecret(bool secret)
        {
            var target = secret ? moveTargetSecret : moveTargetNormal;
            transform.parent.SetPositionAndRotation(target.position, Quaternion.identity);
            objSecretZone.SetActive(secret);
            Rotator.RollInSecret = secret;
        }

        public override IUnidice GetUnidice() => this;
    }
}