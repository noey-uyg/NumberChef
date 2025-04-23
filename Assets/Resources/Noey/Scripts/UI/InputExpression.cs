using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;

public enum LastInputType { None, Nubmer, Operator }

public class InputExpression : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _inputText;
    [SerializeField] private List<NumberButton> _numberButtons;

    private string _expression = "";
    private LastInputType _lastInputType = LastInputType.None;
    private HashSet<int> uniqueNumbers = new HashSet<int>();
    private Vector3[] _bottomCorners = new Vector3[4];

    public LastInputType LastInputType { get { return _lastInputType; }}

    private void Start()
    {
        GenerateUniqueNumbers();
    }

    private void GenerateUniqueNumbers()
    {
        uniqueNumbers.Clear();

        while (uniqueNumbers.Count < _numberButtons.Count)
        {
            int rand = Random.Range(1, 11);
            uniqueNumbers.Add(rand);
        }

        int index = 0;
        foreach (var number in uniqueNumbers)
        {
            _numberButtons[index].Init(number);
            _numberButtons[index].ResetState();
            index++;
        }

        ClearExpression();
    }

    public void OnNumberClicked(int number)
    {
        _expression += number.ToString();
        _inputText.text = _expression;
        _lastInputType = LastInputType.Nubmer;
    }

    public void OnOperatorClicked(string op)
    {
        _expression += $" {op} ";
        _inputText.text = _expression;
        _lastInputType = LastInputType.Operator;
    }

    public void OnClear()
    {
        for(int i=0;i< _numberButtons.Count; i++)
        {
            _numberButtons[i].ResetState();
        }
        ClearExpression();
        _lastInputType = LastInputType.None;
    }

    public void OnSubmit()
    {
        try
        {
            ExpressionEvaluator.Evaluate(_expression, out int result);
            Debug.Log(result);
            //BlockManager.Instance.TryRemoveBlockWithValue(result);
            OnClear();
        }
        catch
        {
            Debug.Log("Invalid Expression");
            return;
        }

        OnClear();
        _lastInputType = LastInputType.None;
    }

    private void ClearExpression()
    {
        _expression = "";
        _inputText.text = "";
    }
}
