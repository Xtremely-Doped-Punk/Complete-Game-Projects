using UnityEngine;
using UnityEngine.UI;

namespace KC
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] Button playBtn;
        [SerializeField] Button quitBtn;

        private void Awake()
        {
            playBtn.onClick.AddListener(() =>
            {
                // play button clicked
                SceneLoader.Load(SceneLoader.Scene.GameScene);
            });

            quitBtn.onClick.AddListener(() =>
            {
                // quit button clicked
                Application.Quit();
            });

            playBtn.Select(); // keep one btn always selected
            // u can also the first selected option in event system to do the same
        }
    }
}