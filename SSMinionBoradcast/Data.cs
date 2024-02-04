using System.Collections.Generic;

namespace SSMinionBoradcast
{
    internal static class Data
    {
        public static Dictionary<ushort, List<(double, double)>> SSMinion { get; } = new()
    {
                        //迷津
            {956,new List<(double, double)>{(24,8.8), (35.5,18), (8.8,22), (25.5,33.5) } },
                        //萨维奈岛
            {957,new List<(double, double)>{(10,14.6), (22.2,10), (16,30), (32.6,26) } },
                        //加雷马
            {958,new List<(double, double)>{(17.4,9.8), (32.9,9), (21.8,33), (33,28.7) } },
                        //叹息海
            {959,new List<(double, double)>{(11.9,20.7), (12.3,36), (29.6,35.4), (33,23) } }, 
                        //厄尔庇斯
            {961,new List<(double, double)>{(17.2,6.8), (29,7), (37,13.6), (8.3,35.6) } }, 
                        //天外天垓
            {960,new List<(double, double)>{(16.3,16.6), (32.4,10.3), (10.4,31.8), (23.4,32.7) } },

                        //雷克兰德
            {813,new List<(double, double)>{(10,25), (13,10), (33,12), (30,36) } },
                        //伊尔美格
            {816,new List<(double, double)>{(6,30), (32,11), (25,22), (24,37) } },
                        //珂露西亚岛
            {814,new List<(double, double)>{(8,29), (12,15), (23,15), (33,32) } },
                        //拉凯提卡大森林
            {817,new List<(double, double)>{(15,36), (8,22), (19,22), (30,13) } }, 
                        //安穆·艾兰
            {815,new List<(double, double)>{(14,32), (13.5,12), (30.5,10), (30,25) } }, 
                        //黑风海
            {818,new List<(double, double)>{(8,7), (26,9.5), (38,14), (33.7,30.1) } },
    };

        public static bool isBoradcasting { get; set; } = false;

        public static List<(double, double)> currSSMinionList { get; set; } = [];

        public static Dictionary<string, (uint, uint)> maps { get; set; } = [];

        public static List<string> currMacro { get; set; } = [];
    }
}
