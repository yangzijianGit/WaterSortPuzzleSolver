
#region 单个水瓶颜色数量控制器数据
using System;
using System.Collections.Generic;
using UnityEngine;

public class SingleTubesColorNumControllerData : BaseController
{
    public int TubesColorsNum;
    public int TubesNum;
    override public ControllerType GetControllerType()
    {
        return ControllerType.SingleTubesColor;
    }

    private bool ExcuteInternal(RandomSystemData data, RandomFlow.RandomCache randomCache, System.Random random)
    {
        // 找出符合条件的空的有色水瓶，然后随机填充颜色
        List<Beaker> emptyBeakers = new List<Beaker>();
        foreach (var tube in randomCache.colorBeakers)
        {
            if (tube.Contents.Count == 0)
            {
                emptyBeakers.Add(tube);
            }
        }
        List<Beaker> randomBeakers = new List<Beaker>();
        while (randomBeakers.Count < TubesNum && emptyBeakers.Count > 0)
        {
            int index = random.Next(0, emptyBeakers.Count);
            randomBeakers.Add(emptyBeakers[index]);
            emptyBeakers.RemoveAt(index);
        }
        // 寻找组合，满足条件的颜色组合
        // 颜色数量应该满足 TubesColorsNum
        // 色块应该足够
        int nIndex = 0;
        while (nIndex < randomBeakers.Count)
        {
            // 取出一个水瓶
            var beaker = randomBeakers[nIndex]; // 最终填满的是这个水瓶 
            int beakerColorsNum = Beaker.maxCapacity;
            // 随机出一种组合
            List<int> colorCount = new();
            // 先保证每种颜色都有一个
            for (int i = 0; i < TubesColorsNum; i++)
            {
                colorCount.Add(1);
            }
            int diffNum = beakerColorsNum - TubesColorsNum;
            // 把多余的，分散给其他的水瓶
            while (diffNum > 0)
            {
                int diffRandomIndex = random.Next(0, TubesColorsNum);
                int diffRandomValue = random.Next(0, diffNum + 1);
                colorCount[diffRandomIndex] += diffRandomValue;
                diffNum -= diffRandomValue;
            }
            // 随机出来的组合，去找颜色
            // 找需求最大颜色数量的组合 colorCount
            List<int> colorVerityKey = new List<int>();
            while (colorCount.Count > 0)
            {
                // 找到最大的 index
                int maxIndex = 0;
                for (int i = 1; i < colorCount.Count; i++)
                {
                    if (colorCount[i] > colorCount[maxIndex])
                    {
                        maxIndex = i;
                    }
                }
                int needColorNum = colorCount[maxIndex];
                // 从颜色中找到符合条件的颜色
                List<int> needColors = new List<int>();
                foreach (var colorIt in randomCache.colors)
                {
                    if (colorIt.Value >= needColorNum && !colorVerityKey.Contains(colorIt.Key))
                    {
                        needColors.Add(colorIt.Key);
                    }
                }
                if (needColors.Count == 0)
                {
                    return false;
                }
                // 随机选择一种颜色
                int randomColorIndex = random.Next(0, needColors.Count);
                int randomColor = needColors[randomColorIndex];
                for (int i = 0; i < needColorNum; i++)
                {
                    colorVerityKey.Add(randomColor);
                }
                // 减少数量
                randomCache.colors[randomColor] -= needColorNum;
                colorCount.RemoveAt(maxIndex);
            }
            if (colorVerityKey.Count < Beaker.maxCapacity)
            {
                return false;
            }
            while (colorVerityKey.Count > 0)
            {
                int nRandomIndex = random.Next(0, colorVerityKey.Count);
                beaker.Contents.Push(colorVerityKey[nRandomIndex]);
                colorVerityKey.RemoveAt(nRandomIndex);
            }
            nIndex++;
        }
        return true;
    }
    override public bool Exucte(RandomSystemData data, RandomFlow.RandomCache randomCache, System.Random random)
    {
        RandomFlow.RandomCache cache = randomCache.Clone();
        bool isOk = ExcuteInternal(data, cache, random);
        if (isOk)
        {
            randomCache.Asigned(cache);
        }
        return isOk;
    }

    public override void RefreshUI(RandomSystemData data, GameObject gameObject)
    {
        var randomController = gameObject.GetComponent<RandomController_SingleTubesColored>();
        randomController.SetTubesColorsNum(TubesColorsNum);
        randomController.SetTubesNum(TubesNum);
        randomController.OnValueChanged = () =>
        {
            TubesColorsNum = randomController.TubesColorsNum;
            TubesNum = randomController.TubesNum;
        };
        randomController.OnRemove = () =>
        {
            data.controllers.Remove(this);
            GameObject.Destroy(gameObject);
        };
    }

    public override Tuple<bool, string> VaildData(RandomSystemData data)
    {
        if (TubesColorsNum <= 0)
        {
            return new Tuple<bool, string>(false, "水瓶颜色数量必须大于0");
        }
        if (TubesNum <= 0)
        {
            return new Tuple<bool, string>(false, "水瓶数量必须大于0");
        }
        if (TubesNum > data.coloredTubes)
        {
            return new Tuple<bool, string>(false, "水瓶数量不能超过" + data.coloredTubes);
        }
        if (TubesColorsNum > data.colors.Count)
        {
            return new Tuple<bool, string>(false, "水瓶颜色数量不能超过" + data.colors.Count);
        }
        return new Tuple<bool, string>(true, "");
    }

    public override int OccupiedColoredTubesCount()
    {
        return TubesNum;
    }
}

#endregion

