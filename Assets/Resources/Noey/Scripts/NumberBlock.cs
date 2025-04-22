using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class NumberBlock : MonoBehaviour
{
    [SerializeField] private Transform _transform;

    private readonly float _leftOffset = 0.1f;
    private readonly float _rightOffset = 0.9f;

    private Camera _mainCamera;
    private float _downDistance;
    private float _downDuration;

    private void Awake()
    {
        SetNumberBlock();
    }

    private void SetNumberBlock()
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        float randomX = Random.Range(_leftOffset, _rightOffset);
        var pos = _mainCamera.ViewportToWorldPoint(new Vector3(randomX, 1f, 0f));
        pos.z = 0f;
        _transform.position = pos;

        _downDistance = pos.y - _mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).y;
        _downDuration = 10f;
    }

    private void Start()
    {
        _transform.DOMoveY(transform.position.y - _downDistance, _downDuration)
                .SetEase(Ease.Linear);
    }
}
