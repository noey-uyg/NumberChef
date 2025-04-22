using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : Singleton<SpawnManager>
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Transform _transform;

    private readonly float _leftOffset = 0.1f;
    private readonly float _rightOffset = 0.9f;

    private float _spawnDuration;
    private float _downDistance;
    private float _downDuration;

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SpawnNumberBlock();
        }
    }

    public void SpawnNumberBlock()
    {
        float randomX = Random.Range(_leftOffset, _rightOffset);

        var spawnPos = _mainCamera.ViewportToWorldPoint(new Vector3(randomX, 1f, 0f));
        spawnPos.z = 0f;

        _downDistance = spawnPos.y - _mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).y;
        _downDuration = 15f;

        NumberBlock numberBlock = NumberBlockPool.Instance.GetNumberBlock();
        numberBlock.Transform.position = spawnPos;
        numberBlock.Transform.SetParent(_transform);
        numberBlock.StartMoveDown(_downDistance, _downDuration);
    }
}
