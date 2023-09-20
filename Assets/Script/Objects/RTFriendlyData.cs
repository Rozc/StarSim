using System.Collections.Generic;
using Script.Data;
using Script.Enums;
using UnityEngine;

namespace Script.Objects
{
    [System.Serializable]
    public class RTFriendlyData : RealtimeData
    {
        public PathType Path;
        private Dictionary<string, float> TraceData;
        private Dictionary<string, float> LightConeData;
        private Dictionary<string, float> RelicsData;

        public RTFriendlyData(FriendlyData data) : base(data)
        {
            Path = data.Path;
            TraceData = new Dictionary<string, float>();
            LightConeData = new Dictionary<string, float>();
            RelicsData = new Dictionary<string, float>();
        }



        public override float Get(string propName)
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
        
        public override float GetFixed(string propName)
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