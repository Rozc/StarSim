using UnityEngine;

namespace Script.Data
{
    [CreateAssetMenu(fileName = "newBounceData", menuName = "Data/Action Data/Bounce Data")]
    public class BounceData : ActionDataBase
    {
        [field: SerializeField] public int BounceTimes { get; private set; }
        [field: SerializeField] public int[] MultipleMain { get; private set; }
        [field: SerializeField] public int[] MultipleRandom { get; private set; }
    }
}