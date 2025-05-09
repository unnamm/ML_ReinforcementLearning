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

        //[SettingMember("item2", ConvertType.Combo, ["select1", "select2"])]
        //public string? Mode { get; set; } = "select1";

        //[SettingMember("item3", ConvertType.Radio, ["select1", "select2"])]
        //public string? Third { get; set; }
    }
}
