﻿using System.Collections.Generic;

namespace GameNet.Models
{
    public class MapInfo
    {
        public int Id { get; set; }
        public virtual GroupInfo Group { get; set; }
        public string Nick { get; set; }

        //public virtual ICollection<ReplayInfo> Replays { get; set; } = new List<ReplayInfo>();
    }
    
    public class MapData
    {
        public GroupData Group { get; set; }

        public string Nick { get; set; }

        //public List<ReplayData> Replays { get; set; }
    }
}
