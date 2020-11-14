using System.Reflection;
using System.Windows.Forms;

namespace Dendrite
{
    public static class ListViewExtensions
    {
        public static void SetDoubleBuffered(this PictureBox listView, bool value)
        {
            listView.GetType()
                .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(listView, value);
        }
    }
}
