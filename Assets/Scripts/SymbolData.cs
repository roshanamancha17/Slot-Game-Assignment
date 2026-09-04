using UnityEngine;

[CreateAssetMenu(fileName = "NewSymbol", menuName = "SlotGame/Symbol Data")]
public class SymbolData : ScriptableObject
{
    public int symbolID;
    public Sprite symbolSprite;
    public float payoutMultiplier;
}