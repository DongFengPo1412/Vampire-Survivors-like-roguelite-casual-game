// Standalone test harness for the actual LevelUp.cs source. This file is outside
// Assets and is not included in the Unity player. Stubs exercise selection only.
using System;
using System.Linq;
using System.Reflection;

namespace UnityEngine
{
    public class MonoBehaviour
    {
        protected T GetComponent<T>() where T : new() => new T();
        protected T[] GetComponentsInChildren<T>(bool includeInactive) => Array.Empty<T>();
    }
    public class RectTransform { public Vector3 localScale; }
    public struct Vector3 { public static Vector3 one, zero; }
    public static class Mathf { public static int Min(int a, int b) => Math.Min(a, b); }
    public static class Random
    {
        private static readonly System.Random Generator = new System.Random(42);
        public static int Range(int lo, int hi) => Generator.Next(lo, hi);
    }
}

public class TestObject
{
    public bool Active;
    public void SetActive(bool value) { Active = value; }
}
public class Item
{
    public bool CanUpgrade;
    public TestObject gameObject = new TestObject();
    public void OnClick() { }
}
public class GameManager
{
    public static GameManager instance = new GameManager();
    public void StopGame() { }
    public void ResumeGame() { }
}
public class AudioManager
{
    public enum Sfx { LevelUp, Select }
    public static AudioManager instance = new AudioManager();
    public void PlaySfx(Sfx value) { }
    public void FilterBgm(bool value) { }
}

public static class UpgradeSelectionRegression
{
    public static int Main()
    {
        bool[][] scenarios = {
            new[] { true, true, true, true, true },
            new[] { false, false, false, false, true },
            new[] { false, false, false, true, true },
            new[] { false, false, false, false, false },
            new[] { true, true }
        };
        foreach (bool[] state in scenarios)
        for (int trial = 0; trial < 100; trial++)
        {
            Item[] items = state.Select(available => new Item { CanUpgrade = available }).ToArray();
            var level = new LevelUp();
            typeof(LevelUp).GetField("items", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(level, items);
            typeof(LevelUp).GetMethod("Next", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(level, null);
            int expected = Math.Min(3, state.Count(available => available));
            if (items.Count(item => item.gameObject.Active) != expected ||
                items.Any(item => item.gameObject.Active && !item.CanUpgrade))
                throw new Exception("Upgrade availability/uniqueness regression");
        }
        Console.WriteLine("PASS: 5 availability scenarios, 500 selections using actual LevelUp.Next");
        return 0;
    }
}
