
using System.Collections.Generic;


namespace HsWaterSort.Level
{
    public class TubeData
    {
        public List<int> Colors; // Colors of the tube //1一个数组代表一个水瓶的4个位置，顺序为从高到低[水瓶顶，×，×，水瓶底]
    }

    public class StepData
    {
        List<KeyValuePair<int, int>> Operations; // 
    }

    public class LevelData
    {
        public int LevelId = -1;
        public List<TubeData> Tubes; // List of tubes in the level 配置内容 "tubes":[[1,2,1,2],[2,1,2,1],[0,0,0,0]] 
        public List<StepData> Steps; // List of steps in the level 跑解"steps":[[0,3],[0,4],[1,3],[1,4],[1,0],[1,3],[2,4],[2,0],[2,3],[2,4]]// 填写数字为水瓶id，最左最上为0，从左向右递增，从上向下递增
    }
}
