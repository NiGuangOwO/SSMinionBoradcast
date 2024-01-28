using System.Collections.Generic;

namespace SSMinionBoradcast
{
    internal static class Data
    {
        public static Dictionary<ushort, List<(double, double)>> SSMinion { get; } = new()
    {
        {343,new List<(double, double)>{(16.3,16.6), (32.4,10.3),(10.4,31.8),(23.4,32.7)} }, //天外天垓
    };

        public static bool isBoradcasting { get; set; } = false;

        public static List<(double, double)> currSSMinionList { get; set; } = [];

        public static Dictionary<string, (uint, uint)> maps { get; set; } = [];

        public static List<string> currMacro { get; set; } = [];
    }
}
