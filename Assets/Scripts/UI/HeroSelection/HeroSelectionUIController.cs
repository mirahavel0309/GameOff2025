using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameOff2025.Assets.Scripts.UI.HeroSelection;
using System.Linq;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;
public class HeroSelectionUIController : MonoBehaviour
{
    private List<HeroInfoModel> heroesSelected = new List<HeroInfoModel>();
    private HeroInfoModel heroSelectedNow;
    [Header("Hero Info UI Elements")]
    public TextMeshProUGUI nameText;
    public Image heroIcon;
    public TextMeshProUGUI descriptionText;
    public Button recruitButton;
    public Button dismisButton;
    public Button startButton;
    public List<UIHeroSelectedSlot> heroSelectedSlots;


    public HeroSelectionData heroSelectionData;
    void OnEnable()
    {
        UIHeroSelectionSlot.OnHeroSelected += SelectHero;
    }
    void OnDisable()
    {
        UIHeroSelectionSlot.OnHeroSelected -= SelectHero;
    }

    private void SelectHero(HeroInfoModel heroModel)
    {
        nameText.text = heroModel.heroName;
        heroIcon.sprite = heroModel.heroIcon.sprite;
        descriptionText.text = heroModel.heroDescription;
        if (heroModel.isRecruited)
        {
            recruitButton.gameObject.SetActive(false);
            dismisButton.gameObject.SetActive(true);
        }
        else
        {
            recruitButton.gameObject.SetActive(true);
            dismisButton.gameObject.SetActive(false);
        }
        heroSelectedNow = heroModel;
    }
    public void RecruitHero()
    {
        if (heroSelectedNow != null && !heroSelectedNow.isRecruited)
        {
            if (heroesSelected.Count >= heroSelectedSlots.Count)
            {
                PopUpManager.Instance.ShowMessage("Maximum number of recruited heroes reached.");
                Debug.Log("Maximum number of recruited heroes reached.");
                return;
            }
            heroSelectedNow.Recruit();
            if (!heroesSelected.Any(hero => hero.heroName == heroSelectedNow.heroName))
            {
                heroesSelected.Add(heroSelectedNow);
            }
            else
            {
                PopUpManager.Instance.ShowMessage("Hero is already recruited.");
                Debug.Log("Hero is already recruited.");
                return;
            }
            foreach (var slot in heroSelectedSlots)
            {
                if (slot.IsSelected() == false)
                {
                    slot.MarkAsSelected(heroSelectedNow);
                    break;
                }
            }
        }
        if (heroesSelected.Count == 3)
        {
            startButton.gameObject.SetActive(true);
        }
    }

    public void DismissHero()
    {
        if (heroSelectedNow != null && heroSelectedNow.isRecruited)
        {
            heroSelectedNow.Dismiss();
            heroesSelected.RemoveAll(hero => hero.heroName == heroSelectedNow.heroName);
            foreach (var slot in heroSelectedSlots)
            {
                if (slot.IsSelected() && slot.GetHeroName() == heroSelectedNow.heroName)
                {
                    slot.UnmarkAsSelected();
                    break;
                }
            }
            recruitButton.gameObject.SetActive(true);
            dismisButton.gameObject.SetActive(false);
        }
        if (heroesSelected.Count < 3)
        {
            startButton.gameObject.SetActive(false);
        }
    }

    public void SaveHero()
    {

        GameObject[] heroesData = new GameObject[this.heroSelectedSlots.Count];

        for (int i = 0; i < this.heroSelectedSlots.Count; i++)
        {
            heroesData[i] = this.heroSelectedSlots[i].GetHeroPrefab();
        }
        heroSelectionData.SaveHeroes(heroesData);
    }

}
