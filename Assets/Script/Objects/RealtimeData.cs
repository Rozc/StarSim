using System.Collections.Generic;
using System.Diagnostics;
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
        private Dictionary<string, float> BuffedData;

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
            BuffedData = new Dictionary<string, float>();
        }

        public float Get(string propName)
        {
            float result = 0;
            if (BaseData.TryGetValue(propName, out float value))
            {
                result += value;
            }
            if (TraceData.TryGetValue(propName, out value))
            {
                result += value;
            }
            if (LightConeData.TryGetValue(propName, out value))
            {
                result += value;
            }
            if (RelicsData.TryGetValue(propName, out value))
            {
                result += value;
            }
            if (BuffedData.TryGetValue(propName, out value))
            {
                result += value;
            }
            return result;
        }

        public float GetFixed(string propName)
        {
            float result = 0;
            if (BaseData.TryGetValue(propName, out float value))
            {
                result += value;
            }
            if (TraceData.TryGetValue(propName, out value))
            {
                result += value;
            }
            if (LightConeData.TryGetValue(propName, out value))
            {
                result += value;
            }
            if (RelicsData.TryGetValue(propName, out value))
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

        /// <summary>
        /// Add to BuffedData
        /// </summary>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        /// <param name="percentage"> false => fixed, true => percentage</param>
        /// <exception cref="NotImplementedException"></exception>
        public void Add(string propName, float value, bool percentage)
        {
            if (BuffedData.ContainsKey(propName))
            {
                BuffedData[propName] += value * (percentage ? (0.01f * GetFixed(propName)) : 1);
            }
            else
            {
                BuffedData[propName] = value * (percentage ? (0.01f * GetFixed(propName)) : 1);
            }
        }

        public void Minus(string propName, float value, bool percentage)
        {
            if (BuffedData.ContainsKey(propName))
            {
                BuffedData[propName] -= value * (percentage ? (0.01f * GetFixed(propName)) : 1);
            }
            else
            {
                Debug.LogError("Remove failed: No such property has been added: " + propName);
            }
        }
    }
}