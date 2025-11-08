using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Skills/Attack Skill")]
public class AttackSkill : BaseSkill
{
    public override void Execute()
    {
        Debug.Log($"Executing Attack Skill: {skillName}");

        // Prompt player to select an enemy target
        GameManager.Instance.StartCoroutine(WaitForTargetAndAttack());
    }

    private System.Collections.IEnumerator WaitForTargetAndAttack()
    {
        GameManager.Instance.SetPlayerInput(false);

        Debug.Log("Select enemy target...");
        GameManager.Instance.SelectedTarget = null;
        InfoPanel.instance.ShowMessage("Select enemy as target...");

        yield return new WaitUntil(() => GameManager.Instance.SelectedTarget != null);

        InfoPanel.instance.Hide();
        var target = GameManager.Instance.SelectedTarget;
        GameManager.Instance.SelectTarget(null);

        // Example: perform attack animation
        Debug.Log($"Attacking {target.name}!");

        target.TakeDamage(5);

        GameManager.Instance.SetPlayerInput(true);
    }
}
