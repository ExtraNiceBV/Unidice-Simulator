using System.Threading;
using Cysharp.Threading.Tasks;
using Unidice.SDK.Interfaces;
using Unidice.SDK.UI;
using Unidice.SDK.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Unidice.Simulator.Unidice
{
    public class UnidiceRotator : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IUnidiceRotator
    {
        [SerializeField] private int rollSpeed = 20;
        [SerializeField] private int rollLiftSpeed = 5;
        [SerializeField] private CursorNotifier cursorNotifier;
        private Rigidbody _rigidbody;
        private bool _isRolling;
        private SpringJoint _joint;
        private float _jointSpringAmount;

        public UnityEvent OnRotated { get; } = new UnityEvent();
        public UnityEvent OnRolled { get; } = new UnityEvent();
        public UnityEvent OnStartedRolling { get; } = new UnityEvent();

        public void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.isKinematic = true;
            _joint = GetComponent<SpringJoint>();
            _jointSpringAmount = _joint.spring;
        }

        public void Roll()
        {
            if (!_isRolling)
            {
                RollSequence(this.GetCancellationTokenOnDestroy()).Forget();
            }
        }

        private async UniTask RollSequence(CancellationToken cancellationToken)
        {
            _isRolling = true;
            OnStartedRolling.Invoke();
            cursorNotifier.enabled = false;
            FPSManager.FPS = TargetFPS.High;
            transform.position += Vector3.up * 0.5f;
            transform.rotation = Random.rotation;
            _rigidbody.isKinematic = false;
            _rigidbody.velocity = Vector3.up * rollLiftSpeed * (RollInSecret ? 0.5f : 1);
            // Make sure we don't just spin around one axis
            _rigidbody.angularVelocity = rollSpeed * new Vector3(RandomSign * Random.Range(0.5f, 1f), RandomSign * Random.Range(0.5f, 1f), RandomSign * Random.Range(0.5f, 1f)); 
            _joint.spring = 0;
            await UniTask.WaitUntil(() => _rigidbody.velocity.y < 0, cancellationToken: cancellationToken);
            _joint.spring = _jointSpringAmount;

            await UniTask.WaitUntil(() => _rigidbody.IsSleeping(), cancellationToken: cancellationToken);
            FPSManager.FPS = TargetFPS.Low;
            cursorNotifier.enabled = true;
            _isRolling = false;
            OnRotated.Invoke();
            OnRolled.Invoke();
        }

        private static int RandomSign => Random.value > 0.5f ? 1 : -1;

        public bool RollInSecret { get; set; }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_isRolling) return;

            _rigidbody.isKinematic = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isRolling) return;

            _rigidbody.isKinematic = false;
            Invoker.InvokeWhen(OnRotated.Invoke, () => _rigidbody.IsSleeping());
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isRolling) return;
            //var newRotation = Quaternion.Euler(eventData.position - _dragStartPosition);
            //var xRotation = Quaternion.Euler(0, 0, _dragStartPosition.x - eventData.position.x);
            //var yRotation = Quaternion.Euler(_dragStartPosition.y - eventData.position.y, 0, 0);
            //transform.rotation = yRotation * xRotation * _dragStartRotation;
            var position = transform.position;
            transform.RotateAround(position, Vector3.right, eventData.delta.y);
            transform.RotateAround(position, Vector3.back, eventData.delta.x);
            // Debug.Log("OnDrag");
        }
    }
}