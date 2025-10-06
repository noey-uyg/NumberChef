using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class ExpressionGenerator 
{
    private static readonly int[] Numbers = Enumerable.Range(1, 10).ToArray();
    private static readonly char[] Ops = { '+', '-', '*', '/' };
    private static readonly System.Random Rand = new System.Random();

    private static readonly Dictionary<int, string> Cache = new Dictionary<int, string>();

    public static async Task<string> GenerateExpressionAsync(int target, int populationSize = 300, int generations = 300)
    {
        if(Cache.ContainsKey(target)) return Cache[target];

        return await Task.Run(() =>
        {
            List<ExpressionChromosome> population = InitializePoplation(populationSize);

            ExpressionChromosome best = null;
            double bestFitness = double.MaxValue;

            for (int gen = 0; gen < generations; gen++)
            {
                foreach (var c in population)
                {
                    c.EvaluateFitness(target);
                }

                population = population.OrderBy(c => c.Fitness).ToList();

                if (population[0].Fitness < bestFitness)
                {
                    bestFitness = population[0].Fitness;
                    best = population[0];
                }

                if (bestFitness < 0.001)
                    break;

                int survivors = (int)(populationSize * 0.3);
                var nextGen = new List<ExpressionChromosome>(population.Take(survivors));

                while (nextGen.Count < populationSize)
                {
                    var parent1 = nextGen[Rand.Next(survivors)];
                    var parent2 = nextGen[Rand.Next(survivors)];
                    var child = ExpressionChromosome.Crossover(parent1, parent2);

                    if (Rand.NextDouble() < 0.2)
                        child.Mutate();
                    nextGen.Add(child);
                }

                population = nextGen;
            }

            string expr = best?.Expression ?? "";
            Cache[target] = expr;
            return expr;
        });
    }

    private static List<ExpressionChromosome> InitializePoplation(int size)
    {
        List<ExpressionChromosome> pop = new List<ExpressionChromosome>(size);

        for(int i = 0; i < size; i++)
        {
            var shuffled = Numbers.OrderBy(_ => Rand.Next()).ToArray();
            var ops = Enumerable.Range(0, 9).Select(_ => Ops[Rand.Next(Ops.Length)]).ToArray();
            pop.Add(new ExpressionChromosome(shuffled, ops));
        }

        return pop;
    }

    private class ExpressionChromosome
    {
        public int[] Numbers;
        public char[] Ops;
        public double Fitness;
        public string Expression;

        public ExpressionChromosome(int[] nums, char[] ops)
        {
            Numbers = (int[])nums.Clone();
            Ops = (char[])ops.Clone();
            BuildExpression();
        }

        public void EvaluateFitness(int target)
        {
            if(!TryEvaluate(Expression, out double value)){
                Fitness = double.MaxValue;
                return;
            }

            Fitness = Math.Abs(value - target);
        }

        public static ExpressionChromosome Crossover(ExpressionChromosome a, ExpressionChromosome b)
        {
            int cut = Rand.Next(1, a.Ops.Length - 1);
            var newOps = a.Ops.Take(cut).Concat(b.Ops.Skip(cut)).ToArray();

            cut = Rand.Next(1, a.Numbers.Length - 1);
            var newNums = a.Numbers.Take(cut).Concat(b.Numbers.Skip(cut)).ToArray();

            newNums = FixNumbers(newNums);

            return new ExpressionChromosome(newNums, newOps);
        }

        public void Mutate()
        {
            int idx = Rand.Next(Ops.Length);
            Ops[idx] = Ops[Rand.Next(Ops.Length)];
            if (Rand.NextDouble() < 0.3)
                Numbers = Numbers.OrderBy(_ => Rand.Next()).ToArray();
            BuildExpression();
        }

        private void BuildExpression()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Numbers.Length; i++)
            {
                sb.Append(Numbers[i]);
                if(i<Ops.Length)
                {
                    sb.Append(Ops[i]);
                }

                Expression = sb.ToString();
            }
        }

        private static bool TryEvaluate(string expr,out double result)
        {
            try
            {
                var table = new DataTable();
                result = Convert.ToDouble(table.Compute(expr, ""));

                if (double.IsInfinity(result) || double.IsNaN(result))
                    return false;

                return true;
            }
            catch
            {
                result = 0;
                return false;
            }
        }

        private static int[] FixNumbers(int[] nums)
        {
            HashSet<int> used = new HashSet<int>();
            List<int> result = new List<int>();
            
            foreach(var v in nums)
            {
                if (!used.Contains(v) && v >= 1 && v <= 10)
                    result.Add(v);
            }

            for(int i = 1; i <= 10; i++)
            {
                if(!used.Contains(i))
                    result.Add(i);
            }

            return result.Take(10).ToArray();

        }
    }
}
