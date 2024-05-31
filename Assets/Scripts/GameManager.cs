using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Enemy[] finalEnemies;
    [SerializeField] private Knight knight;
    [SerializeField] private Slider lifebar;
    [SerializeField] private GameObject deathScreen;
    [SerializeField] private GameObject endScreen;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(knight.IsDead())
        {
            deathScreen.SetActive(true);
            if(Input.GetButtonDown("Restart"))
            {
                SceneManager.LoadScene("MainGame");
            }
        }
        else if(CheckEndGame())
        {
            endScreen.SetActive(true);
            if (Input.GetButtonDown("Restart"))
            {
                SceneManager.LoadScene("MainGame");
            }
        }
        else
        {
            lifebar.value = knight.GetHPRatio();
        }
    }

    private bool CheckEndGame()
    {
        bool endgame = true;
        foreach(Enemy e in finalEnemies)
        {
            endgame = endgame && e.IsDead();
        }
        return endgame;
    }
}
