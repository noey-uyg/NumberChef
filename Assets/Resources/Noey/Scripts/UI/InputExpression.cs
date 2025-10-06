using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;
using System.Text;
using System.Threading.Tasks;

public enum LastInputType { None, Nubmer, Operator }

public class InputExpression : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _inputText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private List<NumberButton> _numberButtons;

    private StringBuilder _expressionBuilder = new StringBuilder();
    private LastInputType _lastInputType = LastInputType.None;
    private HashSet<int> uniqueNumbers = new HashSet<int>();

    private bool _autoSolving = false;
    private bool _autoPlaying = false;
    private bool _autoSubmit = false;

    public LastInputType LastInputType { get { return _lastInputType; }}

    private void Start()
    {
        GenerateUniqueNumbers();
        ScoreManager.OnUpdateScore += UpdateScore;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            _autoPlaying = !_autoPlaying;
        }
        if(Input.GetKeyDown(KeyCode.S))
        {
            _autoSubmit = !_autoSubmit;
        }

        if (_autoSolving || !_autoPlaying) return;
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        if (!string.IsNullOrEmpty(_inputText.text))
            return;

        if(BlockManager.Instance.Blocks.Count > 0)
        {
            int target = BlockManager.Instance.Blocks[0].Number;
            _ = AutoSolveAndSubmit(target);
        }
    }

    private async Task AutoSolveAndSubmit(int target)
    {
        _autoSolving = true;
        Debug.Log($"[Auto]{target}... Find expression");

        string expr = await ExpressionGenerator.GenerateExpressionAsync(target);

        if (!string.IsNullOrEmpty(expr))
        {
            Debug.Log($"[Auto] Find expression for {target}: {expr}");
            AutoInputAndSubmit(expr);
        }
        else
        {
            Debug.LogWarning($"[Auto] Failed to find expression for {target}");
        }

        _autoSolving = false;
    }

    public void AutoInputAndSubmit(string expression)
    {
        if (string.IsNullOrEmpty(expression))
            return;

        _expressionBuilder.Clear();
        _expressionBuilder.Append(expression);
        UpdateInputField();

        if (_autoSubmit)
        {
            OnSubmit();
        }
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
        if (_autoPlaying) return;

        _expressionBuilder.Append(number.ToString());
        UpdateInputField();
        _lastInputType = LastInputType.Nubmer;
    }

    public void OnOperatorClicked(string op)
    {
        if (_autoPlaying) return;

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

            BlockManager.Instance.MatchNumberBlock(result, expression);

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
