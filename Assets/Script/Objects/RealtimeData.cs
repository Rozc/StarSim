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
        
        [field: SerializeField] private PathType _path;
        public PathType Path 
        {
            get
            {
                if (_path == PathType.None)
                {
                    Debug.LogError("You can't get Path of a Enemy! Or there is an error in the data!");
                    return PathType.None;
                }
                else
                {
                    return _path;
                }
            }
            private set => _path = value;
        }

        [field: SerializeField] public List<BattleType> WeaknessList;
        [field: SerializeField] private int _weaknessValue;
        public int WeaknessValue
        {
            get
            {
                if (Path != PathType.None)
                {
                    Debug.LogError("You can't get WeaknessValue of a Friendly! Or there is an error in the data!");
                    return 0;
                }
                else
                {
                    return _weaknessValue;
                }
            }
            private set => _weaknessValue = value;
        }
        
        private Dictionary<string, float> BaseData;
        private Dictionary<string, float> TraceData;
        private Dictionary<string, float> LightConeData;
        private Dictionary<string, float> RelicsData;
        
        [field: SerializeField] public List<Buff> BuffList;
        private float _healthDiff;

        public float CurrentHealth
        {
            get => Get("Health") + _healthDiff;
            set => _healthDiff = value - Get("Health");
        } 

        public RealtimeData(ObjectData data)
        {
            CharacterID = data.CharacterID;
            Name = data.Name;
            Level = data.Level;
            BattleType = data.BattleType;
            WeaknessList = new List<BattleType>();
            switch (data)
            {
                case FriendlyData fd:
                    Path = fd.Path;
                    break;
                case EnemyData ed:
                {
                    Path = PathType.None;
                    foreach (var bt in ed.WeaknessList)
                    {
                        WeaknessList.Add(bt);
                    }
                    _weaknessValue = ed.WeaknessValue;
                    break;
                }
            }
            
            BaseData = PropertyFile.Read(data.PropertyStrings, data is FriendlyData ? 0 : 1);
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
            // if (BaseData.TryGetValue(propName+"%", out value)) result += GetFixed(propName) * value * 0.01f;
            
            if (TraceData.TryGetValue(propName, out value)) result += value;
            // if (TraceData.TryGetValue(propName+"%", out value)) result += GetFixed(propName) * value * 0.01f;

            if (LightConeData.TryGetValue(propName, out value)) result += value;
            // if (LightConeData.TryGetValue(propName+"%", out value)) result += GetFixed(propName) * value * 0.01f;

            if (RelicsData.TryGetValue(propName, out value)) result += value;
            // if (RelicsData.TryGetValue(propName+"%", out value)) result += GetFixed(propName) * value * 0.01f;


            foreach (var buff in BuffList)
            {
                if (buff.PropertyDict.TryGetValue(propName, out value)) 
                    result += value * buff.CurrentStack;
            }
            return result;
        }
        
        
        // TODO 考虑光锥上的数据怎么做, 体现为 不可见的 Buff ？以及如何处理光锥的各种效果
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