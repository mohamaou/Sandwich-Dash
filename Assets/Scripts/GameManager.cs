using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Start, Play, Lose, Win
}

[Serializable]
public class Part
{
    public Grade grade;
}



public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; protected set;}
    public static GameState State;
    public static int Level;
    [SerializeField] private Part[] parts;
    [HideInInspector] public int partIndex, heads;




    private void Awake()
    {
        Instance = this;
        State = GameState.Start;
        Level = PlayerPrefs.GetInt("Level", 1);
        Time.timeScale = 1f;
        TinySauce.OnGameStarted(Level.ToString());
        // Application.targetFrameRate = 120;
    }
    
    public void Play()
    {
        State = GameState.Play;
        UiManager.Instance.panels.SetPanel(State);
    }



    public Part GetActivePart()
    {
        return parts[partIndex];
    }
    private void Update()
    {
        print(Time.time);
        KeyBoard();
    }
    private void KeyBoard()
    {
        if (Input.GetKeyDown(KeyCode.N)) UiManager.Instance.Next();
        if (Input.GetKeyDown(KeyCode.R)) Restart();
        if (Input.GetKeyDown(KeyCode.S)) Time.timeScale = Time.timeScale  == 0.2f ? 1f :0.2f;
    }
    public void EndGame(bool win)
    {
        State = win ? GameState.Win : GameState.Lose;
        Invoke(nameof(ShowUi),1f);
        if (win)
        {
          
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    public bool IsDone()
    {
        heads--;
        return heads < 0;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ShowUi()
    {
         TinySauce.OnGameFinished(State == GameState.Win, 0, Level.ToString());
        UiManager.Instance.panels.SetPanel(State);
    }
}



#if UNITY_EDITOR
[CustomEditor(typeof(GameManager))]
public class EnemyPartEditor : Editor
{ 
    public override void OnInspectorGUI()
    { 
        base.OnInspectorGUI();
        if(GUILayout.Button("Randomized Food"))
        {
            var people = FindObjectsOfType<Food>();
            for (int i = 0; i < people.Length; i++)
            {
                people[i].SetRandomFood();
            }
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
