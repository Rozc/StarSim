using UnityEngine;

namespace Script.Data.ActionData.AttackData
{
    [CreateAssetMenu(fileName = "newBlastData", menuName = "Data/Action Data/Blast Data")]
    public class BlastData : ActionDataBase
    {
        [field: SerializeField] public int[] MultipleCenter { get; private set; }
        [field: SerializeField] public int[] MultipleAdjacent { get; private set; }
    }
}