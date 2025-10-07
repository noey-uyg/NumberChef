using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 1~10의 숫자와 + - * / 연산자를 이용해 주어진 target 정수와 정확히 일치하는 수식을 찾음
/// 유전 알고리즘을 사용하여 수식을 탐색하고, 찾지 못하면 재시도를 한다.
/// 찾은 수식 문자열 또는 빈 문자열을 반환한다.
/// </summary>
public static class ExpressionGenerator 
{
    // 사용할 숫자 집합
    private static readonly int[] Numbers = Enumerable.Range(1, 10).ToArray();
    // 사용할 연산자 집합
    private static readonly char[] Ops = { '+', '-', '*', '/' };
    private static readonly System.Random Rand = new System.Random();

    private static readonly Dictionary<int, string> Cache = new Dictionary<int, string>();
    public static async Task<string> GenerateExpressionAsync(int target, int populationSize = 300, int generations = 300, int maxRetries = 10)
    {
        // 이미 캐시된 결과가 있다면 바로 반환
        if (Cache.TryGetValue(target, out string cached))
            return cached;

        string expr = "";
        int attempt = 0;

        // 실패하면 재시도
        while (string.IsNullOrEmpty(expr) && attempt < maxRetries)
        {
            attempt++;
            expr = await TryGenerateOnce(target, populationSize, generations);
        }

        // 찾지 못한 경우 경고 및 빈 문자열 반환
        if (string.IsNullOrEmpty(expr))
        {
            Debug.LogWarning($"[ExpressionGenerator] Failed to find exact expression for {target} after {maxRetries} tries.");
            return "";
        }

        // 찾은 경우 캐싱 및 반환
        Cache[target] = expr;
        return expr;
    }

    /// <summary>
    /// 하나의 유전 알고리즘 수행을 Task로 실행(백그라운드 스레드)
    /// Population 초기화 후 generations만큼 반복(평가, 선택, 교배, 돌연변이)
    /// 수식과 일치하는 값 발견 시 즉시 반환
    /// </summary>
    private static async Task<string> TryGenerateOnce(int target, int populationSize, int generations)
    {
        return await Task.Run(() =>
        {
            // 초기 인구 생성(무작위성)
            var population = InitializePopulation(populationSize);
            ExpressionChromosome best = null;

            for (int gen = 0; gen < generations; gen++)
            {
                // 각 개체의 fitness 평가(함수 내부에서 수식 평가 수행)
                foreach (var c in population)
                    c.EvaluateFitness(target);

                // fitnee가 0인(target과 정확히 일치할 경우) 개체가 있으면 best로 삼고 종료
                var exactMatch = population.FirstOrDefault(p => p.Fitness == 0);
                if (exactMatch != null)
                {
                    best = exactMatch;
                    break;
                }

                // fitness 기준 정렬(작을수록 좋은 값임)
                population = population.OrderBy(c => c.Fitness).ToList();

                // 상위 일부 보존
                int survivors = (int)(populationSize * 0.3);
                var nextGen = new List<ExpressionChromosome>(population.Take(survivors));

                // 나머지는 교배와 돌연변이로 채움
                while (nextGen.Count < populationSize)
                {
                    // 부모 선택 : 보존된 그룹 내 랜덤 선택
                    var parent1 = nextGen[Rand.Next(survivors)];
                    var parent2 = nextGen[Rand.Next(survivors)];
                    var child = ExpressionChromosome.Crossover(parent1, parent2);
                    if (Rand.NextDouble() < 0.3)
                        child.Mutate();
                    nextGen.Add(child);
                }

                population = nextGen;
                // 다음 세대 반복
            }

            // best가 null일 경우는 못찾은 경우이므로 빈 문자열 반환
            return best?.Expression ?? "";
        });
    }

    /// <summary>
    /// populaationSize개의 임의 개체 생성
    /// 각 개체는 무작위 숫자 배열과 무작위 연산자 배열로 생성된다.
    /// </summary>
    private static List<ExpressionChromosome> InitializePopulation(int size)
    {
        var pop = new List<ExpressionChromosome>(size);
        for (int i = 0; i < size; i++)
        {
            // 숫자 무작위 섞기
            var shuffled = Numbers.OrderBy(_ => Rand.Next()).ToArray();
            
            // 연산자 9개 무작위 선택(숫자 10개를 모두 사용할 경우 연산자는 9개이기 때문)
            var ops = Enumerable.Range(0, 9).Select(_ => Ops[Rand.Next(Ops.Length)]).ToArray();
            pop.Add(new ExpressionChromosome(shuffled, ops));
        }
        return pop;
    }

    /// <summary>
    /// 각 개체는 숫자배열과 연산자 배열을 가짐
    /// Expression 문자열은 BuildExpression으로 만듬
    /// EvaluateFitness는 Expression을 정수 방식으로 계산해서 target과 비교
    /// </summary>
    private class ExpressionChromosome
    {
        public int[] Numbers;
        public char[] Ops;
        public int Fitness; // 0이면 target과 정확히 일치
        public string Expression; // 수식의 문자열

