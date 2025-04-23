using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : Singleton<SpawnManager>
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Transform _transform;
    [SerializeField] Color[] _colors;

    private readonly float _leftOffset = 0.1f;
    private readonly float _rightOffset = 0.9f;

    private WaitForSeconds _spawnDuration = new WaitForSeconds(1f);
    private float _downDistance;
    private float _downDuration;

    private float _minNum=1;
    private float _maxNum=100;

    public void Start()
    {
        StartCoroutine(IEStartSpawn());
    }

    IEnumerator IEStartSpawn()
    {
        while (true)
        {
            yield return _spawnDuration;

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

        int randomNum = (int)Random.Range(_minNum, _maxNum+1);
        int randomColor = (int)Random.Range(0, _colors.Length);

        NumberBlock numberBlock = NumberBlockPool.Instance.GetNumberBlock();
        numberBlock.Transform.position = spawnPos;
        numberBlock.Transform.SetParent(_transform);
        numberBlock.SetNumberBlock(randomNum, _colors[randomColor]);
        numberBlock.StartMoveDown(_downDistance, _downDuration);
    }
}
