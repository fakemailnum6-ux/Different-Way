using System;
using System.Collections.Generic;

namespace DifferentWay.Core
{
    public class NPC
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Dictionary<string, string> Stats { get; set; } = new Dictionary<string, string>();
    }

    public class Location
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> NpcIds { get; set; } = new List<string>();
    }

    public class Quest
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public string TargetNpcId { get; set; }
    }
}
