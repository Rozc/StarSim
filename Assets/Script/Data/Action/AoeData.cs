using UnityEngine;

namespace Script.Data.ActionData.AttackData
{
    [CreateAssetMenu(fileName = "newAoeData", menuName = "Data/Action Data/Aoe Data")]
    public class AoeData : ActionDataBase
    {
        [field: SerializeField] public int[] Multiple { get; private set; }
    }
}