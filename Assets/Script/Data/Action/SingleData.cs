using UnityEngine;

namespace Script.Data.ActionData.AttackData
{
    [CreateAssetMenu(fileName = "newSingleData", menuName = "Data/Action Data/Single Data")]
    public class SingleData : ActionDataBase
    {
        [field: SerializeField] public int[] Multiple { get; private set; }
    }
}