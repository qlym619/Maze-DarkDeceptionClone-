using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // 开始游戏按钮调用的方法
    public void StartGame()
    {
        Debug.Log("开始游戏！");
        // 加载游戏场景
        SceneManager.LoadScene("GameScene");
    }

    // 退出游戏按钮调用的方法
    public void QuitGame()
    {
        Debug.Log("退出游戏");

        // 在正式版游戏中退出
        Application.Quit();

        // 在Unity编辑器中停止运行
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // 从游戏场景返回主菜单
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // 重新开始当前关卡
    public void RestartCurrentLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}