using System.Windows;

namespace WinRTForSilverlight
{
    public partial class PointerPoint
    {
        Point _position;

        public PointerPoint(Point position)
        {
            _position = position;
        }

        public Point Position
        {
            get
            {
                return _position;
            }
        }

    }
}
