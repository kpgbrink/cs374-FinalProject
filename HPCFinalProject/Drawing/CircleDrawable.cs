using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace HPCFinalProject.Drawing
{
    class CircleDrawable : Drawable
    {
        CircleDef CircleDef { get; }
        float Radius { get; }

        public CircleDrawable(Body body, System.Windows.Media.Color color, CircleDef circleDef) : base(body, color)
        {
            CircleDef = circleDef;
            Radius = (float)circleDef.Radius;
        }

        public override void Draw(WriteableBitmap wb, float scale, float posX, float posY)
        {
            var worldVec = Body.GetPosition();//Body.GetWorldPoint(new Vec2());
            wb.FillEllipseCentered(
                (int)((worldVec.X + posX) *scale), 
                (int)((worldVec.Y + posY) *scale), 
                (int)(Radius*scale), 
                (int)(Radius*scale), 
                Color);
        }
    }
}
