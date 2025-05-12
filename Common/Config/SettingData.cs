using Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Config
{
    [SettingMapper("Setting Name")]
    public class SettingData : YamlBase
    {
        [SettingMember("MapSizeX", ConvertType.Text)]
        public int MapSizeX { get; set; }

        [SettingMember("MapSizeY", ConvertType.Text)]
        public int MapSizeY { get; set; }

        [SettingMember("GoalX", ConvertType.Text)]
        public int GoalX { get; set; }

        [SettingMember("GoalY", ConvertType.Text)]
        public int GoalY { get; set; }

        //[SettingMember("item2", ConvertType.Combo, ["select1", "select2"])]
        //public string? Mode { get; set; } = "select1";

        //[SettingMember("item3", ConvertType.Radio, ["select1", "select2"])]
        //public string? Third { get; set; }
    }
}
