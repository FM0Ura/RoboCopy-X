using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;

namespace RoboCopy_X.Helpers
{
    public static class RetryOptionsHelper
    {
        public static List<ComboBoxItem> GenerateRetryOptions()
        {
            var retryOptions = new List<ComboBoxItem>
            {
                new ComboBoxItem
                {
                    Content = "1 tentativa",
                    Tag = 1
                }
            };

            // Add intervals of 10 from 10 to 100
            for (int i = 10; i <= 100; i += 10)
            {
                retryOptions.Add(new ComboBoxItem
                {
                    Content = $"{i} tentativas",
                    Tag = i
                });
            }

            return retryOptions;
        }
    }
}
