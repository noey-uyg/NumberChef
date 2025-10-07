using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 1~10�� ���ڿ� + - * / �����ڸ� �̿��� �־��� target ������ ��Ȯ�� ��ġ�ϴ� ������ ã��
/// ���� �˰����� ����Ͽ� ������ Ž���ϰ�, ã�� ���ϸ� ��õ��� �Ѵ�.
/// ã�� ���� ���ڿ� �Ǵ� �� ���ڿ��� ��ȯ�Ѵ�.
/// </summary>
public static class ExpressionGenerator 
{
    // ����� ���� ����
    private static readonly int[] Numbers = Enumerable.Range(1, 10).ToArray();
    // ����� ������ ����
    private static readonly char[] Ops = { '+', '-', '*', '/' };
    private static readonly System.Random Rand = new System.Random();

    private static readonly Dictionary<int, string> Cache = new Dictionary<int, string>();
    public static async Task<string> GenerateExpressionAsync(int target, int populationSize = 300, int generations = 300, int maxRetries = 10)
    {
        // �̹� ĳ�õ� ����� �ִٸ� �ٷ� ��ȯ
        if (Cache.TryGetValue(target, out string cached))
            return cached;

        string expr = "";
        int attempt = 0;

        // �����ϸ� ��õ�
        while (string.IsNullOrEmpty(expr) && attempt < maxRetries)
        {
            attempt++;
            expr = await TryGenerateOnce(target, populationSize, generations);
        }

        // ã�� ���� ��� ��� �� �� ���ڿ� ��ȯ
        if (string.IsNullOrEmpty(expr))
        {
            Debug.LogWarning($"[ExpressionGenerator] Failed to find exact expression for {target} after {maxRetries} tries.");
            return "";
        }

        // ã�� ��� ĳ�� �� ��ȯ
        Cache[target] = expr;
        return expr;
    }

    /// <summary>
    /// �ϳ��� ���� �˰��� ������ Task�� ����(��׶��� ������)
    /// Population �ʱ�ȭ �� generations��ŭ �ݺ�(��, ����, ����, ��������)
    /// ���İ� ��ġ�ϴ� �� �߰� �� ��� ��ȯ
    /// </summary>
    private static async Task<string> TryGenerateOnce(int target, int populationSize, int generations)
    {
        return await Task.Run(() =>
        {
            // �ʱ� �α� ����(��������)
            var population = InitializePopulation(populationSize);
            ExpressionChromosome best = null;

            for (int gen = 0; gen < generations; gen++)
            {
                // �� ��ü�� fitness ��(�Լ� ���ο��� ���� �� ����)
                foreach (var c in population)
                    c.EvaluateFitness(target);

                // fitnee�� 0��(target�� ��Ȯ�� ��ġ�� ���) ��ü�� ������ best�� ��� ����
                var exactMatch = population.FirstOrDefault(p => p.Fitness == 0);
                if (exactMatch != null)
                {
                    best = exactMatch;
                    break;
                }

                // fitness ���� ����(�������� ���� ����)
                population = population.OrderBy(c => c.Fitness).ToList();

                // ���� �Ϻ� ����
                int survivors = (int)(populationSize * 0.3);
                var nextGen = new List<ExpressionChromosome>(population.Take(survivors));

                // �������� ����� �������̷� ä��
                while (nextGen.Count < populationSize)
                {
                    // �θ� ���� : ������ �׷� �� ���� ����
                    var parent1 = nextGen[Rand.Next(survivors)];
                    var parent2 = nextGen[Rand.Next(survivors)];
                    var child = ExpressionChromosome.Crossover(parent1, parent2);
                    if (Rand.NextDouble() < 0.3)
                        child.Mutate();
                    nextGen.Add(child);
                }

                population = nextGen;
                // ���� ���� �ݺ�
            }

            // best�� null�� ���� ��ã�� ����̹Ƿ� �� ���ڿ� ��ȯ
            return best?.Expression ?? "";
        });
    }

