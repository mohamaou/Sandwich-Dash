using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;



[Serializable]
public class UIPanels
{
    [SerializeField] private GameObject startPanel, playPanel, winPanel, losePanel;
    public void SetPanel(GameState state = GameState.Start)
    {
        startPanel.SetActive(false);
        playPanel.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        return;
        switch (state)
        {
            case GameState.Start:
                startPanel.SetActive(true);
                break;
            case GameState.Play:
                playPanel.SetActive(true);
                break;
            case GameState.Lose:
                losePanel.SetActive(true);
                break;
            case GameState.Win:
                winPanel.SetActive(true);
                break;
        }
    }
}

public class UiManager : MonoBehaviour
{
    public static UiManager Instance {get; protected set;}
    public UIPanels panels;
    [SerializeField] private TextMeshProUGUI level;
    [SerializeField]private Transform hand;


    void Start()
    {
        Instance = this;
        panels.SetPanel();
        if(level != null)level.text = "Level " + GameManager.Level;
        hand.position = Input.mousePosition;
        hand.gameObject.SetActive(false);
    }


    private void Update()
    {
        hand.position = Vector3.Lerp(hand.position, Input.mousePosition, 12 * Time.deltaTime);
    }


    #region Button
    public void Play()
    {
        
    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Next()
    {
        var level = PlayerPrefs.GetInt("Level", 1) +1;
        PlayerPrefs.SetInt("Level", level);
        if (level >= SceneManager.sceneCountInBuildSettings)
        {
            level = Random.Range(0, SceneManager.sceneCountInBuildSettings);
        }
        SceneManager.LoadScene(level);
    }
    #endregion
}
