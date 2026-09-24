using CSI.Runtime;
using UnityEngine;

namespace CSI.Core
{
    /// <summary>
    /// Persistent root that guarantees the cross-scene singletons (SaveSystem, ActiveCaseContext)
    /// exist no matter which scene is opened first (useful when testing a scene directly in the Editor).
    /// </summary>
    public static class GameBootstrap
    {
        private static bool _bootstrapped;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureBootstrapped()
        {
            if (_bootstrapped) return;
            _bootstrapped = true;

            var root = new GameObject("GameBootstrap");
            Object.DontDestroyOnLoad(root);

            if (SaveSystem.Instance == null)
                root.AddComponent<SaveSystem>();

            if (ActiveCaseContext.Instance == null)
                root.AddComponent<ActiveCaseContext>();
        }
    }
}
