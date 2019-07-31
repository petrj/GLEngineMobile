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
    public class TapCrossMoveEvent
    {
        // all directions:  <0..100>

        public double Left { get; set; } = 0;
        public double Top { get; set; } = 0;
        public double Right { get; set; } = 0;
        public double Bottom { get; set; } = 0;

        public override string ToString()
        {
            return 
                $"\nLeft: {Left}\n" +
                $"Top: {Top}\n" +
                $"Right: {Right}\n" +
                $"Bottom: {Bottom}\n";
        }

        public static TapCrossMoveEvent GetTapMoveEvent(int width, int height, float x, float y)
        {
            var res = new TapCrossMoveEvent();

            if ( (x<0) || (x>width) ||
                 (y < 0) || (y > height))
            {
                return null;
            }

            var cx = (float)width / 2.0;
            var cy = (float)height / 2.0;

            if (x<cx)
            {
                res.Left = 100.0-x/cx*100;
            } else
                if (x > cx)
            {
                res.Right = (x-cx) / cx * 100;
            }

            if (y < cy)
            {
                res.Top = 100.0 - y / cy * 100;
            }
            else
                if (y > cy)
            {
                res.Bottom = (y - cy) / cy * 100;
            }

            if (res.Left < 0 ||
                res.Right > 100 ||
                res.Top < 0 ||
                res.Bottom > 100)
            {
                return null;
            }
            else
            {
                return res;
            }
        }
    }
}