/* <License>The source code below is the property of Userware and is strictly confidential. It is licensed to OP.SERV under agreement 'USE-200-CLM-OPS'</License> */

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
