using System.Threading.Tasks;
using System.Windows.Media;

namespace ToDoCalendar.Utils
{
    public static class FontLoader
    {
        /// <summary>
        /// Call this method before the UI is rendered to prevent visual glitches when the font is applied for the first time.
        /// </summary>
        public static async Task LoadAppFonts()
        {
            await FontFamily.LoadFontAsync("ms-appx:///ToDoCalendar/Fonts/Inter_VariableFont_slnt_wght.ttf");
        }
    }
}