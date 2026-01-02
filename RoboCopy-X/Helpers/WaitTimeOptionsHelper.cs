using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;

namespace RoboCopy_X.Helpers
{
    public static class WaitTimeOptionsHelper
    {
        public static List<ComboBoxItem> GenerateWaitTimeOptions()
        {
            var waitTimeOptions = new List<ComboBoxItem>();
            var waitTimes = new[] { 1, 5, 10, 15, 30, 60, 120, 180, 300, 600 };

            foreach (var seconds in waitTimes)
            {
                string displayText;
                if (seconds < 60)
                {
                    displayText = $"{seconds} segundo{(seconds == 1 ? "" : "s")}";
                }
                else
                {
                    int minutes = seconds / 60;
                    displayText = $"{minutes} minuto{(minutes == 1 ? "" : "s")} ({seconds}s)";
                }

                waitTimeOptions.Add(new ComboBoxItem
                {
                    Content = displayText,
                    Tag = seconds
                });
            }

            return waitTimeOptions;
        }
    }
}
