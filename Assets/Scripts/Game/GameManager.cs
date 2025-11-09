using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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

    [Header("UI")]
    public Button useButton;

    public bool PlayerInputEnabled => playerInput.InputEnabled;

    private enum GamePhase { None, GameStart, PlayerTurn, EnemyTurn }
    private int currentTurn = 0;
    private HeroInstance selectedHero;
    public List<HeroInstance> PlayerHeroes { get; private set; } = new();
    public CardInstance SelectedTarget;
    private List<ElementalCardInstance> selectedCards = new List<ElementalCardInstance>();
    private BaseSkill matchedSkill = null;

    void Awake()
    {
        Instance = this;
        if (useButton != null)
        {
            useButton.onClick.AddListener(OnUseButtonPressed);
            useButton.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        StartCoroutine(GameStartRoutine());
        //StartCoroutine(GameLoop());
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
            playerField.AddCardRepresentation(hero.gameObject);
            yield return new WaitForSeconds(0.1f);
        }

        // Spawn enemy wave
        yield return enemySpawner.SpawnWaveCoroutine();

        StartCoroutine(PlayerTurnRoutine());
    }

    // Called when player plays element cards
    public void AddElementToCombo(ElementType element)
    {
        selectedElements.Add(element);
        Debug.Log($"Element {element} added. Current combo: {string.Join(", ", selectedElements)}");
    }

    // Called when player clicks on their hero to choose caster
    public void SelectCaster(HeroInstance hero)
    {
        selectedHero = hero;
        //Debug.Log($"Selected caster: {hero.cardData.cardName}");
    }

    // Called when player clicks on target (enemy or ally)
    public void SelectTarget(CardInstance target)
    {
        SelectedTarget = target;
        //Debug.Log($"Selected target: {target.cardData.cardName}");
    }

    //// Called when player confirms the spell
    //public void ConfirmSkill()
    //{
    //    if (selectedHero == null || selectedTarget == null)
    //    {
    //        Debug.LogWarning("Cannot cast skill — missing caster or target!");
    //        return;
    //    }

    //    BaseSkill skill = FindMatchingSkill(selectedElements);
    //    if (skill == null)
    //    {
    //        Debug.Log("No spell matches this element combination!");
    //        selectedElements.Clear();
    //        return;
    //    }

    //    Debug.Log($"Casting {skill.skillName} using {string.Join(", ", selectedElements)} elements!");
    //    StartCoroutine(skill.Execute(selectedHero, selectedTarget));
    //    selectedElements.Clear();
    //}

    //private BaseSkill FindMatchingSkill(List<ElementType> elements)
    //{
    //    foreach (var s in allSkills)
    //    {
    //        if (s.Matches(elements))
    //            return s;
    //    }
    //    return null;
    //}

    public void SelectHero(HeroInstance hero)
    {
        selectedHero = hero;
    }
    public HeroInstance GetSelectedHero()
    {
        return selectedHero;
    }

    private IEnumerator GameLoop()
    {
        playerInput.SetInputEnabled(false);

        // Start enemy wave
        yield return StartCoroutine(enemySpawner.SpawnWaveCoroutine());

        // Player draws 5 cards
        for (int i = 0; i < 5; i++)
        {
            playerDeck.DrawCard();
            yield return new WaitForSeconds(0.2f);
        }

        StartCoroutine(PlayerTurnRoutine());
    }

    private IEnumerator PlayerTurnRoutine()
    {
        currentTurn++;
        Debug.Log($"--- Player Turn {currentTurn} ---");

        ResetAllAttacks();
        if (currentTurn > 1)
        {
            playerDeck.DrawCard();
        }

        SetPlayerInput(true);
        yield return new WaitUntil(() => playerInput.EndTurnPressed);
        playerInput.EndTurnPressed = false;

        SetPlayerInput(false);
        StartCoroutine(EnemyTurnRoutine());
    }

    private IEnumerator EnemyTurnRoutine()
    {
        Debug.Log("--- Enemy Turn ---");

        List<CardInstance> enemyCards = enemyField.GetCards();
        List<CardInstance> playerCards = playerField.GetCards();

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
    public void OnHeroActionChosen(HeroInstance hero, string actionType)
    {
        selectedHero = hero;
        pendingActionType = actionType;
        //Debug.Log($"{hero.cardData.cardName} selected action: {actionType}");

        switch (actionType)
        {
            case "Attack":
                BeginAttackAction();
                break;

            case "Cast":
                BeginCastAction();
                break;

            case "Defend":
                BeginDefendAction();
                break;
        }
    }
    private void BeginAttackAction()
    {
        Debug.Log("Choose an enemy target to attack...");
        selectedHero.SelectAsAttacker();
        // Next left-click on an enemy card triggers attack
    }
    private void BeginCastAction()
    {
        Debug.Log("Choose a target for the spell (can be enemy or ally)...");
        selectedHero.SelectAsAttacker();
        // The click handling in CardInstance can detect this mode and pass target
    }
    private void BeginDefendAction()
    {
        Debug.Log("Choose a target for the spell (can be enemy or ally)...");
        selectedHero.SelectAsAttacker();
        // The click handling in CardInstance can detect this mode and pass target
    }
    public void ClearPendingAction()
    {
        pendingActionType = null;
    }

    public bool AreCardsAllies(CardInstance a, CardInstance b)
    {
        // Simple check: compare field ownership
        List<CardInstance> cards = playerField.GetCards();
        return (cards.Contains(a) && cards.Contains(b));
    }
    // Called by ElementalCardInstance when toggled ON
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
        matchedSkill = FindMatchingSkill(selectedElements);

        if (useButton != null)
            useButton.gameObject.SetActive(matchedSkill != null);

        if (matchedSkill != null)
            InfoPanel.instance.ShowMessage($"Found skill: {matchedSkill.skillName}");
        else
            InfoPanel.instance.ShowMessage($"No skill matches current combo!");
    }

    private BaseSkill FindMatchingSkill(List<ElementType> combo)
    {
        foreach (var skill in allSkills)
        {
            if(skill.Matches(combo))
                return skill;

            //if (skill.RequiredElements.Count == combo.Count &&
            //    !skill.RequiredElements.Except(combo).Any())
            //{
            //    return skill;
            //}
        }
        return null;
    }

    private void OnUseButtonPressed()
    {
        if (matchedSkill == null)
            return;

        Debug.Log($"Using skill: {matchedSkill.skillName}");

        // Consume the cards
        foreach (var card in selectedCards.ToList())
            card.Consume();

        selectedElements.Clear();
        selectedCards.Clear();

        if (useButton != null)
            useButton.gameObject.SetActive(false);

        // Execute the skill (will prompt for target if needed)
        matchedSkill.Execute();
    }
}
