using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void StartRun()
    {
        SceneManager.LoadScene("Base1");
    }
}
