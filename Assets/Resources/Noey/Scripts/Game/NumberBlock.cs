using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class NumberBlock : MonoBehaviour
{
    [SerializeField] private Transform _transform;

    public Transform Transform { get { return _transform; } }

    private void SetNumberBlock()
    {

    }

    public void StartMoveDown(float distance, float duration)
    {
        _transform.DOMoveY(transform.position.y - distance, duration)
                .SetEase(Ease.Linear);
    }
}
