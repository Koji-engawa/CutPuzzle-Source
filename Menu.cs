using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

//メニューUIの処理（今のところあきらめる処理のみ）
public class Menu : MonoBehaviour
{
    private GameObject gameController;

    void Awake()
    {
        TMP_Dropdown dropdown = GetComponent<TMP_Dropdown>();
        dropdown.onValueChanged.AddListener(Exit);
        gameController = GameObject.Find("GameController");
    }

    //あきらめる処理
    public void Exit(int i)
    {
        Destroy(gameController); //DontDestroyOnLoadしているので、あきらめるときは手動で削除する
        SceneManager.LoadScene("TitleScene");
    }
}
