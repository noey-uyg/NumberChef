using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager : Singleton<BlockManager>
{
    [SerializeField] private RectTransform _deadLineRect;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Transform _transform;
    [SerializeField] Color[] _colors;

    private readonly float _leftOffset = 0.1f;
    private readonly float _rightOffset = 0.9f;

    private List<NumberBlock> _blocks = new List<NumberBlock>();

    private float _deadLine;
    private float _downDistance;
    private float _downDuration;
    private float _spawnDuration;
    private float _lastSpawnTime = 0f;

    private bool _gameStart;

    private void Start()
    {
        Vector3[] deadLineCorners = new Vector3[4];
        _deadLineRect.GetWorldCorners(deadLineCorners);
        _deadLine = deadLineCorners[1].y;

        _gameStart = true;
        ScoreManager.Reset();
        DifficultyManager.Start();
        _spawnDuration = DifficultyManager.GetCurrentSpawnDuration();
    }

    private void Update()
    {
        if (!_gameStart)
            return;

        _lastSpawnTime += Time.deltaTime;

        if (_lastSpawnTime >= _spawnDuration)
        {
            _spawnDuration = DifficultyManager.GetCurrentSpawnDuration();
            SpawnNumberBlock();
            _lastSpawnTime = 0;
        }
    }

    public void SpawnNumberBlock()
    {
        float randomX = Random.Range(_leftOffset, _rightOffset);

        var spawnPos = _mainCamera.ViewportToWorldPoint(new Vector3(randomX, 1f, 0f));
        spawnPos.z = 0f;

        _downDistance = spawnPos.y - _deadLine;
        _downDuration = DifficultyManager.GetCurrentDownDuration();

        int randomNum = (int)Random.Range(
            DifficultyManager.GetCurrentMinNumber(),
            DifficultyManager.GetCurrentMaxNumber()+1
            );

        int randomColor = (int)Random.Range(0, _colors.Length);

        NumberBlock numberBlock = NumberBlockPool.Instance.GetNumberBlock();
        numberBlock.Transform.position = spawnPos;
        numberBlock.Transform.SetParent(_transform);
        numberBlock.SetNumberBlock(randomNum, _colors[randomColor]);
        numberBlock.StartMoveDown(_downDistance, _downDuration);

        _blocks.Add(numberBlock);
    }

    public void MatchNumberBlock(int result)
    {
        for(int i = 0; i < _blocks.Count; i++)
        {
            if (_blocks[i].Number == result)
            {
                _blocks[i].Match();
                _blocks.RemoveAt(i);
                ScoreManager.AddScore(1);
                break;
            }
        }
    }
}
