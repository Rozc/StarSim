using UnityEngine;

namespace Script.Data
{
    [CreateAssetMenu(fileName = "newPerTypeProperties", menuName = "Data/Object Data/Per Type Properties")]
    public class PerTypeProperties : ScriptableObject
    {
        [field: SerializeField] public TypePropType TypePropType { get; private set; }
        [field: SerializeField] public float All { get; private set; }
        [field: SerializeField] public float Phsy { get; private set; }
        [field: SerializeField] public float Fire { get; private set; }
        [field: SerializeField] public float Ice { get; private set; }
        [field: SerializeField] public float Litn { get; private set; }
        [field: SerializeField] public float Wind { get; private set; }
        [field: SerializeField] public float Qutm { get; private set; }
        [field: SerializeField] public float Imag { get; private set; }
    }
}