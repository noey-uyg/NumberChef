using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : Singleton<PopupManager>
{
    private readonly Stack<PopupBase> _popupStack = new();

    [SerializeField] private Transform _popupRoot;
    [SerializeField] private List<PopupBase> _popupPrefabs;

    public void ShowPopup<T>(Action<T> onInit = null) where T : PopupBase
    {
        T prefab = GetPopupPrefab<T>();
        if (prefab == null)
        {
            return;
        }

        T instance = Instantiate(prefab, _popupRoot);
        onInit?.Invoke(instance);
        instance.Show();
        _popupStack.Push(instance);
    }

    public void HideTopPopup()
    {
        if (_popupStack.Count == 0) return;

        var top = _popupStack.Pop();
        top.Hide();
        Destroy(top.gameObject);
    }

    public void HideAllPopups()
    {
        while (_popupStack.Count > 0)
        {
            var popup = _popupStack.Pop();
            popup.Hide();
            Destroy(popup.gameObject);
        }
    }

    private T GetPopupPrefab<T>() where T : PopupBase
    {
        foreach (var p in _popupPrefabs)
        {
            if (p is T typedPopup)
                return typedPopup;
        }
        return null;
    }

    public bool HasOpenPopup => _popupStack.Count > 0;
}
