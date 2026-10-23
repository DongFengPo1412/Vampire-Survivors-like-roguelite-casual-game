// Compiles the actual GameManager.cs and Pause.cs with engine-interface stubs.
// Outside Assets: no Unity player dependency, Play Mode or real audio/input.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Unity.VisualScripting { public class Unused { } }
namespace UnityEngine
{
    public class MonoBehaviour
    {
        public GameObject gameObject = new GameObject();
        public List<IEnumerator> Routines = new List<IEnumerator>();
        protected T GetComponent<T>() where T : new() => new T();
        protected void StartCoroutine(IEnumerator routine)
        {
            Routines.Add(routine);
            routine.MoveNext();
        }
    }
    public class HeaderAttribute : Attribute { public HeaderAttribute(string text) { } }
    public class GameObject
    {
        public bool activeSelf;
        public Transform transform = new Transform();
        public void SetActive(bool value) { activeSelf = value; }
    }
    public class Transform { public Vector3 localScale = Vector3.zero; }
    public class RectTransform : Transform { }
    public struct Vector3
    {
        int value;
        public static Vector3 zero => new Vector3();
        public static Vector3 one => new Vector3 { value = 1 };
        public static bool operator ==(Vector3 a, Vector3 b) => a.value == b.value;
        public static bool operator !=(Vector3 a, Vector3 b) => !(a == b);
        public override bool Equals(object obj) => obj is Vector3 v && this == v;
        public override int GetHashCode() => value;
    }
    public static class Time { public static float timeScale = 1, deltaTime = .016f; }
    public enum KeyCode { Escape }
    public static class Input
    {
        public static bool Escape;
        public static bool GetKeyDown(KeyCode key) => Escape;
    }
    public static class Mathf { public static int Min(int a, int b) => Math.Min(a, b); }
    public static class Application { public static void Quit() { } }
    public class WaitForSeconds { public WaitForSeconds(float seconds) { } }
}
namespace UnityEngine.SceneManagement
{
    public static class SceneManager
    {
        public static int Loads;
        public static void LoadScene(int scene) { Loads++; }
    }
}
public class Player { }
public class ObjectManager { }
public class LevelUp : UnityEngine.MonoBehaviour
{
    public void Select(int index) { }
    public void Show() { }
}
public class Result : UnityEngine.MonoBehaviour
{
    public bool Won, Lost;
    public void Win() { Won = true; }
    public void Lose() { Lost = true; }
}
public class AudioManager
{
    public static AudioManager instance = new AudioManager();
    public enum Sfx { Select, Lose, Win }
    public void PlayBgm(bool value) { }
    public void PlaySfx(Sfx value) { }
    public void FilterBgm(bool value) { }
}

public static class GameStateRegression
{
    static void Call(object instance, string method) => instance.GetType()
        .GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(instance, null);
    static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
    static GameManager Fresh(bool live = true)
    {
        UnityEngine.Input.Escape = false;
        UnityEngine.Time.timeScale = 1;
        var game = new GameManager
        {
            uiLevelUp = new LevelUp(), uiResult = new Result(),
            enemyCleaner = new UnityEngine.GameObject(), pause = new Pause(), isLive = live
        };
        GameManager.instance = game;
        Call(game.pause, "Awake");
        return game;
    }
    static void EndingIgnoresInput(bool win)
    {
        var game = Fresh();
        if (win) game.GameWon(); else game.GameOver();
        Check(!game.isLive && game.IsEnding && !game.uiResult.gameObject.activeSelf,
              "Ending must stop gameplay before the result delay");
        UnityEngine.Input.Escape = true;
        Call(game, "Update");
        game.ResumeGame(); game.pause.Show(); game.pause.Hide(); game.GameStart();
        Check(!game.isLive && !game.pause.IsPaused && UnityEngine.Time.timeScale == 1,
              "Input or resume changed ending state/froze its delay");
        game.GameOver(); game.GameWon();
        Check(game.Routines.Count == 1, "Duplicate result coroutine");
        game.Routines[0].MoveNext();
        Check(game.uiResult.gameObject.activeSelf && !game.isLive && UnityEngine.Time.timeScale == 0,
              "Result coroutine must still finish normally");
        Check(win ? game.uiResult.Won : game.uiResult.Lost, "Wrong result selected");
        Call(game, "Update");
        Check(!game.isLive, "Completed result resumed via Escape");
    }
    public static int Main()
    {
        EndingIgnoresInput(false);
        EndingIgnoresInput(true);

        var game = Fresh();
        game.gameTime = game.maxGameTime;
        UnityEngine.Input.Escape = true;
        Call(game, "Update");
        Check(game.IsEnding && !game.pause.IsPaused && UnityEngine.Time.timeScale == 1,
              "Timeout and Escape in one frame paused the ending coroutine");

        game = Fresh();
        game.pause.Show();
        Check(game.pause.IsPaused && !game.isLive && UnityEngine.Time.timeScale == 0, "Pause failed");
        UnityEngine.Input.Escape = true;
        Call(game, "Update");
        Check(!game.pause.IsPaused && game.isLive && UnityEngine.Time.timeScale == 1, "Pause resume failed");

        game = Fresh(false);
        UnityEngine.Input.Escape = true;
        Call(game, "Update");
        Check(!game.isLive, "Start menu resumed as a pause");

        game = Fresh();
        game.StopGame();
        game.uiLevelUp.gameObject.transform.localScale = UnityEngine.Vector3.one;
        UnityEngine.Input.Escape = true;
        Call(game, "Update"); game.pause.Hide();
        Check(!game.isLive && UnityEngine.Time.timeScale == 0, "Upgrade selection resumed as a pause");

        game = Fresh();
        game.pause.Hide();
        Check(game.isLive && !game.pause.IsPaused, "Hiding an unopened pause changed state");

        game = Fresh();
        game.GameOver();
        UnityEngine.Time.timeScale = 0;
        int loads = UnityEngine.SceneManagement.SceneManager.Loads;
        game.GameRetry();
        Check(UnityEngine.Time.timeScale == 1 && !game.isLive &&
              UnityEngine.SceneManagement.SceneManager.Loads == loads + 1, "Retry did not restore scene time");

        Console.WriteLine("PASS: 8 state scenarios using actual GameManager.cs and Pause.cs; no Play Mode/audio/input.");
        return 0;
    }
}
