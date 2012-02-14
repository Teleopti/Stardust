using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Support.Tool.DataLayer;

namespace Teleopti.Support.Tool
{
   public sealed class ConfigFunctions
    {
       private ConfigFunctions() { }
       
       /// <summary>
       /// Returns a Point that places the child control in center of it's parent
       /// </summary>
       /// <param name="parentWidth">The width of the parent</param>
       /// <param name="parentHeight">The height of the parent</param>
       /// <param name="width">The width of the control that you want to center</param>
       /// <param name="height">The height of the control that you want to center</param>
       /// <param name="relativeX">The pixels that you want to move the child from the center on the X axel</param>
       /// <param name="relativeY">The pixels that you want to move the child from the center on the Y axel</param>
       /// <returns>The Center point relative to relativeX and relativeY</returns>
        public static System.Drawing.Point Center(int parentWidth, int parentHeight, int width, int height, int relativeX, int relativeY)
        {
            int x = (parentWidth - width) / 2 + relativeX;
            int y = (parentHeight - height) / 2 + relativeY;
            if (x < 0)
                x = 0;
            if (y < 0)
                y = 0;
            return new System.Drawing.Point(x, y);
        }
           
          
        
    }
}
