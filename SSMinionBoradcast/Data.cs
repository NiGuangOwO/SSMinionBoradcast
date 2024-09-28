using System.Collections.Generic;
using System.Numerics;

namespace SSMinionBoradcast
{
    internal static class Data
    {
        public static Dictionary<ushort, Vector2[]> SSMinion { get; } = new()
    {
            {1187,[new(18.3f,17.8f),new(25.7f,13.9f),new(15.5f,28.7f),new(34.7f,28.4f)] }, //奥阔帕恰山
            {1188,[new(16.7f,7.3f),new(33.2f,8.2f),new(15.7f,32.7f),new(29.7f,24.5f)] }, //克扎玛乌卡湿地
            {1189,[new(17,13.7f),new(35.6f,22.4f),new(28.1f,24.7f),new(12.7f,35.6f)] }, //亚克特尔树海
            {1190,[new(11.6f,8.5f),new(23.3f,13.3f),new(14.8f,31),new(34.2f,31.7f)] }, //夏劳尼荒野
            {1191,[new(14.2f,17.6f),new(14.9f,34.6f),new(30.2f,9.6f),new(32.2f,22.7f)] }, //遗产之地
            {1192,[new(11.1f,18.4f),new(27.3f,7),new(19.7f,30.8f),new(28.5f,36.5f)] }, //活着的记忆

            {956,[new(24,8.8f), new(35.5f,18), new(8.8f,22), new(25.5f,33.5f)] }, //迷津               
            {957,[new(12.0f,16.1f), new(22.8f,10.2f), new(16.7f,29.3f), new(33.5f,24.6f)]}, //萨维奈岛
            {958,[new(17.4f,9.8f), new(32.9f,9), new(21.8f,33), new(33,28.7f)] }, //加雷马
            {959,[ new(11.9f,20.7f), new(12.3f,36), new(29.6f,35.4f), new(33,23)]}, //叹息海
            {960,[new(16.3f,16.6f), new(32.4f,10.3f), new(10.4f,31.8f), new(23.4f,32.7f)] }, //天外天垓
            {961,[new(17.2f,6.8f), new(29,7), new(37,13.6f), new(8.3f,35.6f)] }, //厄尔庇斯

            {813, [new(10,25), new(13,10), new(33,12), new(30,36)] }, //雷克兰德
            {814, [new(8,29), new(12,15), new(23,15), new(33,32)]}, //珂露西亚岛
            {815,[new(14,32), new(13.5f,12), new(30.5f,10), new(30,25)] }, //安穆·艾兰
            {816,[ new(6,30), new(32,11), new(25,22), new(24,37)]  }, //伊尔美格
            {817,[new(15,36), new(8,22), new(19,22), new(30,13)] }, //拉凯提卡大森林
            {818,[new(8,7), new(26,9.5f), new(38,14), new(33.7f,30.1f)] }, //黑风海

            {1055,[new(24,8.8f), new(35.5f,18), new(8.8f,22), new(25.5f,33.5f)] }, //无人岛test
        };
        public static Dictionary<string, (uint, uint)> maps { get; set; } = [];
        public static List<string> currMacro { get; set; } = [];
    }
}
