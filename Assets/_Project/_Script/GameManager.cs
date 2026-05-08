using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public string menuSceneName = "Menu";   // Nếu bạn có scene Menu

    private int currentLevelIndex;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);   // Giữ qua các scene
    }

    private void Start()
    {
        currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
    }

    

    public void NextLevel()
    {
        int nextIndex = currentLevelIndex + 1;

        // Nếu là level cuối (ví dụ level 3) thì quay về level 1 hoặc Menu
        if (nextIndex > 3)
            nextIndex = 1;           // Hoặc 0 nếu bạn có Menu

        LoadLevel(nextIndex);
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 1) levelIndex = 1;
        currentLevelIndex = levelIndex;
        SceneManager.LoadScene(levelIndex);
    }

    public void RestartCurrentLevel()
    {
        // Reset Player + LeverManager
        PlayerController player = FindObjectOfType<PlayerController>();
        LeverManager lever = FindObjectOfType<LeverManager>();

        if (player != null)
            player.ResetState();

        if (lever != null)
            lever.ResetToOriginalData();     // Sẽ dùng hàm này sau

        // Reload scene để reset sạch sẽ
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ====================== UTILITY ======================

    public void GoToMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }

    public int GetCurrentLevel()
    {
        return currentLevelIndex;
    }
}