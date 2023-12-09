using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//全てのピースがフレームに収まっているかをチェックする
public class ClearChecker : MonoBehaviour
{
    public List<GameObject> pieces;
    public List<Fit> fits;
    private GameObject clear;
    private SoundManager soundManager;
    private GameController gameController;

    //フレームに持たせるスクリプトなので、フレームとして必要な設定をする
    void Awake()
    {
        gameController = FindObjectOfType<GameController>();
        SortingLayer sortingLayer = gameObject.AddComponent<SortingLayer>();
        sortingLayer.LayerName = "Frame";
        sortingLayer.OrderInLayer = 0;
        gameObject.tag = "Frame";
    }

    void Start()
    {
        //シーンの再読み込み後は自動実行されないので手動で実行
        gameController.Start();

        clear = GameObject.Find("Clear");
        clear.SetActive(false);

        pieces = new List<GameObject>(GameObject.FindGameObjectsWithTag("Piece"));
        fits = new List<Fit>();
        soundManager = GameObject.FindObjectOfType<SoundManager>();

        foreach (GameObject piece in pieces)
        {
            fits.Add(piece.GetComponent<Fit>());
        }
    }

    //全てのピースがフレームに収まっていればクリア
    public void ClearCheck()
    {
        if (fits.TrueForAll(fit => fit.fitted))
        {
            clear.SetActive(true);
            soundManager.Play(soundManager.clearMusic);
            soundManager.bgm.Stop();

            gameController.Clear();//GameControllerにクリアを通知

            Invoke("ReloadScene", 2.3f); // 時間差でシーンの再読み込み
        }
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(0);
    }
}
