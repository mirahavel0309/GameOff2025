using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct CameraPathPoint
{
    public Transform target;   // position + rotation
    public float time;        // movement speed
}

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
    [Header("Stages swap")]
    [HideInInspector]public int waveCounter = 0;
    public int maxWavesCount = 2;
    public Transform[] exitPathPoints;
    public CameraPathPoint[] enterCameraPath;
    public CameraPathPoint[] exitCameraPath;
    private int currentStageIndex = 0;
    public GameBoard[] allStages;
    public Transform roomOrigin;
    GameBoard currentRoom;
    public Image blackFade;
    public Camera cam;
    public FreeCameraControl camController;

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
    [SerializeField] private bool waveActive = false;

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

    public List<CardInstance> GetEnemies()
    {
        return enemyField.GetCards();
    }

    public HeroActionMenu heroActionMenuPrefab;

    internal HeroInstance GetHeroOfelement(ElementType element)
    {
        foreach (var hero in PlayerHeroes)
        {
            if (hero.mainElement == element)
                return hero;
        }
        return null;
    }
    private IEnumerator GameStartRoutine()
    {
        camController.enabled = false;
        yield return StartCoroutine(LoadNextRoomEnvironment());
        yield return StartCoroutine(MoveCamera(enterCameraPath));
        camController.enabled = true;
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
        waveCounter++;
        InfoPanel.instance.UpdateWavesCount(waveCounter, maxWavesCount);
        
        yield return enemySpawner.SpawnWaveCoroutine();
        waveActive = true;
        
        

        StartCoroutine(PlayerTurnRoutine());
    }
    private IEnumerator WaveMonitorRoutine()
    {
        while (true)
        {
            yield return new WaitUntil(() => enemyField.GetCards().Count == 0 && waveActive);

            yield return new WaitForSeconds(0.5f);
            waveActive = false;

            // TODO: Intermission phase (player upgrades, shop, etc.)
            // yield return StartCoroutine(IntermissionRoutine());
            // For now, we go straight to the next wave

            yield return new WaitForSeconds(0.5f);
            waveCounter++;
            if (waveCounter < maxWavesCount)
            {
                InfoPanel.instance.UpdateWavesCount(waveCounter, maxWavesCount);
                yield return enemySpawner.SpawnWaveCoroutine();
                waveActive = true;

                // Return control to the player
                StartCoroutine(PlayerTurnRoutine());
            }
            else
            {
                StartCoroutine(EndLevel());
            }  
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

        if (playerField.AllHeroesDefeated())
        {
            GameOverScreen.instance.Show();
        }

        foreach (var hero in playerField.GetCards())
        {
            if (hero == null) continue;
            yield return StartCoroutine(hero.ProcessStartOfTurnEffects());
            yield return new WaitForSeconds(0.2f);
        }

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
        List<CardInstance> playerCards = playerField.GetCards().ToList();
        playerCards.RemoveAll(x => (x as HeroInstance).isDefeated);

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

            AttackMonsterSkill skill = enemyCard.gameObject.GetComponent<AttackMonsterSkill>();

            CardInstance target = playerCards[Random.Range(0, playerCards.Count)];
            yield return skill.StartCoroutine(skill.Execute(target));
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
    public IEnumerator EndLevel()
    {
        Debug.Log("EndLevel triggered.");

        SetPlayerInput(false);
        InfoPanel.instance.ShowMessage("Stage Cleared!");

        yield return new WaitForSeconds(1f);

        bool hasNextStage = (currentStageIndex + 1) < allStages.Length;

        if (hasNextStage)
        {
            yield return StartCoroutine(MoveHeroesThroughPath(exitPathPoints));
            currentStageIndex++;
            camController.enabled = false;
            yield return StartCoroutine(FadeScreen());
            camController.enabled = true;


            //yield return StartCoroutine(MoveHeroesThroughPath(entryPathPoints));


            waveCounter = 0;
            yield return enemySpawner.SpawnWaveCoroutine();
            waveActive = true;
            StartCoroutine(PlayerTurnRoutine());
        }
        else
        {
            // --- Step 1: Play victory celebration movement if needed ---
            yield return StartCoroutine(MoveHeroesThroughPath(exitPathPoints));

            // --- Step 2: End game with Win Screen ---
            Debug.Log("GAME COMPLETED. Display Win Screen.");

            // TODO: Call your WinScreen or transition here
            // Example:
            // UIManager.Instance.ShowWinScreen();

            InfoPanel.instance.Hide();
        }
    }

    private IEnumerator MoveCamera(CameraPathPoint[] path)
    {
        camController.enabled = false;
        foreach (var point in path)
        {
            Transform target = point.target;
            float duration = point.time;

            Vector3 startPos = cam.transform.position;
            Quaternion startRot = cam.transform.rotation;

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float normalized = t / duration;

                cam.transform.position = Vector3.Lerp(startPos, target.position, normalized);
                cam.transform.rotation = Quaternion.Slerp(startRot, target.rotation, normalized);

                yield return null;
            }

            cam.transform.position = target.position;
            cam.transform.rotation = target.rotation;

            
            camController.ResetValues();
            //yield return new WaitForSeconds(0.05f);
        }
        
    }

    public IEnumerator FadeScreen()
    {
        float duration = 1f;
        blackFade.gameObject.SetActive(true);
        yield return StartCoroutine(MoveCamera(exitCameraPath));

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float normalized = t / duration;
            Color c = blackFade.color;
            c.a = Mathf.Lerp(0f, 1f, normalized);
            blackFade.color = c;
            yield return null;
        }
        blackFade.color = new Color(blackFade.color.r, blackFade.color.g, blackFade.color.b, 1f);

        yield return StartCoroutine(LoadNextRoomEnvironment());
        StartCoroutine(MoveCamera(enterCameraPath));


        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float normalized = t / duration;
            Color c = blackFade.color;
            c.a = Mathf.Lerp(1f, 0f, normalized);
            blackFade.color = c;
            yield return null;
        }

        blackFade.gameObject.SetActive(false);
        blackFade.color = new Color(blackFade.color.r, blackFade.color.g, blackFade.color.b, 0f);
    }

    private IEnumerator MoveHeroesThroughPath(Transform[] pathPoints)
    {
        if (pathPoints == null || pathPoints.Length == 0)
            yield break;

        List<HeroInstance> heroes = PlayerHeroes;

        float travelSpeed = 6f;

        // Move heroes through each point in sequence
        foreach (Transform t in pathPoints)
        {
            bool allReached = false;

            while (!allReached)
            {
                allReached = true;

                foreach (var hero in heroes)
                {
                    if (hero == null) continue;

                    hero.transform.position = Vector3.MoveTowards(
                        hero.transform.position,
                        t.position,
                        travelSpeed * Time.deltaTime
                    );

                    if (Vector3.Distance(hero.transform.position, t.position) > 0.05f)
                        allReached = false;
                }

                yield return null;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }




    private IEnumerator LoadNextRoomEnvironment()
    {
        Debug.Log("Loading next room...");

        GameBoard roomPrefab = allStages[currentStageIndex];
        if (currentRoom != null)
            Destroy(currentRoom.gameObject);

        currentRoom = Instantiate(roomPrefab, roomOrigin);

        playerField.fieldPositions = currentRoom.playerLocations.ToList();
        playerField.spawnPoint = currentRoom.playerEnterLocation;
        enemyField.fieldPositions = currentRoom.enemyLocations.ToList();
        enemyField.spawnPoint = currentRoom.enemyEnterLocation;
        exitPathPoints = currentRoom.exitPath;
        enterCameraPath = currentRoom.enterCameraPath;
        exitCameraPath = currentRoom.exitCameraPath;

        playerField.ClearPositions();
        foreach (var hero in PlayerHeroes)
        {
            hero.transform.position = currentRoom.playerEnterLocation.position;
            playerField.ReasignPositions(hero);
        }

        yield return new WaitForSeconds(1f);
    }
    public void RemoveElementFromDeck(ElementType element)
    {
        if(playerDeck.availableElements.Contains(element))
            playerDeck.availableElements.Remove(element);
    }
}
