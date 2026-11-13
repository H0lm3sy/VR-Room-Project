using UnityEngine;
using TMPro;

public class GameManagerBottle : MonoBehaviour
{
    public static GameManagerBottle Instance;
    public TextMeshProUGUI scoreText;
    int score = 0;

    void Awake() => Instance = this;

    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = "Score: " + score;
    }
}