using System.Collections.Generic;

namespace CSI.Interaction
{
    /// <summary>
    /// Lightweight registry so CameraTool doesn't have to FindObjectsByType every shutter press.
    /// Props register on enable, unregister on disable/destroy.
    /// </summary>
    public static class PhotographableRegistry
    {
        private static readonly List<IPhotographable> _all = new List<IPhotographable>();

        public static IReadOnlyList<IPhotographable> All => _all;

        public static void Register(IPhotographable item)
        {
            if (!_all.Contains(item))
                _all.Add(item);
        }

        public static void Unregister(IPhotographable item)
        {
            _all.Remove(item);
        }
    }
}
