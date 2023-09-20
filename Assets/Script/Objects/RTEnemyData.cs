using System.Collections.Generic;
using Script.Data;
using Script.Enums;
using UnityEngine;

namespace Script.Objects
{
    [System.Serializable]
    public class RTEnemyData : RealtimeData
    {

        public RTEnemyData(EnemyData data) : base(data)
        {
            WeaknessList = new List<BattleType>();
            foreach (var type in data.WeaknessList)
            {
                WeaknessList.Add(type);
            }

            WeaknessValue = data.WeaknessValue;
        }

        public override float Get(string propName)
        {
            float result = 0;
            if (BaseData.TryGetValue(propName, out float value)) result += value;
            // if (BaseData.TryGetValue(propName+"%", out value)) result += GetFixed(propName) * value * 0.01f;

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
            if (BaseData.TryGetValue(propName, out float value)) result += value;
            return result;
        }


        
        [field: SerializeField] public List<BattleType> WeaknessList;
        [field: SerializeField] public int MaxWeaknessValue { get; private set; }
        [field: SerializeField] private int _weaknessValueDiff; // -Max < diff < 0
        public int WeaknessValue
        {
            get => MaxWeaknessValue + _weaknessValueDiff;
            set => _weaknessValueDiff = Mathf.Min(0, Mathf.Max(-MaxWeaknessValue, value - MaxWeaknessValue));
        }
    }
}