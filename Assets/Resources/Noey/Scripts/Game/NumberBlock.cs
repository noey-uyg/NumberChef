using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class NumberBlock : MonoBehaviour
{
    [SerializeField] private Transform _transform;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private TextMeshProUGUI _numText;

    private int _num;

    public Transform Transform { get { return _transform; } }
    public int Number { get { return _num; } }

    public void SetNumberBlock(int num, Color color)
    {
        _num = num;
        _numText.text = num.ToString();
        _spriteRenderer.color = color;
    }

    public void StartMoveDown(float distance, float duration)
    {
        _transform.DOMoveY(_transform.position.y - distance, duration)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    NumberBlockPool.Instance.ReleaseNumberBlock(this);
                });
    }
}
