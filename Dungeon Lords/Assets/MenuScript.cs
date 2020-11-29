
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public void Play()
    {
        GameObject.Find("__app").GetComponent<Game>().StartGame();

    }

    public void Quit()
    {
        Application.Quit();
    }
}
