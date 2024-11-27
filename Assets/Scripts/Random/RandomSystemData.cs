
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public abstract class BaseController
{
    public abstract ControllerType GetControllerType();
    public abstract bool Exucte(RandomSystemData data, RandomFlow.RandomCache randomCache, System.Random random);
    public abstract void RefreshUI(RandomSystemData data, GameObject gameObject);
    public abstract Tuple<bool, string> VaildData(RandomSystemData data);
    public abstract int OccupiedColoredTubesCount();
}

public class RandomSystemData
{
    public string folder;
    public int levelBeginIndex;
    public int levelEndIndex;
    public int coloredTubes;
    public int noColoredTubes;
    public List<int> colors = new List<int>();
    public List<BaseController> controllers = new List<BaseController>();
    public List<ColorSampleData> colorSampleDataList = new List<ColorSampleData>(); // seriazable data of color samples, but not used in the code. because it will be loaded from color instance container.

    public void FixData()
    {
        if (colors == null)
        {
            colors = new List<int>();
        }
        if (controllers == null)
        {
            controllers = new List<BaseController>();
        }
        if (colorSampleDataList == null)
        {
            colorSampleDataList = new List<ColorSampleData>();
        }
    }

    override public string ToString()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }


}
