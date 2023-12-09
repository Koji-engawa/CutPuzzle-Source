using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

//タイトル画面の処理
public class Title : MonoBehaviour
{
    public TextMeshProUGUI highScoreText;
    public int highScore = 0;

    void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = "ハイスコア：ステージ" + highScore;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            SceneManager.LoadScene("MainScene");
    }
}
