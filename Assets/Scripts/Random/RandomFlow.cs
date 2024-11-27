using System;
using System.Collections.Generic;
using UnityEngine;

public static class RandomFlow
{
    public class RandomCache
    {
        // Cache of random numbers
        public List<Beaker> colorBeakers = new();
        public List<Beaker> emptyBeakers = new();
        public Dictionary<int, int> colors = new();

        public RandomCache Clone()
        {
            RandomCache cache = new RandomCache();
            foreach (var beaker in colorBeakers)
            {
                cache.colorBeakers.Add(new Beaker(beaker));
            }
            foreach (var beaker in emptyBeakers)
            {
                cache.emptyBeakers.Add(new Beaker(beaker));
            }
            foreach (var color in colors)
            {
                cache.colors.Add(color.Key, color.Value);
            }
            return cache;
        }

        public void Asigned(RandomCache cache)
        {
            colorBeakers = cache.colorBeakers;
            emptyBeakers = cache.emptyBeakers;
            colors = cache.colors;
        }
    }

    public class RandomResult
    {
        public enum ResultType
        {
            Success,
            Fail
        }
        public ResultType type = ResultType.Success;
        public List<Beaker> beakers = new List<Beaker>();
        public List<StepAction> actions;
    }

    /*
    todo 创建有色瓶子，无色瓶子
    todo 根据有色水瓶数量，和颜色总数，随机所有颜色
    todo 通过控制器数据，填充瓶子数据。
    todo 其余颜色随机填充。
    */
    public static RandomResult Excute(RandomSystemData data)
    {
        System.Random random = new System.Random();
        RandomResult randomResult = new RandomResult();
        RandomCache randomCache = new RandomCache();
        // 创建有色瓶子，无色瓶子
        for (int i = 0; i < data.coloredTubes; i++)
        {
            Beaker beaker = new Beaker();
            randomCache.colorBeakers.Add(beaker);
        }
        for (int i = 0; i < data.noColoredTubes; i++)
        {
            Beaker beaker = new Beaker();
            randomCache.emptyBeakers.Add(beaker);
        }
        // 根据有色水瓶数量，算出颜色总数。
        // 先填充固定颜色，其他时候随机
        for (int i = 0; i < data.colors.Count; i++)
        {
            randomCache.colors.Add(data.colors[i], Beaker.maxCapacity);
        }
        for (int i = randomCache.colors.Count; i < data.coloredTubes; i++)
        {
            int randomColor = data.colors[random.Next(0, data.colors.Count)];
            randomCache.colors[randomColor] += Beaker.maxCapacity;
        }
        // 通过控制器数据，填充瓶子数据。
        foreach (var controller in data.controllers)
        {
            controller.Exucte(data, randomCache, random);
        }
        // 其余颜色随机填充。
        // 取出空的有色瓶子, 剩的颜色随机填充。
        List<Beaker> emptyBeakers = new List<Beaker>();
        foreach (var beaker in randomCache.colorBeakers)
        {
            if (beaker.Contents.Count >= Beaker.maxCapacity)
            {
                continue;
            }
            emptyBeakers.Add(beaker);
        }
        while (emptyBeakers.Count > 0)
        {
            Beaker beaker = emptyBeakers[0];
            List<int> colorList = new List<int>();
            foreach (var color in randomCache.colors)
            {
                if (color.Value > 0)
                {
                    colorList.Add(color.Key);
                }
            }
            if (colorList.Count <= 0)
            {
                randomResult.type = RandomResult.ResultType.Fail;
                return randomResult;
            }
            int randomColor = colorList[random.Next(0, colorList.Count)];
            if (beaker.Contents.Count == Beaker.maxCapacity - 1)
            {
                bool isSameColor = true;
                foreach (int beakerColor in beaker.Contents)
                {
                    if (beakerColor != randomColor)
                    {
                        isSameColor = false;
                        break;
                    }
                }
                if (isSameColor)
                {
                    // 额外随机一个不同颜色
                    // 找到不同的颜色列表
                    List<int> differentColors = new List<int>();
                    foreach (var color in randomCache.colors)
                    {
                        if (color.Value > 0 && color.Key != randomColor)
                        {
                            differentColors.Add(color.Key);
                        }
                    }
                    if (differentColors.Count == 0)
                    {
                        randomResult.type = RandomResult.ResultType.Fail;
                        return randomResult;
                    }
                    randomColor = differentColors[random.Next(0, differentColors.Count)];
                }
            }
            beaker.Contents.Push(randomColor);
            randomCache.colors[randomColor]--;
            if (beaker.Contents.Count >= Beaker.maxCapacity)
            {
                emptyBeakers.Remove(beaker);
            }
        }
        // 填充result 
        randomResult.beakers.AddRange(randomCache.colorBeakers);
        randomResult.beakers.AddRange(randomCache.emptyBeakers);
        // 测试step action  // Tuple<ECalculateResult, string, State, List<StepAction>>
        var stepResult = Solver.CalculateStep(randomResult.beakers);
        if (stepResult.Item1 == Solver.ECalculateResult.Success)
        {
            randomResult.actions = stepResult.Item4;
        }
        else
        {
            randomResult.type = RandomResult.ResultType.Fail;
        }
        return randomResult;
    }
}