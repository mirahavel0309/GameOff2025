using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    public int actionsThisTurn = 3;
    public int maxActionsPerTurn = 3;
    public Material skybox;

    [Header("References")]
    [SerializeField] private HeroInstance heroPrefab;
    [SerializeField] private ElementalDeck playerDeck;
    public TroopsField playerField;
    [SerializeField] public TroopsField enemyField;
    [SerializeField] private WaveSpawner enemySpawner;
    [SerializeField] private PlayerInputController playerInput;
    [SerializeField] private List<HeroCard> startingHeroes;
    public AudioSource bgm;
    public AudioClip winSound;
    public AudioClip lossSound;
    public bool IsActionPending(string type) => pendingActionType == type;
    [Header("Stages swap")]
    [HideInInspector] public int waveCounter = 0;
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
    public AudioClip useSound;

    [Header("Monster Scaling")]
    public float hpScaleIncPerWave = 0.025f;
    public float dmgScaleIncPerWave = 0.015f;
    public float hpScaleIncPerStage = 0.25f;
    public float dmgScaleIncPerStage = 0.15f;

    private string pendingActionType = null;

    [Header("Turn Settings")]
    [SerializeField] private float enemyTurnDuration = 2f;
    public int[] healLevels;

    [Header("Global Skills")]
    public List<BaseSkill> allSkills = new List<BaseSkill>();

    private List<ElementType> selectedElements = new List<ElementType>();

    [Header("Skill UI")]
    public Transform skillCardParent;        // UI parent container for displaying skill cards
    public GameObject skillCardPrefab;       // Prefab with SkillCardUI script
    private List<GameObject> activeSkillCards = new List<GameObject>();
    public TextMeshProUGUI actionsText;
    public TextMeshProUGUI healText;

    public bool PlayerInputEnabled => playerInput.InputEnabled;

    private int currentTurn = 0;
    private HeroInstance selectedHero;
    public List<HeroInstance> PlayerHeroes { get; private set; } = new();
    public CardInstance SelectedTarget;
    private List<ElementalCardInstance> selectedCards = new List<ElementalCardInstance>();
    private BaseSkill matchedSkill = null;
    [SerializeField] private bool waveActive = false;
    public HeroSelectionData heroSelectionData;

    private Queue<CardInstance> attackQueue = new Queue<CardInstance>();
    private PriorityQueue<CardInstance> attackPQ = new PriorityQueue<CardInstance>();
    public Image[] attackQueueImages;
    public Image currentAttackerImage;

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

    private void ResetSpeeds()
    {
        var enemies = enemyField.GetCards();
        var heroes = playerField.GetCards();

        foreach (var card in heroes)
        {
            card.speedCount = 499;
        }
        foreach (var card in enemies)
        {
            card.speedCount = 500;
        }
    }

    private void ProcessNextAttacker()
    {
        var enemies = enemyField.GetCards();
        var heroes = playerField.GetCards();
        int lowestCount = 100000;
        int highestSpeed = 0;
        CardInstance nextCard = null;

        foreach (var card in heroes)
        {
            if (card.speedCount < lowestCount)
            {
                nextCard = card;
                lowestCount = card.speedCount;
                highestSpeed = card.speed;
            }
            else if (card.speedCount == lowestCount && card.speed > highestSpeed)
            {
                nextCard = card;
                lowestCount = card.speedCount;
                highestSpeed = card.speed;
            }
            card.speedCount -= card.speed;
        }
        foreach (var card in enemies)
        {
            if (card.speedCount < lowestCount)
            {
                nextCard = card;
                lowestCount = card.speedCount;
                highestSpeed = card.speed;
            }
            else if (card.speedCount == lowestCount && card.speed > highestSpeed)
            {
                nextCard = card;
                lowestCount = card.speedCount;
                highestSpeed = card.speed;
            }
            card.speedCount -= card.speed;
        }



        nextCard.speedCount += 500;
        attackQueue.Enqueue(nextCard);


        Queue<CardInstance> newAttackQueue = new Queue<CardInstance>();
        bool dead = false;
        foreach (var card in attackQueue)
        {
            if (card == null || (card.GetComponent<HeroInstance>() != null && card.GetComponent<HeroInstance>().isDefeated))
            {
                dead = true;
            }
            else
            {
                newAttackQueue.Enqueue(card);
            }
        }
        attackQueue = newAttackQueue;
        if (dead) ProcessNextAttacker();
        UpdateAttackQueue();
    }

    private void UpdateAttackQueue()
    {
        while (attackQueue.Count < 4)
        {
            ProcessNextAttacker();
        }
        int num = 0;
        foreach (var card in attackQueue)
        {
            if (num > 3) continue;
            attackQueueImages[num].sprite = card.cardSprite.sprite;
            num += 1;
        }
        for (int i = num; i < 4; i++)
        {
            attackQueueImages[num].sprite = null;
        }
    }

    private void StartOfWaveRoutine()
    {
        attackQueue.Clear();
        ResetSpeeds();
        for (int i = 0; i < 4; i++)
        {
            ProcessNextAttacker();
        }
        BeginNextTurn();
    }

    private void BeginNextTurn()
    {
        if (playerField.AllHeroesDefeated())
        {
            bgm.Stop();
            EffectsManager.instance.CreateSoundEffect(lossSound, Vector3.zero);
            GameOverScreen.instance.Show();
            return;
        }

        ProcessNextAttacker();
        CardInstance card = attackQueue.Dequeue();
        UpdateAttackQueue();

        if (card.gameObject.GetComponent<HeroInstance>() != null)
        {
            StartCoroutine(PlayerTurnRoutine(card));
        }
        else
        {
            StartCoroutine(EnemyTurnRoutine(card));
        }
        currentAttackerImage.sprite = card.cardSprite.sprite;
    }

    private IEnumerator GameStartRoutine()
    {
        camController.enabled = false;
        yield return StartCoroutine(LoadNextRoomEnvironment());
        yield return StartCoroutine(MoveCamera(enterCameraPath));
        camController.enabled = true;
        // Spawn heroes
        for (int i = 0; i < heroSelectionData.selectedHeroes.Length; i++)
        {
            HeroInstance hero = Instantiate(heroSelectionData.selectedHeroes[i], playerField.transform).GetComponent<HeroInstance>();
            hero.name = heroSelectionData.selectedHeroes[i].name;
            //hero.SetCardData(hero);
            hero.troopsField = playerField;
            PlayerHeroes.Add(hero);
            playerField.AddCard(hero);
            playerDeck.availableElements.Add(hero.mainElement);
            yield return new WaitForSeconds(0.1f);
        }

        // Spawn enemy wave
        waveCounter++;
        InfoPanel.instance.UpdateWavesCount(waveCounter, maxWavesCount);

        yield return enemySpawner.SpawnWaveCoroutine();
        waveActive = true;


        //StartOfWaveRoutine();
        yield return StartCoroutine(PlayerTurnSimple()); // trying out simple turn order
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

            HealOnWaveClear();
            int enemyCount = 3;
            waveCounter++;
            if (waveCounter is (2 or 4 or 8))
                enemyCount = 4;
            if (waveCounter is (5 or 9))
                enemyCount = 5;

            enemySpawner.healthScale += hpScaleIncPerWave;
            enemySpawner.damageScale += dmgScaleIncPerWave;
            if (waveCounter < maxWavesCount)
            {
                InfoPanel.instance.UpdateWavesCount(waveCounter, maxWavesCount);
                yield return enemySpawner.SpawnWaveCoroutine(enemyCount);
                waveActive = true;

                // Return control to the player
                //StartOfWaveRoutine();
                yield return StartCoroutine(PlayerTurnSimple());
            }
            else if (waveCounter == maxWavesCount)
            {
                InfoPanel.instance.UpdateWavesCount(waveCounter, maxWavesCount);
                yield return enemySpawner.SpawnBossCoroutine();
                waveActive = true;

                // Return control to the player
                //StartOfWaveRoutine();
                yield return StartCoroutine(PlayerTurnSimple());
            }
            else
            {
                StartCoroutine(EndLevel());
            }
        }
    }

    private IEnumerator PlayerTurnSimple()
    {
        currentTurn++;

        if (playerField.AllHeroesDefeated())
        {
            GameOverScreen.instance.Show();
        }

        CardInstance[] playerUnits = playerField.GetCards().ToArray();
        foreach (var hero in playerUnits)
        {
            if (hero == null) continue;
            yield return StartCoroutine(hero.ProcessStartOfTurnEffects());
            yield return StartCoroutine(hero.ProcessPassivesTurnStart());
            yield return new WaitForSeconds(0.2f);
        }

        List<CardInstance> justHeroes = playerField.GetCards().ToList();
        justHeroes.RemoveAll(card =>
        {
            HeroInstance hero = card as HeroInstance;
            return hero == null || hero.isDefeated;
        });

        actionsThisTurn = 3;
        ResetAllAttacks();
        playerDeck.DrawUntilHandIsFull();

        SetPlayerInput(true);
        foreach (var hero in justHeroes)
            hero.ShowSelector(SelectionState.Active);

        yield return new WaitUntil(() => playerInput.EndTurnPressed);
        playerInput.EndTurnPressed = false;

        //HealPlayers(); bad idea. can be exploited :(

        SetPlayerInput(false);
        foreach (var hero in justHeroes)
            hero.HideSelector();

        if( enemyField.GetCards().Count > 0)
            StartCoroutine(SummonsTurnSimple());
    }
    private IEnumerator SummonsTurnSimple()
    {
        Debug.Log("Player minions turn start");
        List<CardInstance> enemyCards = enemyField.GetCards();
        List<CardInstance> summonCards = playerField.GetCards().ToList();
        summonCards.RemoveAll(card =>
        {
            HeroInstance hero = card as HeroInstance;
            return hero != null;
        });

        //enemyCards = enemyField.GetCards().ToList(); // Rebuild the list after potential removals
        summonCards.RemoveAll(e => e.GetComponent<FreezeEffect>() != null); // make frozen units skip action

        foreach (var summon in summonCards)
            summon.ShowSelector(SelectionState.Inactive);

        foreach (var summon in summonCards)
        {
            summon.ShowSelector(SelectionState.Active);
            enemyCards = enemyField.GetCards();
            if (summon == null) continue;
            if (enemyCards.Count == 0) break;

            BaseMonsterSkill[] skills = summon.gameObject.GetComponents<BaseMonsterSkill>();

            BaseMonsterSkill selectedSkill = skills[Random.Range(0, skills.Length)];

            CardInstance target = enemyCards[Random.Range(0, enemyCards.Count)];
            yield return selectedSkill.StartCoroutine(selectedSkill.Execute(target));
            yield return new WaitForSeconds(0.2f);
            summon.HideSelector();
        }
        Debug.Log("Player minions turn end");
        foreach (var summon in summonCards)
            summon.HideSelector();

        if (enemyField.GetCards().Count > 0)
            StartCoroutine(EnemyTurnSimple());
    }
    private IEnumerator EnemyTurnSimple()
    {
        List<CardInstance> enemyCards = enemyField.GetCards();
        List<CardInstance> playerCards = playerField.GetCards().ToList();
        playerCards.RemoveAll(card =>
        {
            HeroInstance hero = card as HeroInstance;
            return hero != null && hero.isDefeated;
        });

        foreach (var enemyCard in new List<CardInstance>(enemyCards))
        {
            if (enemyCard == null) continue;

            yield return StartCoroutine(enemyCard.ProcessStartOfTurnEffects());
            yield return StartCoroutine(enemyCard.ProcessPassivesTurnStart());

            yield return new WaitForSeconds(0.2f);
        }

        enemyCards = enemyField.GetCards().ToList(); // Rebuild the list after potential removals
        enemyCards.RemoveAll(e => e.GetComponent<FreezeEffect>() != null); // make frozen units skip action

        foreach (var enemyCard in enemyCards)
            enemyCard.ShowSelector(SelectionState.Inactive);
        foreach (var enemyCard in enemyCards)
        {
            enemyCard.ShowSelector(SelectionState.Active);
            if (enemyCard == null) continue;
            if (playerCards.Count == 0) break;

            BaseMonsterSkill[] skills = enemyCard.gameObject.GetComponents<BaseMonsterSkill>();

            BaseMonsterSkill selectedSkill = skills[Random.Range(0, skills.Length)];

            CardInstance target = playerCards[Random.Range(0, playerCards.Count)];
            yield return selectedSkill.StartCoroutine(selectedSkill.Execute(target));
            yield return new WaitForSeconds(0.5f);
            enemyCard.HideSelector();
        }

        yield return new WaitForSeconds(enemyTurnDuration);
        StartCoroutine(PlayerTurnSimple());
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
    private void Update()
    {
        actionsText.text = $"actions left: {actionsThisTurn}";
        if (actionsThisTurn > 0)
        {
            healText.gameObject.SetActive(true);
            healText.text = $"heal for {healLevels[actionsThisTurn - 1]} hp";
        }
        else
        {
            healText.gameObject.SetActive(false);
        }
    }



    private IEnumerator PlayerTurnRoutine(CardInstance hero)
    {
        currentTurn++;

        if (playerField.AllHeroesDefeated())
        {
            bgm.Stop();
            EffectsManager.instance.CreateSoundEffect(lossSound, Vector3.zero);
            GameOverScreen.instance.Show();
        }


        if (hero != null)
        {
            yield return StartCoroutine(hero.ProcessStartOfTurnEffects());
            yield return new WaitForSeconds(0.2f);
        }

        actionsThisTurn = 1;
        ResetAllAttacks();
        playerDeck.DrawMultiple(1);

        SetPlayerInput(true);
        yield return new WaitUntil(() => playerInput.EndTurnPressed);
        playerInput.EndTurnPressed = false;

        //HealPlayers(); bad idea. can be exploited :(

        SetPlayerInput(false);
        BeginNextTurn();
    }
    private IEnumerator EnemyTurnRoutine(CardInstance enemyCard)
    {
        List<CardInstance> enemyCards = enemyField.GetCards();
        List<CardInstance> playerCards = playerField.GetCards().ToList(); 
        
        playerCards.RemoveAll(card =>
        {
            HeroInstance hero = card as HeroInstance;
            return hero != null && hero.isDefeated;
        });

        if (enemyCard != null)
        {
            yield return StartCoroutine(enemyCard.ProcessStartOfTurnEffects());
            yield return StartCoroutine(enemyCard.ProcessPassivesTurnStart());

            yield return new WaitForSeconds(0.2f);
        }

        enemyCards = enemyField.GetCards().ToList(); // Rebuild the list after potential removals
        enemyCards.RemoveAll(e => e.GetComponent<FreezeEffect>() != null); // make frozen units skip action


        playerCards.RemoveAll(card =>
        {
            HeroInstance hero = card as HeroInstance;
            return hero != null && hero.isDefeated;
        });
        if (enemyCard != null && playerCards.Count > 0 && enemyCard.CurrentHealth > 0)
        {

            BaseMonsterSkill[] skills = enemyCard.gameObject.GetComponents<BaseMonsterSkill>().Where(x => x.enabled).ToArray();

            BaseMonsterSkill selectedSkill = skills[Random.Range(0, skills.Length)];

            CardInstance target = playerCards[Random.Range(0, playerCards.Count)];
            yield return selectedSkill.StartCoroutine(selectedSkill.Execute(target));
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(enemyTurnDuration);
        BeginNextTurn();
    }
    public void RegisterActionUse()
    {
        actionsThisTurn--;

        if (actionsThisTurn <= 0 || PlayerHand.instance.GetCards().Count == 0)
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
    private void HealPlayers()
    {
        // experimental... looking for ways to replace green mage in battles.
        if (actionsThisTurn > 0)
        {
            foreach (var hero in PlayerHeroes)
            {
                hero.Heal(healLevels[actionsThisTurn - 1]);
            }
        }
    }
    private void HealOnWaveClear()
    {
        // experimental... looking for ways to replace green mage in battles.
        if (actionsThisTurn > 0)
        {
            foreach (var hero in PlayerHeroes)
            {
                hero.maxHealth += 1;
                hero.Heal(10);
            }
        }
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

        EffectsManager.instance.CreateSoundEffect(useSound, Vector3.zero);

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
            RevieveFallenHeroes();
            yield return StartCoroutine(MoveHeroesThroughPath(exitPathPoints));
            currentStageIndex++;
            camController.enabled = false;
            yield return StartCoroutine(FadeScreen());
            camController.enabled = true;

            enemySpawner.healthScale += hpScaleIncPerStage;
            enemySpawner.damageScale += dmgScaleIncPerStage;

            //yield return StartCoroutine(MoveHeroesThroughPath(entryPathPoints));


            waveCounter = 1;
            yield return enemySpawner.SpawnWaveCoroutine();
            InfoPanel.instance.UpdateWavesCount(waveCounter, maxWavesCount);
            waveActive = true;
            //StartOfWaveRoutine(); 
            yield return StartCoroutine(PlayerTurnSimple());
        }
        else
        {
            bgm.Stop();
            yield return new WaitForSeconds(1f);
            EffectsManager.instance.CreateSoundEffect(winSound, Vector3.zero);
            InfoPanel.instance.Hide();
            yield return StartCoroutine(JustFadeOut());
            yield return new WaitForSeconds(4f);
            SceneController.ToEndScene();
        }
    }
    public void RevieveFallenHeroes()
    {
        foreach (var hero in PlayerHeroes)
        {
            if (hero.isDefeated)
                hero.Revive();
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
    public IEnumerator JustFadeOut()
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
        RenderSettings.skybox = currentRoom.skybox;
        enemySpawner.BossPrefab = currentRoom.StageBoss;
        enemySpawner.enemies = currentRoom.enemies.ToList();

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
        if (playerDeck.availableElements.Contains(element))
            playerDeck.availableElements.Remove(element);
    }
    public void AddElementToDeck(ElementType element)
    {
        if (!playerDeck.availableElements.Contains(element))
            playerDeck.availableElements.Add(element);
    }
}
