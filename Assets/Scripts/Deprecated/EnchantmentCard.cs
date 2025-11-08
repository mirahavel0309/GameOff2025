using UnityEngine;

public enum EnchantmentEffectType
{
    AttackBoost,
    Shield,
    Heal,
    Burn,
    Custom
}

[CreateAssetMenu(fileName = "New Enchantment", menuName = "Cards/Enchantment")]
public class EnchantmentCard : Card
{
    [Header("Enchantment Settings")]
    public EnchantmentEffectType effectType;
    public int magnitude; // e.g. +10 attack, +5 shield, etc.
    public int duration; // in turns

    [TextArea]
    public string effectDescription;
}
