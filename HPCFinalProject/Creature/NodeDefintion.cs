using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;
using HPCFinalProject.Drawing;
using System.Windows.Media;

namespace HPCFinalProject.Creature
{
    /// <summary>
    /// Part of the creature that has collision
    /// </summary>
    /// size, pos, friction
    internal class NodeDefintion
    {
        public float PosX { get; }
        public float PosY { get; }
        public float Radius { get; }
        public float Friction { get; }
        public float Density { get; }

        public NodeDefintion(float posX, float posY, float radius, float friction, float density)
        {
            PosX = posX;
            PosY = posY;
            Radius = radius;
            Friction = friction;
            Density = density;
        }

        public (Body, Drawable) ToBody(World world)
        {
            var circleBody = world.CreateBody(new BodyDef {
                Position = new Vec2(PosX, PosY),
            });
            var circleDef = new CircleDef
            {
                Radius = Radius,
                Density = Density,
                Friction = Friction,
            };
            circleBody.CreateShape(circleDef);
            var drawable = new CircleDrawable(
                circleBody,
                System.Windows.Media.Color.FromRgb(
                    (byte)(Friction * 255.0f),
                    80,
                    (byte)(Density * 255.0f)),
                circleDef);
            circleBody.SetMassFromShapes();

            return (circleBody, drawable);
        }
    }
}