    /// <summary>
    /// populaationSize���� ���� ��ü ����
    /// �� ��ü�� ������ ���� �迭�� ������ ������ �迭�� �����ȴ�.
    /// </summary>
    private static List<ExpressionChromosome> InitializePopulation(int size)
    {
        var pop = new List<ExpressionChromosome>(size);
        for (int i = 0; i < size; i++)
        {
            // ���� ������ ����
            var shuffled = Numbers.OrderBy(_ => Rand.Next()).ToArray();
            
            // ������ 9�� ������ ����(���� 10���� ��� ����� ��� �����ڴ� 9���̱� ����)
            var ops = Enumerable.Range(0, 9).Select(_ => Ops[Rand.Next(Ops.Length)]).ToArray();
            pop.Add(new ExpressionChromosome(shuffled, ops));
        }
        return pop;
    }

    /// <summary>
    /// �� ��ü�� ���ڹ迭�� ������ �迭�� ����
    /// Expression ���ڿ��� BuildExpression���� ����
    /// EvaluateFitness�� Expression�� ���� ������� ����ؼ� target�� ��
    /// </summary>
    private class ExpressionChromosome
    {
        public int[] Numbers;
        public char[] Ops;
        public int Fitness; // 0�̸� target�� ��Ȯ�� ��ġ
        public string Expression; // ������ ���ڿ�

        /// <summary>
        /// �����ڷ� nums�� �ܺο��� ���޵� �迭
        /// Distinct().Take(10)�� �ߺ� ���� �� ���� ���� �õ�(���� ������ 10���� �۾��� �� ����)
        /// </summary>
        public ExpressionChromosome(int[] nums, char[] ops)
        {
            // �ߺ� ���� �� �ִ� 10�������� ���(�ʱ� population���� 10���� ����)
            Numbers = nums.Distinct().Take(10).ToArray();
            Ops = (char[])ops.Clone();
            BuildExpression();
        }

        /// <summary>
        /// Expression�� ���� ������� ��
        /// �߸��� �������� ���ܷ� ó���Ͽ� �ش� ��ü�� ��ȿȭ
        /// ����� target�� ��Ȯ�� ������ Fitness = 0
        /// ���� ������ Fitness = value- target
        /// </summary>
        public void EvaluateFitness(int target)
        {
            if (!TryEvaluate(Expression, out int value))
            {
                // ��ȿ���� ���� ����(������ ����[�Ҽ��� ����] ��)�� ��� �ſ� ū ��ġ�� �г�Ƽ
                Fitness = int.MaxValue;
                return;
            }

            // ��Ȯ�� �´� ��츸 true�� ��
            Fitness = (value == target) ? 0 : Math.Abs(value - target);
        }

        /// <summary>
        /// �θ� a, b�� �Ϻθ� �����Ͽ� �ڽ� ����
        /// ���� �迭�� a�� �պκ� + b�� �޺κ��� �����ϰ� Distinct�� �ߺ� ����
        /// �����ڴ� �� �ε��� ���� ���� Ȯ���� �θ� �� �ϳ��� ������ ����
        /// </summary>
        public static ExpressionChromosome Crossover(ExpressionChromosome a, ExpressionChromosome b)
        {
            // ���� ���� ���� ����
            int cut = Rand.Next(1, a.Numbers.Length - 1);

            // a�� �պκ� + b�� �޺κ� ����, �ߺ� ����, �ִ� 10��
            var newNums = a.Numbers.Take(cut).Concat(b.Numbers.Skip(cut)).Distinct().Take(10).ToArray();

            // ������ : �ε��� ���� ���� ����
            var newOps = new char[9];
            for (int i = 0; i < newOps.Length; i++)
                newOps[i] = (Rand.NextDouble() < 0.5) ? a.Ops[i] : b.Ops[i];

            return new ExpressionChromosome(newNums, newOps);
        }

