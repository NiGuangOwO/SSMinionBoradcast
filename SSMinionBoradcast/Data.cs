using SSMinionBoradcast.Classes;
using System.Collections.Generic;

namespace SSMinionBoradcast
{
    internal static class Data
    {
        public static Dictionary<ushort, List<Position>> SSMinion { get; } = new()
    {
                        //迷津
            {956,new List<Position>{new(24,8.8), new(35.5,18), new(8.8,22), new(25.5,33.5) } },
                        //萨维奈岛
            {957,new List<Position>{new(12.0,16.1), new(22.8,10.2), new(16.7,29.3), new(33.5,24.6) } },
                        //加雷马
            {958,new List<Position>{ new(17.4,9.8), new(32.9,9), new(21.8,33), new(33,28.7) } },
                        //叹息海
            {959,new List<Position>{ new(11.9,20.7), new(12.3,36), new(29.6,35.4), new(33,23) } }, 
                        //厄尔庇斯
            {961,new List<Position>{ new(17.2,6.8), new(29,7), new(37,13.6), new(8.3,35.6) } }, 
                        //天外天垓
            {960,new List<Position>{ new(16.3,16.6), new(32.4,10.3), new(10.4,31.8), new(23.4,32.7) } },

                        //雷克兰德
            {813,new List<Position>{ new(10,25), new(13,10), new(33,12), new(30,36) } },
                        //伊尔美格
            {816,new List<Position>{ new(6,30), new(32,11), new(25,22), new(24,37) } },
                        //珂露西亚岛
            {814,new List<Position>{ new(8,29), new(12,15), new(23,15), new(33,32) } },
                        //拉凯提卡大森林
            {817,new List<Position>{ new(15,36), new(8,22), new(19,22), new(30,13) } }, 
                        //安穆·艾兰
            {815,new List<Position>{ new(14,32), new(13.5,12), new(30.5,10), new(30,25) } }, 
                        //黑风海
            {818,new List<Position>{ new(8,7), new(26,9.5), new(38,14), new(33.7,30.1) } },

                        //无人岛test
            {1055,new List<Position>{new(24,8.8), new(35.5,18), new(8.8,22), new(25.5,33.5) } },
        };

        public static Dictionary<string, (uint, uint)> maps { get; set; } = [];

        public static List<string> currMacro { get; set; } = [];
    }
}