        /// <summary>
        /// 생성자로 nums는 외부에서 전달된 배열
        /// Distinct().Take(10)로 중복 제거 및 길이 보장 시도(숫자 개수가 10보다 작아질 수 있음)
        /// </summary>
        public ExpressionChromosome(int[] nums, char[] ops)
        {
            // 중복 제거 후 최대 10개까지만 사용(초기 population에선 10개가 보장)
            Numbers = nums.Distinct().Take(10).ToArray();
            Ops = (char[])ops.Clone();
            BuildExpression();
        }

        /// <summary>
        /// Expression을 정수 방식으로 평가
        /// 잘못된 나눗셈은 예외로 처리하여 해당 게체는 무효화
        /// 결과가 target과 정확히 같으면 Fitness = 0
        /// 같지 않으면 Fitness = value- target
        /// </summary>
        public void EvaluateFitness(int target)
        {
            if (!TryEvaluate(Expression, out int value))
            {
                // 유효하지 않은 수식(나눗셈 오류[소수점 동반] 등)인 경우 매우 큰 수치로 패널티
                Fitness = int.MaxValue;
                return;
            }

            // 정확히 맞는 경우만 true로 봄
            Fitness = (value == target) ? 0 : Math.Abs(value - target);
        }

        /// <summary>
        /// 부모 a, b의 일부를 결합하여 자식 생성
        /// 숫자 배열은 a의 앞부분 + b의 뒷부분을 연결하고 Distinct로 중복 제거
        /// 연산자는 각 인덱스 별로 절반 확률로 부모 중 하나의 연산자 선택
        /// </summary>
        public static ExpressionChromosome Crossover(ExpressionChromosome a, ExpressionChromosome b)
        {
            // 숫자 연결 지점 선택
            int cut = Rand.Next(1, a.Numbers.Length - 1);

            // a의 앞부분 + b의 뒷부분 연결, 중복 제거, 최대 10개
            var newNums = a.Numbers.Take(cut).Concat(b.Numbers.Skip(cut)).Distinct().Take(10).ToArray();

            // 연산자 : 인덱스 별로 랜덤 선택
            var newOps = new char[9];
            for (int i = 0; i < newOps.Length; i++)
                newOps[i] = (Rand.NextDouble() < 0.5) ? a.Ops[i] : b.Ops[i];

            return new ExpressionChromosome(newNums, newOps);
        }

        /// <summary>
        /// 연산자 한 칸을 랜덤으로 변경
        /// 확률적으로 숫자 전체를 무작위로 섞음
        /// BuildExpression으로 Expression 갱신
        /// </summary>
        public void Mutate()
        {
            // 연산자 랜덤 변경
            int idx = Rand.Next(Ops.Length);
            Ops[idx] = Ops[Rand.Next(Ops.Length)];

            // 숫자 랜덤 섞기
            if (Rand.NextDouble() < 0.3)
                Numbers = Numbers.OrderBy(_ => Rand.Next()).ToArray();

            BuildExpression();
        }

        /// <summary>
        /// Numbers와 Ops를 이어붙여 문자열 Expression 생성\
        /// 교배로 인해서 Numbers.Length가 10이 아닐 수 있음 이 경우 생성된 식은 숫자 개수가 줄어듬
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
        /// EvaluateExpression을 호출하고 예외처리
        /// 유효하면 result값과 함께 true반환, 오동작일 때 false 반환
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
        /// 문자열 형태의 괄호 없는 수식을 토큰화
        /// *와 / 연산을 왼쪽->오른쪽으로 먼저 처리(좌결합)
        /// 이후 +와 - 연산을 왼쪽->오른쪽으로 처리
        /// 나눗셈은 정수 나눗셈만 허용함 (왼쪽%오른쪽 == 0)
        /// 조건 위배 시 예외 발생 -> 개체 무효
        /// </summary>
        private static int EvaluateExpression(string expr)
        {
            // 토큰 분리 : 숫자(연속된 숫자 문자)와 연산자(단일 문자)를 리스트로 분리
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

            // 먼저 *와 /처리(인덱스 순회하면서 연산자 만날 때마다 처리하여 토큰 축소)
            for (int i = 0; i < tokens.Count;)
            {
                if (tokens[i] == "*" || tokens[i] == "/")
                {
                    int left = int.Parse(tokens[i - 1]);
                    int right = int.Parse(tokens[i + 1]);

                    // 정수 나눗셈 검증
                    if (tokens[i] == "/" && (right == 0 || left % right != 0))
                        throw new Exception("Invalid division");

                    int res = (tokens[i] == "*") ? left * right : left / right;

                    // 왼쪽 결과로 치환하고 연산자와 오른쪽 피연산자 제거
                    tokens[i - 1] = res.ToString();
                    tokens.RemoveAt(i); // 연산자
                    tokens.RemoveAt(i); // 오른쪽 숫자
                    continue; // 같은 인덱스에서 다시 검사(연속되는 * / 처리하기 위해)
                }
                i++;
            }

            // 남은 토큰은 +와 0 밖에 없으므로 좌측부터 평가
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
