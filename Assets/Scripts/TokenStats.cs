using UnityEngine;

// A struct to define the stats of this Token. Set in the Inspector
public class TokenStats : MonoBehaviour {
    public string characterName;
    public int HP;
    public int AC;
    public int InitativeMod;
    public int Speed;

    public string AttackName;
    public int AttackRange;
    public int AttackMod;
    public int DamageDiceNum;
    public int DamageDiceMagnitude;
    public int DamageMod;
}
