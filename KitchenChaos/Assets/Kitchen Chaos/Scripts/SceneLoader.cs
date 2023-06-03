using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
}
