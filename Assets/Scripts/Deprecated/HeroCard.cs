using UnityEngine;

[CreateAssetMenu(fileName = "New Hero", menuName = "Cards/Hero")]
public class HeroCard : Card
{
    [Header("Hero Settings")]
    public int baseAttack;
    public int baseHealth;
    public int baseMana;

    [TextArea]
    public string attackDescription;
    public string castDescription;
    public string defendDescription;
}