        /// <summary>
        /// ������ �� ĭ�� �������� ����
        /// Ȯ�������� ���� ��ü�� �������� ����
        /// BuildExpression���� Expression ����
        /// </summary>
        public void Mutate()
        {
            // ������ ���� ����
            int idx = Rand.Next(Ops.Length);
            Ops[idx] = Ops[Rand.Next(Ops.Length)];

            // ���� ���� ����
            if (Rand.NextDouble() < 0.3)
                Numbers = Numbers.OrderBy(_ => Rand.Next()).ToArray();

            BuildExpression();
        }

        /// <summary>
        /// Numbers�� Ops�� �̾�ٿ� ���ڿ� Expression ����\
        /// ����� ���ؼ� Numbers.Length�� 10�� �ƴ� �� ���� �� ��� ������ ���� ���� ������ �پ��
        /// </summary>
        private void BuildExpression()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Numbers.Length; i++)
            {
                sb.Append(Numbers[i]);
                if (i < Ops.Length)
                    sb.Append(Ops[i]);
            }
            Expression = sb.ToString();
        }

        /// <summary>
        /// EvaluateExpression�� ȣ���ϰ� ����ó��
        /// ��ȿ�ϸ� result���� �Բ� true��ȯ, �������� �� false ��ȯ
        /// </summary>
        private static bool TryEvaluate(string expr, out int result)
        {
            try
            {
                result = EvaluateExpression(expr);
                return true;
            }
            catch
            {
                result = 0;
                return false;
            }
        }

        /// <summary>
        /// ���ڿ� ������ ��ȣ ���� ������ ��ūȭ
        /// *�� / ������ ����->���������� ���� ó��(�°���)
        /// ���� +�� - ������ ����->���������� ó��
        /// �������� ���� �������� ����� (����%������ == 0)
        /// ���� ���� �� ���� �߻� -> ��ü ��ȿ
        /// </summary>
        private static int EvaluateExpression(string expr)
        {
            // ��ū �и� : ����(���ӵ� ���� ����)�� ������(���� ����)�� ����Ʈ�� �и�
            List<string> tokens = new();
            StringBuilder num = new();

            foreach (char c in expr)
            {
                if (char.IsDigit(c))
                    num.Append(c);
                else
                {
                    tokens.Add(num.ToString());
                    num.Clear();
                    tokens.Add(c.ToString());
                }
            }
            if (num.Length > 0)
                tokens.Add(num.ToString());

            // ���� *�� /ó��(�ε��� ��ȸ�ϸ鼭 ������ ���� ������ ó���Ͽ� ��ū ���)
            for (int i = 0; i < tokens.Count;)
            {
                if (tokens[i] == "*" || tokens[i] == "/")
                {
                    int left = int.Parse(tokens[i - 1]);
                    int right = int.Parse(tokens[i + 1]);

                    // ���� ������ ����
                    if (tokens[i] == "/" && (right == 0 || left % right != 0))
                        throw new Exception("Invalid division");

                    int res = (tokens[i] == "*") ? left * right : left / right;

                    // ���� ����� ġȯ�ϰ� �����ڿ� ������ �ǿ����� ����
                    tokens[i - 1] = res.ToString();
                    tokens.RemoveAt(i); // ������
                    tokens.RemoveAt(i); // ������ ����
                    continue; // ���� �ε������� �ٽ� �˻�(���ӵǴ� * / ó���ϱ� ����)
                }
                i++;
            }

            // ���� ��ū�� +�� 0 �ۿ� �����Ƿ� �������� ��
            int total = int.Parse(tokens[0]);
            for (int i = 1; i < tokens.Count; i += 2)
            {
                string op = tokens[i];
                int val = int.Parse(tokens[i + 1]);
                total = (op == "+") ? total + val : total - val;
            }

            return total;
        }
    }
}
