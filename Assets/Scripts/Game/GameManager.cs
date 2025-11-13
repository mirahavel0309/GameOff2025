using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool playerTurn = true;
    public int actionsThisTurn = 0;
    public int maxActionsPerTurn = 3;

    [Header("References")]
    [SerializeField] private HeroInstance heroPrefab;
    [SerializeField] private ElementalDeck playerDeck;
    public PlayerHand playerHand;
    public TroopsField playerField;
    [SerializeField] private TroopsField enemyField;
    [SerializeField] private WaveSpawner enemySpawner;
    [SerializeField] private PlayerInputController playerInput;
    [SerializeField] private List<HeroCard> startingHeroes;
    public bool IsActionPending(string type) => pendingActionType == type;

    public HeroActionMenu heroActionMenuPrefab;
    private string pendingActionType = null;

    [Header("Turn Settings")]
    [SerializeField] private float enemyTurnDuration = 2f;

    [Header("Global Skills")]
    public List<BaseSkill> allSkills = new List<BaseSkill>();

    private List<ElementType> selectedElements = new List<ElementType>();

    [Header("Skill UI")]
    public Transform skillCardParent;        // UI parent container for displaying skill cards
    public GameObject skillCardPrefab;       // Prefab with SkillCardUI script
    private List<GameObject> activeSkillCards = new List<GameObject>();

    public bool PlayerInputEnabled => playerInput.InputEnabled;

    private int currentTurn = 0;
    private HeroInstance selectedHero;
    public List<HeroInstance> PlayerHeroes { get; private set; } = new();
    public CardInstance SelectedTarget;
    private List<ElementalCardInstance> selectedCards = new List<ElementalCardInstance>();
    private BaseSkill matchedSkill = null;
    private bool waveActive = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        allSkills = GetComponentsInChildren<BaseSkill>().ToList();
        StartCoroutine(GameStartRoutine());
        StartCoroutine(WaveMonitorRoutine());
    }
    private IEnumerator GameStartRoutine()
    {
        // Spawn heroes
        for (int i = 0; i < startingHeroes.Count; i++)
        {
            HeroInstance hero = Instantiate(startingHeroes[i].characterPrefab, playerField.transform).GetComponent<HeroInstance>();
            hero.SetCardData(startingHeroes[i]);
            hero.troopsField = playerField;
            PlayerHeroes.Add(hero);
            playerField.AddCard(hero);
            yield return new WaitForSeconds(0.1f);
        }

        // Spawn enemy wave
        yield return enemySpawner.SpawnWaveCoroutine();
        waveActive = true;

        StartCoroutine(PlayerTurnRoutine());
    }
    private IEnumerator WaveMonitorRoutine()
    {
        while (true)
        {
            yield return new WaitUntil(() => enemyField.GetCards().Count == 0 && waveActive);

            waveActive = false;
            Debug.Log("Wave cleared!");

            // TODO: Intermission phase (player upgrades, shop, etc.)
            // yield return StartCoroutine(IntermissionRoutine());
            // For now, we go straight to the next wave

            yield return new WaitForSeconds(1f);

            Debug.Log("Spawning next wave...");
            yield return enemySpawner.SpawnWaveCoroutine();
            waveActive = true;

            // Return control to the player
            StartCoroutine(PlayerTurnRoutine());    
        }
    }
    public void SelectTarget(CardInstance target)
    {
        SelectedTarget = target;
    }

    public void SelectHero(HeroInstance hero)
    {
        selectedHero = hero;
    }
    public HeroInstance GetSelectedHero()
    {
        return selectedHero;
    }

    private IEnumerator PlayerTurnRoutine()
    {
        currentTurn++;

        actionsThisTurn = 0;
        ResetAllAttacks();
        playerDeck.DrawUntilHandIsFull();

        SetPlayerInput(true);
        yield return new WaitUntil(() => playerInput.EndTurnPressed);
        playerInput.EndTurnPressed = false;

        SetPlayerInput(false);
        StartCoroutine(EnemyTurnRoutine());
    }
    private IEnumerator EnemyTurnRoutine()
    {
        List<CardInstance> enemyCards = enemyField.GetCards();
        List<CardInstance> playerCards = playerField.GetCards();

        foreach (var enemyCard in new List<CardInstance>(enemyCards))
        {
            if (enemyCard == null) continue;

            yield return StartCoroutine(enemyCard.ProcessStartOfTurnEffects());

            yield return new WaitForSeconds(0.2f);
        }

        enemyCards = enemyField.GetCards().ToList(); // Rebuild the list after potential removals
        enemyCards.RemoveAll(e => e.GetComponent<FreezeEffect>() != null); // make frozen units skip action

        foreach (var enemyCard in enemyCards)
        {
            if (enemyCard == null) continue;
            if (playerCards.Count == 0) break;

            CardInstance target = playerCards[Random.Range(0, playerCards.Count)];
            yield return enemyCard.StartCoroutine(enemyCard.PerformAttack(target));
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(enemyTurnDuration);
        StartCoroutine(PlayerTurnRoutine());
    }
    public void RegisterActionUse()
    {
        actionsThisTurn++;

        if (actionsThisTurn >= maxActionsPerTurn)
        {
            StartCoroutine(EndTurnAfterDelay());
        }
    }
    private IEnumerator EndTurnAfterDelay()
    {
        // Wait a bit to let final skill visuals complete
        yield return new WaitForSeconds(1f);
        playerInput.EndTurnPressed = true;
        Debug.Log("simulate pressing end turn!");
    }

    private void ResetAllAttacks()
    {
        foreach (var c in playerField.GetCards())
        {
            if (c != null) c.HasActedThisTurn = false;
        }
    }

    public void SetPlayerInput(bool enabled)
    {
        playerInput.SetInputEnabled(enabled);
    }
    public void AddElementToCombo(ElementType element, ElementalCardInstance card)
    {
        selectedElements.Add(element);
        selectedCards.Add(card);
        EvaluateCombo();
    }

    // Called by ElementalCardInstance when toggled OFF
    public void RemoveElementFromCombo(ElementType element, ElementalCardInstance card)
    {
        selectedElements.Remove(element);
        selectedCards.Remove(card);
        EvaluateCombo();
    }

    private void EvaluateCombo()
    {
        matchedSkill = null;

        // Clear any previously shown skill cards
        ClearSkillCards();

        List<BaseSkill> matchingSkills = FindMatchingSkills(selectedElements);

        if (matchingSkills.Count > 0)
        {
            foreach (var skill in matchingSkills)
            {
                GameObject cardObj = Instantiate(skillCardPrefab, skillCardParent);
                var cardUI = cardObj.GetComponent<SkillCardUI>();
                cardUI.Initialize(skill);
                activeSkillCards.Add(cardObj);
            }

            InfoPanel.instance.ShowMessage("Choose a skill!");
        }
        else
        {
            InfoPanel.instance.ShowMessage("No skill matches this combination.");
        }
    }
    private List<BaseSkill> FindMatchingSkills(List<ElementType> combo)
    {
        List<BaseSkill> result = new List<BaseSkill>();

        foreach (var skill in allSkills)
        {
            if (skill.requiredElements.Count != combo.Count)
                continue;

            List<ElementType> skillElements = new List<ElementType>(skill.requiredElements);
            List<ElementType> comboElements = new List<ElementType>(combo);

            bool match = true;

            foreach (var elem in comboElements)
            {
                if (skillElements.Contains(elem))
                {
                    skillElements.Remove(elem);
                }
                else
                {
                    match = false;
                    break;
                }
            }

            if (match && skillElements.Count == 0)
                result.Add(skill);
        }

        return result;
    }

    public void OnSkillCardChosen(BaseSkill chosenSkill)
    {
        InfoPanel.instance.Hide();

        // Consume used cards
        foreach (var card in selectedCards.ToList())
            card.Consume();

        selectedElements.Clear();
        selectedCards.Clear();

        ClearSkillCards();

        chosenSkill.Execute();
    }

    private void ClearSkillCards()
    {
        foreach (var card in activeSkillCards)
        {
            if (card != null)
                Destroy(card);
        }
        activeSkillCards.Clear();
    }
}
