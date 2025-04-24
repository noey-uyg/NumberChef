using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;
using System.Text;

public enum LastInputType { None, Nubmer, Operator }

public class InputExpression : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _inputText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private List<NumberButton> _numberButtons;

    private StringBuilder _expressionBuilder = new StringBuilder();
    private LastInputType _lastInputType = LastInputType.None;
    private HashSet<int> uniqueNumbers = new HashSet<int>();

    public LastInputType LastInputType { get { return _lastInputType; }}

    private void Start()
    {
        GenerateUniqueNumbers();
        ScoreManager.OnUpdateScore += UpdateScore;
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
        _expressionBuilder.Append(number.ToString());
        UpdateInputField();
        _lastInputType = LastInputType.Nubmer;
    }

    public void OnOperatorClicked(string op)
    {
        _expressionBuilder.Append(op);
        UpdateInputField();
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
            string expression = _expressionBuilder.ToString();
            ExpressionEvaluator.Evaluate(expression, out int result);
            BlockManager.Instance.MatchNumberBlock(result);
            OnClear();
            _lastInputType = LastInputType.None;
        }
        catch
        {
            Debug.Log("잘못된 연산식");
        }
    }

    private void ClearExpression()
    {
        _expressionBuilder.Clear();
        UpdateInputField();
    }

    private void UpdateInputField()
    {
        _inputText.text = _expressionBuilder.ToString();
    }

    private void UpdateScore(int score)
    {
        _scoreText.text = score.ToString();
    }
}
