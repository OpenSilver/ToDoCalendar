/* <License>The source code below is the property of Userware and is strictly confidential. It is licensed to OP.SERV under agreement 'USE-200-CLM-OPS'</License> */

using System.Windows;
using System.Windows.Input;

namespace WinRTForSilverlight
{
    public partial class PointerRoutedEventArgs : RoutedEventArgs
    {
        MouseButtonEventArgs _e;

        public PointerRoutedEventArgs(MouseButtonEventArgs e)
        {
            _e = e;
        }

        public PointerPoint GetCurrentPoint(UIElement relativeTo)
        {
            Point point = _e.GetPosition(relativeTo);
            return new PointerPoint(point);
        }

        public bool Handled { get; set; }

    }
}
