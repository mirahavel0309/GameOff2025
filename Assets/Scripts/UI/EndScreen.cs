using UnityEngine;

public class EndScreen : MonoBehaviour
{
    public Animator blackScreen;
    void Start()
    {
        blackScreen.Play("FadeIn");
    }
    void Update()
    {
        
    }
    public void ToMain()
    {
        SceneController.ToMainMenu();
    }
}
