using Box2DX.Collision;
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
    class PolygonDrawable : Drawable
    {
        PolygonDef PolygonDef { get; }

        public PolygonDrawable(Body body, System.Windows.Media.Color color, PolygonDef polygonDef) : base(body, color)
        {
            PolygonDef = polygonDef;
        }

        public override void Draw(WriteableBitmap wb, float scale, float posX, float posY)
        {
            int[] asPoints(Vec2 v)
            {
                var worldVec = Body.GetWorldPoint(v);
                return new[] { (int)((worldVec.X+posX)*scale), (int)((worldVec.Y+posY)*scale), };
            }
            wb.FillPolygon(
                PolygonDef.Vertices.SelectMany(asPoints).Concat(asPoints(PolygonDef.Vertices[0])).ToArray(),
                Color);
        }
    }
}
