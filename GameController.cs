using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

//残り回数やハイスコア、ゲームの進行を管理
public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    public int restCount = 5;
    public int cutCount = 0; //削除するか検討中
    public int stageNumber = 1;
    public int highScore = 0;

    public TextMeshProUGUI cutText;
    public TextMeshProUGUI restText;
    public TextMeshProUGUI stageText;
    public TextMeshProUGUI highScoreText;

    //シーンを再読み込みしてもインスタンスは常に一つだけとする
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    public void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        cutText = GameObject.Find("CutCount").GetComponent<TextMeshProUGUI>();
        restText = GameObject.Find("RestCount").GetComponent<TextMeshProUGUI>();
        stageText = GameObject.Find("StageName").GetComponent<TextMeshProUGUI>();
        highScoreText = GameObject.Find("HighScore").GetComponent<TextMeshProUGUI>();

        cutText.text = "カット回数:" + cutCount + "回"; //削除するか検討中
        restText.text = "残り:" + restCount + "回";
        stageText.text = "ステージ " + stageNumber;
        highScoreText.text = "ハイスコア：ステージ" + highScore;
    }

    public void UpdateCount()
    {
        cutCount++;
        restCount--;
        cutText.text = "カット回数:" + cutCount + "回"; //削除するか検討中
        restText.text = "残り:" + restCount + "回";
    }

    public void Clear()
    {
        restCount += 2;

        if (stageNumber > highScore)
            highScore = stageNumber;

        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();

        stageNumber++;
    }

    public void LoadTitleScene()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
