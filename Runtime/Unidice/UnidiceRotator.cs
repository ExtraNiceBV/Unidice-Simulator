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
    public class UnidiceRotator : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IUnidiceRotator, IPointerClickHandler
    {
        [SerializeField] private readonly int rollLiftSpeed = 5;
        [SerializeField] private readonly int rollSpeed = 20;
        private int _clickMask;
        private bool _isRolling;
        private bool _isShaking;
        private SpringJoint _joint;
        private float _jointSpringAmount;
        private Rigidbody _rigidbody;
        private float _durationNotMoving;

        [SerializeField] private CursorNotifier cursorNotifier;
        [SerializeField] private float shakeSpeed = 50;
        [SerializeField] private float shakeDuration = 1;
        [SerializeField] private float shakeStrength = 1;

        public Vector3 Gravity { get; internal set; } = Vector3.up * -10;
        public bool IsBusy => _isShaking || _isRolling;
        private static int RandomSign => Random.value > 0.5f ? 1 : -1;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_isRolling) return;

            _rigidbody.isKinematic = true;
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

        public void OnEndDrag(PointerEventData eventData)
        {
            if (IsBusy) return;

            _rigidbody.isKinematic = false;
            Invoker.InvokeWhen(OnRotated.Invoke, () => _rigidbody.IsSleeping());
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var ray = new Ray(eventData.pointerPressRaycast.worldPosition, eventData.pointerPressRaycast.worldNormal * -0.3f);

            Debug.DrawRay(ray.origin, ray.direction, Color.cyan, 2, false);
            var result = new RaycastHit[3];
            var hits = Physics.RaycastNonAlloc(ray, result, 0.3f, _clickMask, QueryTriggerInteraction.Collide);

            for (var i = 0; i < hits; i++)
            {
                var screen = result[i].collider.GetComponent<TappableScreen>();
                if (screen)
                {
                    screen.OnClick(eventData);
                    break;
                }
            }
        }

        public UnityEvent OnRotated { get; } = new UnityEvent();
        public UnityEvent OnRolled { get; } = new UnityEvent();
        public UnityEvent OnShake { get; } = new UnityEvent();
        public UnityEvent OnStartedRolling { get; } = new UnityEvent();

        public bool RollInSecret { get; set; }

        public void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.isKinematic = true;
            _joint = GetComponent<SpringJoint>();
            _jointSpringAmount = _joint.spring;
            _clickMask = LayerMask.GetMask("Table");
        }

        private async UniTask ApplyGravitySequence(CancellationToken cancellationToken)
        {
            float durationNotMoving = 0;
            while (!_rigidbody.IsSleeping())
            {
                // Sleeping has to be done manually - barely moving?
                if (_rigidbody.velocity.sqrMagnitude < 0.01f)
                {
                    durationNotMoving += Time.fixedDeltaTime;
                    if (durationNotMoving >= 0.1f) // after amount of time not moving
                    {
                        _rigidbody.Sleep();
                        await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationToken);
                        continue;
                    }
                }
                else durationNotMoving = 0;

                // By adding force manually, we prevent sleeping
                _rigidbody.AddForce(Gravity, ForceMode.Acceleration);
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationToken);
            }
        }

        public void FixedUpdate()
        {
 
        }

        public void Roll()
        {
            if (IsBusy) return;
            RollSequence(this.GetCancellationTokenOnDestroy()).Forget();
        }

        public void Shake()
        {
            if (IsBusy) return;
            ShakeSequence(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTask RollSequence(CancellationToken cancellationToken)
        {
            _isRolling = true;
            OnStartedRolling.Invoke();
            cursorNotifier.enabled = false;
            FPSManager.FPS = TargetFPS.High;
            var up = -Gravity.normalized;
            transform.position += up * 0.5f;
            transform.rotation = Random.rotation;
            _rigidbody.isKinematic = false;
            _rigidbody.velocity = up * rollLiftSpeed * (RollInSecret ? 0.5f : 1);
            // Make sure we don't just spin around one axis
            _rigidbody.angularVelocity = rollSpeed * new Vector3(RandomSign * Random.Range(0.5f, 1f), RandomSign * Random.Range(0.5f, 1f), RandomSign * Random.Range(0.5f, 1f));
            _joint.spring = 0;
            var gravity = ApplyGravitySequence(cancellationToken);
            await UniTask.WaitUntil(() => _rigidbody.velocity.y < 0, cancellationToken: cancellationToken);
            _joint.spring = _jointSpringAmount;

            await gravity;
            FPSManager.FPS = TargetFPS.Low;
            cursorNotifier.enabled = true;
            _isRolling = false;
            OnRotated.Invoke();
            OnRolled.Invoke();
        }

        private async UniTask ShakeSequence(CancellationToken cancellationToken)
        {
            _isShaking = true;
            OnShake.Invoke();
            cursorNotifier.enabled = false;
            FPSManager.FPS = TargetFPS.High;
            var up = -Gravity.normalized;

            var startPosition = transform.localPosition;
            var startTime = Time.time;

            _rigidbody.isKinematic = true;

            while (Time.time < startTime + shakeDuration)
            {
                transform.localPosition = startPosition + Vector3.up + (Vector3.up+Vector3.forward) * Mathf.Cos(Time.time * shakeSpeed) * shakeStrength;
                await UniTask.Yield(cancellationToken);
            }

            _rigidbody.isKinematic = false;

            transform.localPosition = startPosition;
            
            FPSManager.FPS = TargetFPS.Low;
            cursorNotifier.enabled = true;
            _isShaking = false;
        }
    }
}