using System;
using UnityEngine;

public class Claw : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float descentDistance;
    [SerializeField] private float descentTime;

    private Rigidbody _rigidbody;
    private Vector2 _previousInput;
    private float _startHeight;

    private void Awake()
    {
        TryGetComponent(out _rigidbody);
    }

    private void Start()
    {
        _startHeight = _rigidbody.transform.position.y;
    }

    private void Update()
    {
        _previousInput = GetInput();
    }

    private void FixedUpdate()
    {
        Vector3 translation = new Vector3(_previousInput.x, 0, _previousInput.y) * (speed * Time.deltaTime);
        transform.Translate(translation, Space.World);
    }

    private Vector2 GetInput() => new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
}
