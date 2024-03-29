using System.Collections.Generic;
using GI.UnityToolkit.Utilities;
using UnityEngine;

[RequireComponent(typeof(ConstantForce))]
public class BallControls : MonoBehaviour
{
    [SerializeField] private float torqueAdd = 1f;
    [SerializeField] private float maxAngularVelocity = 50f;
    [SerializeField] private float spinForceMultiplier = 2f;
    [SerializeField] private float launchForceMultiplier = 2f;
    [SerializeField] private float inputHistoryDuration = 0.1f;

    private InputHandler _inputHandler;
    private Rigidbody _rb;
    private ConstantForce _force;
    private Camera _mainCamera;
    
    private Vector3 _axis;
    private bool _wasDragging = false;
    private Vector3 _inputPosition;
    private Vector3 _lastInputPosition;
    private Vector3 _constantForceToApply;

    private struct InputEntry
    {
        public Vector3 Delta;
        public float Time;
    }

    private readonly Queue<InputEntry> _inputQueue = new();
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _force = GetComponent<ConstantForce>();
        
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        _inputHandler = new MobileInputHandler();
#else
        _inputHandler = new PCInputHandler();
#endif
        
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            this.LogError("Could not find main camera!");
            return;
        }
        _axis = (transform.position - _mainCamera.transform.position).normalized;
        _axis = Quaternion.Euler(15f, 0, 0) * Vector3.forward;
    }

    private void OnEnable()
    {
        _inputQueue.Clear();
        _rb.constraints = RigidbodyConstraints.FreezePosition;
        _force.enabled = false;
    }

    private void Update()
    {
        _inputHandler.UpdateState(ref _inputPosition);
        var deltaPosition = _inputPosition - _lastInputPosition;
        if (_inputHandler.IsDragging)
        {
            _wasDragging = true;
            if (!deltaPosition.Equals(Vector3.zero))
            {
                var ballScreenPosition = _mainCamera.WorldToScreenPoint(transform.position);
                var invert = IsRight(_lastInputPosition, _inputPosition, ballScreenPosition);
                var amount = (invert ? -1f : 1f) * deltaPosition.magnitude * torqueAdd;
                _rb.AddTorque(_axis * amount);
                _rb.maxAngularVelocity = maxAngularVelocity;

                // Determines if the point is to the right of the vector going from start to end.
                bool IsRight(Vector3 start, Vector3 end, Vector3 point) {
                    return (end.x - start.x) * (point.y - start.y) - (end.y - start.y) * (point.x - start.x) < 0;
                }
            }

            _inputQueue.Enqueue(new InputEntry
            {
                Delta = _inputPosition - _lastInputPosition, Time = Time.time
            });
        }
        else if (_wasDragging)
        {
            _rb.constraints = RigidbodyConstraints.None;
            _rb.AddTorque(Vector3.right * 5f);

            _inputQueue.Enqueue(new InputEntry
            {
                Delta = _inputPosition - _lastInputPosition, Time = Time.time
            });
            
            var deltaDirection = Vector3.zero;
            while (_inputQueue.Count > 0)
            {
                var entry = _inputQueue.Dequeue();
                deltaDirection += entry.Delta;
            }
            
            _rb.AddForce(new Vector3(deltaDirection.x, 0, deltaDirection.y) * launchForceMultiplier, ForceMode.Impulse);
            enabled = false;
            _constantForceToApply = Vector3.left * (_rb.angularVelocity.z * spinForceMultiplier);
        }
        _lastInputPosition = _inputPosition;

        while (_inputQueue.Count > 0 && Time.time - _inputQueue.Peek().Time >= inputHistoryDuration)
        {
            _inputQueue.Dequeue();
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Floor") == false) return;
        _force.enabled = true;
        _force.force = _constantForceToApply;
        _force.relativeForce = Vector3.forward * 10f;
    }
}