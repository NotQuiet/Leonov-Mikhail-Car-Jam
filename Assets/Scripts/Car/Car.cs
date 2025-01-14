using System;
using DG.Tweening;
using UnityEngine;
using Unity.Cinemachine;

namespace Car
{
    [Obsolete("Obsolete")]
    public class Car : MonoBehaviour
    {
        public float moveSpeed = 5f; 
        public float shakeDuration = 0.3f; 
        public float shakeStrength = 0.5f; 
        public int shakeVibrato = 10; 
        public LayerMask obstacleLayer;
        public LayerMask finishTrigger;
        
        [Header("Cinemachine Path Settings")] 
        [SerializeField] private CinemachineDollyCart _cinemachineDollyCart;
        
        private CinemachineSmoothPath _cinemachinePath; 
        private Vector3 _startPosition; 
        private bool _isMoving = false; 
        private bool _isOnCinemachinePath = false;
        private Camera _mainCamera;
        private Vector3 _mouseStartPosition; 
        private bool _isDragging = false; 
        private Vector3 _moveDirection; 

        private void Start()
        {
            _startPosition = transform.position;
            _mainCamera = Camera.main;
        }

        public void SetPath(CinemachineSmoothPath path)
        {
            _cinemachinePath = path;
            _cinemachineDollyCart.m_Path = _cinemachinePath;
        }

        private void OnMouseEnter()
        {
            DOTween.Sequence()
                .Append(transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), .1f))
                .Append(transform.DOScale(Vector3.one, 0.1f));
        }

        private void OnMouseDown()
        {
            if (_isMoving || _isOnCinemachinePath) return;

            _mouseStartPosition = GetMouseWorldPosition();
            _isDragging = true;
        }

        private void OnMouseUp()
        {
            if (!_isDragging || _isMoving || _isOnCinemachinePath) return;

            Vector3 mouseEndPosition = GetMouseWorldPosition();
            _isDragging = false;

            Vector3 localStartPosition = transform.InverseTransformPoint(_mouseStartPosition);
            Vector3 localEndPosition = transform.InverseTransformPoint(mouseEndPosition);

            float directionZ = localEndPosition.z - localStartPosition.z;

            if (Mathf.Abs(directionZ) > 0.1f) 
            {
                _moveDirection = directionZ > 0 ? transform.forward : -transform.forward;
                StartMoving();
            }
        }

        private Vector3 GetMouseWorldPosition()
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                return hit.point;
            }

            return transform.position;
        }

        private void StartMoving()
        {
            _isMoving = true;
            Move();
        }

        private void Move()
        {
            transform.DOMove(transform.position + _moveDirection * moveSpeed, 1f / moveSpeed)
                .SetEase(Ease.Linear)
                .OnUpdate(CheckCollision) 
                .SetLoops(-1, LoopType.Incremental); 
        }

        private void CheckCollision()
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position, _moveDirection, out hit, 2f, obstacleLayer))
            {
                if (hit.collider.gameObject != gameObject) 
                {
                    StopMovement();
                    ShakeAndReset();
                    return;
                }
            }
            else if (Physics.Raycast(transform.position, _moveDirection, out hit, 2f, finishTrigger))
            {
                StopMovement();
                StartCinemachinePathMovement();
            }
        }

        private void StopMovement()
        {
            _isMoving = false;
            DOTween.Kill(transform); 
        }

        private void ShakeAndReset()
        {
            transform.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato)
                .OnComplete(() =>
                {
                    transform.DOMove(_startPosition, shakeDuration)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() => _isMoving = false);
                });
        }

        private void StartCinemachinePathMovement()
        {
            _isOnCinemachinePath = true;

            DOTween.Sequence()
                .Append(transform.DOLookAt(_cinemachinePath.EvaluatePosition(_cinemachinePath.MinPos), 0.1f))
                .Append(transform.DOMove(_cinemachinePath.EvaluatePosition(_cinemachinePath.MinPos), 0.5f))
                .OnComplete(() =>
                {
                    _cinemachineDollyCart.enabled = true;
                });
        }
    }
}
