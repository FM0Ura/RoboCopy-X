using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;

namespace RoboCopy_X.Helpers
{
    public static class ThreadOptionsHelper
    {
        public static List<ComboBoxItem> GenerateThreadOptions(int systemThreadCount)
        {
            var threadOptions = new List<ComboBoxItem>();
            var presets = new[] { 1, 2, 4, 8, 16, 32, 64, 128 };
            int maxRobocopyThreads = Math.Min(systemThreadCount * 2, 128);

            foreach (var count in presets)
            {
                if (count <= maxRobocopyThreads)
                {
                    var item = new ComboBoxItem
                    {
                        Content = count == systemThreadCount
                            ? $"{count} threads (recomendado para este sistema)"
                            : count == systemThreadCount * 2
                                ? $"{count} threads (2x núcleos)"
                                : $"{count} threads",
                        Tag = count
                    };

                    threadOptions.Add(item);
                }
            }

            // If system has unusual thread count, add it
            if (!Array.Exists(presets, p => p == systemThreadCount))
            {
                var systemItem = new ComboBoxItem
                {
                    Content = $"{systemThreadCount} threads (recomendado para este sistema)",
                    Tag = systemThreadCount
                };

                // Insert in sorted position
                int insertIndex = threadOptions.FindIndex(t => (int)t.Tag > systemThreadCount);
                if (insertIndex == -1)
                    threadOptions.Add(systemItem);
                else
                    threadOptions.Insert(insertIndex, systemItem);
            }

            return threadOptions;
        }

        public static int GetDefaultThreadSelection(List<ComboBoxItem> options, int systemThreadCount)
        {
            for (int i = 0; i < options.Count; i++)
            {
                if (options[i].Tag is int count && count == systemThreadCount)
                {
                    return i;
                }
            }

            return 0;
        }
    }
}
