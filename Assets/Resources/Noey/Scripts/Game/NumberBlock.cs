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
    private Tween _moveYTween;

    public Transform Transform { get { return _transform; } }
    public int Number { get { return _num; } }

    public void SetNumberBlock(int num, Color color)
    {
        _num = num;
        _numText.text = num.ToString();
        _spriteRenderer.color = color;

        GameManager.Instance.OnStateChanged += GameStateChange;
    }

    public void StartMoveDown(float distance, float duration)
    {
        if(_moveYTween != null)
        {
            _moveYTween.Kill();
        }

        _moveYTween = _transform.DOMoveY(_transform.position.y - distance, duration)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    NumberBlockPool.Instance.ReleaseNumberBlock(this);
                    GameManager.Instance.OnStateChanged -= GameStateChange;
                });
    }

    public void Match()
    {
        if (_moveYTween != null)
        {
            _moveYTween.Kill();
        }

        NumberBlockPool.Instance.ReleaseNumberBlock(this);
        GameManager.Instance.OnStateChanged -= GameStateChange;
    }

    private void GameStateChange(GameManager.GameState state)
    {
        if (_moveYTween == null || !_moveYTween.IsActive())
            return;

        switch (state)
        {
            case GameManager.GameState.Paused:
                {
                    _moveYTween.Pause();
                }
                break;
            case GameManager.GameState.Playing:
                {
                    _moveYTween.Play();
                }
                break;
        }
    }
}
