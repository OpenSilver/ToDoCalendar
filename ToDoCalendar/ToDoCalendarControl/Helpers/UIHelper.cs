using System.Windows;
using System.Windows.Media;

namespace ToDoCalendarControl.Helpers
{
    public class UIHelper
    {
        public static T GetParentOfType<T>(DependencyObject source) where T : class
        {
            var parent = VisualTreeHelper.GetParent(source);
            while (parent != null)
            {
                if (parent is T)
                {
                    return parent as T;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
    }
}
