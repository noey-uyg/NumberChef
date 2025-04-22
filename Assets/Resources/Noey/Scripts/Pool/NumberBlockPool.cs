using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class NumberBlockPool : Singleton<NumberBlockPool>
{
    [SerializeField] private NumberBlock _numberBlockPrefab;

    private ObjectPool<NumberBlock> _numberBlockPool;

    private const int maxSize = 500;
    private const int initSize = 100;

    protected override void OnAwake()
    {
        _numberBlockPool = new ObjectPool<NumberBlock>(CreateNumberBlock, ActivateNumberBlock, DisableNumberBlock, DestroyNumberBlock, false, initSize, maxSize);
    }

    #region ÆÛÁñ ÇÁ·¹ÀÓ
    private NumberBlock CreateNumberBlock()
    {
        return Instantiate(_numberBlockPrefab);
    }

    private void ActivateNumberBlock(NumberBlock raw)
    {
        raw.gameObject.SetActive(true);
    }

    private void DisableNumberBlock(NumberBlock raw)
    {
        raw.gameObject.SetActive(false);
    }

    private void DestroyNumberBlock(NumberBlock raw)
    {
        Destroy(raw);
    }

    public NumberBlock GetNumberBlock()
    {
        NumberBlock raw = null;

        if (_numberBlockPool.CountActive >= maxSize)
        {
            raw = CreateNumberBlock();
        }
        else
        {
            raw = _numberBlockPool.Get();
        }

        return raw;
    }

    public void ReleaseNumberBlock(NumberBlock raw)
    {
        _numberBlockPool.Release(raw);
    }
    #endregion
}
