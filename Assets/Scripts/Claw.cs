using System;
using System.Collections;
using UnityEngine;

public class Claw : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float descentDistance;
    [SerializeField] private float descentDuration;
    [SerializeField] private float pauseDuration;
    [SerializeField] private HingeJoint[] hinges;
    [SerializeField] private float closeDuration;
    [SerializeField] private float closeTarget;
    [SerializeField] private float openTarget;

    
    private Vector2 _previousInput;
    private float _startHeight;
    private bool _extended;

    private void Start()
    {
        _startHeight = transform.position.y;
    }

    private void Update()
    {
        _previousInput = GetInput();
        
        if (Input.GetKeyDown(KeyCode.Space) && !_extended)
        {
            _extended = true;
            StartCoroutine(Move(_startHeight + descentDistance, descentDuration, OnDownComplete));
        }

        void OnDownComplete() => StartCoroutine(Delay(pauseDuration, OnDownDelayComplete));
        
        void OnDownDelayComplete() => StartCoroutine(Close(closeTarget, closeDuration, OnCloseComplete));
        
        void OnCloseComplete() => StartCoroutine(Delay(pauseDuration, OnCloseDelayComplete));

        void OnCloseDelayComplete() => StartCoroutine(Move(_startHeight, descentDuration, OnUpComplete));

        void OnUpComplete() => StartCoroutine(Delay(pauseDuration, OnUpDelayComplete));

        void OnUpDelayComplete() => StartCoroutine(Close(openTarget, closeDuration, OnOpenComplete));

        void OnOpenComplete() => _extended = false;
    }

    private void FixedUpdate()
    {
        if (_extended)
            return;
        
        Vector3 translation = new Vector3(_previousInput.x, 0, _previousInput.y) * (speed * Time.deltaTime);
        transform.Translate(translation, Space.World);
    }

    private static Vector2 GetInput() => new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

    private IEnumerator Move(float targetHeight, float duration, Action callback = null)
    {
        float startTime = Time.time;
        float startHeight = transform.position.y;

        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            transform.position = new Vector3(
                transform.position.x,
                Mathf.Lerp(startHeight, targetHeight, t),
                transform.position.z
                );
            
            yield return new WaitForEndOfFrame();
        }

        transform.position = new Vector3(transform.position.x, targetHeight, transform.position.z);
        
        callback?.Invoke();
    }

    private IEnumerator Close(float target, float duration, Action callback = null)
    {
        float[] startTargets = new float[hinges.Length];
        for (int i = 0; i < hinges.Length; i++)
            startTargets[i] = hinges[i].spring.targetPosition;

        float startTime = Time.time;

        JointSpring spring;

        while (Time.time < startTime + duration)
        {
            for (int i = 0; i < hinges.Length; i++)
            {
                HingeJoint joint = hinges[i];
                float t = (Time.time - startTime) / duration;
                spring = new JointSpring
                {
                    damper = joint.spring.damper,
                    spring = joint.spring.spring,
                    targetPosition = Mathf.Lerp(startTargets[i], target, t)
                };
                joint.spring = spring;
            }
            
            yield return new WaitForEndOfFrame();
        }

        foreach (HingeJoint joint in hinges)
        {
            spring = new JointSpring
            {
                damper = joint.spring.damper,
                spring = joint.spring.spring,
                targetPosition = joint.spring.targetPosition
            };
            
            joint.spring = spring;
        }

        callback?.Invoke();
    }

    private IEnumerator Delay(float time, Action callback = null)
    {
        yield return new WaitForSeconds(time);
        callback?.Invoke();
    }
}
