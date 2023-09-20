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
    public abstract class RealtimeData
    {
        [field: SerializeField] public int CharacterID { get; private set; }
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public int Level { get; private set; }
        [field: SerializeField] public BattleType BattleType { get; private set; }
        
        
        protected Dictionary<string, float> BaseData;

        
        [field: SerializeField] public List<Buff> BuffList;
        [field: SerializeField] private float _healthDiff; // Always negative

        public float CurrentHealth
        {
            get => Get("Health") + _healthDiff;
            set => _healthDiff = Mathf.Min(0, value - Get("Health"));
        }

        protected RealtimeData(ObjectData data)
        {
            CharacterID = data.CharacterID;
            Name = data.Name;
            Level = data.Level;
            BattleType = data.BattleType;
            BaseData = PropertyFile.Read(data.PropertyStrings, data is FriendlyData ? 0 : 1);
            BuffList = new List<Buff>();
        }

        public abstract float Get(string propName);
        public abstract float GetFixed(string propName);
        
        // TODO 考虑光锥上的数据怎么做, 体现为 不可见的 Buff ？以及如何处理光锥的各种效果
        

        
    }
}