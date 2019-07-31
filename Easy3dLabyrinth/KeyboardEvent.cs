using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Easy3DLabyrinth
{
    public class KeyboardEvent
    {
        public Keycode Key { get; set; }
        public KeyEvent Event { get; set; } = null;            
        public DateTime Time { get; set; } = DateTime.MinValue;
    }
}