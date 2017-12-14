using Box2DX.Common;
using Box2DX.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HPCFinalProject.Drawing
{
    abstract class Drawable
    {
        protected Body Body { get; }
        protected System.Windows.Media.Color Color { get; }
        protected Drawable(Body body, System.Windows.Media.Color color)
        {
            Body = body;
            Color = color;
        }

        public abstract void Draw(WriteableBitmap wb, float scale, float posX, float posY);
    }
}
