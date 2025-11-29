using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void StartRun()
    {
        SceneManager.LoadScene("Base1");
    }
    public static void ToHeroSelection()
    {
        SceneManager.LoadScene("HeroSelection");
    }
    public static void ToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public static void ToGame()
    {
        SceneManager.LoadScene("Base3");
    }
    public static void ToEndScene()
    {
        SceneManager.LoadScene("EndScene");
    }
}
