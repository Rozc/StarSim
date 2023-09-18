using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Script.BuffLogic;
using Script.Data;
using Script.Enums;
using Script.Tools;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Script.Objects
{
    [System.Serializable]
    public class RealtimeData
    {
        [field: SerializeField] public int CharacterID { get; private set; }
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public int Level { get; private set; }
        [field: SerializeField] public BattleType BattleType { get; private set; }
        [field: SerializeField] public PathType Path { get; private set; }
        private Dictionary<string, float> BaseData;
        private Dictionary<string, float> TraceData;
        private Dictionary<string, float> LightConeData;
        private Dictionary<string, float> RelicsData;
        [field: SerializeField] public List<Buff> BuffList;

        public RealtimeData(ObjectData data)
        {
            CharacterID = data.CharacterID;
            Name = data.Name;
            Level = data.Level;
            BattleType = data.BattleType;
            Path = data.Path;
            BaseData = PropertyFile.Read(data.PropertyStrings, Path == PathType.None ? 1 : 0);
            TraceData = new Dictionary<string, float>();
            LightConeData = new Dictionary<string, float>();
            RelicsData = new Dictionary<string, float>();

            BuffList = new List<Buff>();

        }

        public float Get(string propName)
        {
            float result = 0;
            if (propName == "HitFactor")
            {
                result += HitFactorByPath(Path);
            }
            
            if (BaseData.TryGetValue(propName, out float value)) result += value;
            if (BaseData.TryGetValue(propName+"%", out value)) result += GetFixed(propName) * value * 0.01f;
            
            if (TraceData.TryGetValue(propName, out value)) result += value;
            if (TraceData.TryGetValue(propName+"%", out value)) result += GetFixed(propName) * value * 0.01f;

            if (LightConeData.TryGetValue(propName, out value)) result += value;
            if (LightConeData.TryGetValue(propName+"%", out value)) result += GetFixed(propName) * value * 0.01f;

            if (RelicsData.TryGetValue(propName, out value)) result += value;
            if (RelicsData.TryGetValue(propName+"%", out value)) result += GetFixed(propName) * value * 0.01f;


            foreach (var buff in BuffList)
            {
                if (buff.PropertyDict.TryGetValue(propName, out value)) result += value;
            }
            return result;
        }
        
        
        // TODO 考虑光锥上的数据怎么做, 体现为 不可见的 Buff ？
        public float GetFixed(string propName)
        {
            float result = 0;
            if (BaseData.TryGetValue(propName, out float value))
            {
                result += value;
            }
            return result;
        }

        private static float HitFactorByPath(PathType path) => path switch
        {
            PathType.TheHunt or PathType.Erudition => 75,
            PathType.Abundance or PathType.Harmony or PathType.Nihility => 100,
            PathType.Destruction => 125,
            PathType.Preservation => 150,
            _ => 0,
        };
        
    }
}