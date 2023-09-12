using UnityEngine;
using UnityEngine.SceneManagement;
namespace KC
{
    public static class SceneLoader
    // static class exist independent of any scene's gameobject's instance,
    // thus can be accessed by any script running in "any scene"
    {
        // NOTE: make sure this enum is definied exactly as ur scene-name
        // or is same order of build-index and type cast enum val to int
        public enum Scene
        {
            MainMenuScene,
            GameScene,
            LoadingScene,
        }

        // intermediate loading scene are to prevent game-running when the scene is still
        // being loaded in backgroud which might caz a freezing screen kinda effect

        private static Scene targetScene;

        public static void Load(Scene targetScene)
        {
            SceneLoader.targetScene = targetScene;
            SceneManager.LoadScene(Scene.LoadingScene.ToString());
        }

        public static void Callback()
        {
            SceneManager.LoadScene(targetScene.ToString());
        }

        // debug along with class-name, game-obj-name
        public static void Log(this MonoBehaviour mono, string msg) => Debug.Log(LogFormat(mono, msg));
        public static void LogWarning(this MonoBehaviour mono, string msg) => Debug.LogWarning(LogFormat(mono, msg));
        public static void LogError(this MonoBehaviour mono, string msg) => Debug.LogError(LogFormat(mono, msg));
        static string LogFormat(this MonoBehaviour mono, string msg) => $"{mono.GetType()}[{mono.name}] :: {msg}";
    }

    // rpc fn cant be declared as static ( which will not work by extention methods ),
    // thus using Inheritence of Unity.Netcode.'s NetworkBehaviour to local class, i.e. KC.'s NetworkBehaviour
    // also rpc cant be generic, so for each typeof(T) in NetworkVariable<T>, we have to declare separte rpc fn()s
    // buts whats the point, as you cant even make single generic type function server-rpc as
    // NetworkVariable<T> of any type-T cant be passed as parameter in rpc calls as its is not INetworkSerializable
}