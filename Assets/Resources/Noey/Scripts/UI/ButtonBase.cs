using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class ButtonBase : MonoBehaviour
{
    [SerializeField] protected Button _button;
    [SerializeField] protected TextMeshProUGUI _buttonText;

    protected virtual void Awake()
    {
        if( _button != null )
        {
            _button = GetComponent<Button>();
        }

        _button.onClick.AddListener(OnClicked);
    }

    protected abstract void OnClicked();
}
