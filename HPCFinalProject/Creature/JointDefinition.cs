using Box2DX.Common;
using Box2DX.Dynamics;
using HPCFinalProject.Drawing;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace HPCFinalProject.Creature
{
    internal class JointDefinition
    {
        //bool EnableLimit = false;
        public int Node1Index { get; }
        public int Node2Index { get; }
        float Length { get; }
        public float LengthDelta { get; }
        public float MotorInterval { get; }
        //bool EnableMotor = false;

        public JointDefinition(int node1Index, int node2Index, float length, float lengthDelta, float motorInterval)
        {
            Node1Index = node1Index;
            Node2Index = node2Index;
            Length = length;
            LengthDelta = lengthDelta;
            MotorInterval = motorInterval;
        }

        public JointDefinition With(
            int? node1Index = null,
            int? node2Index = null,
            float? length = null,
            float? lengthDelta = null,
            float? motorInterval = null)
            => new JointDefinition(
                node1Index ?? Node1Index,
                node2Index ?? Node2Index,
                length ?? Length,
                lengthDelta ?? LengthDelta,
                motorInterval ?? MotorInterval);

        public Drawable ToJoint(World world, IReadOnlyList<Body> bodies)
        {
            var body1 = bodies[Node1Index];
            var body2 = bodies[Node2Index];
            
            return new JointDrawable(
                body1, 
                body2,
                System.Windows.Media.Color.FromRgb((byte)(Length*255), 2, 3),
                this);
        }

        class JointDrawable : Drawable
        {
            JointDefinition Definition { get; }
            bool IsMaxLength { get; set; }
            float cummulativeDt;
            Joint Joint { get; set; }
            Body Body2 { get; }
            public JointDrawable(Body body, Body body2, System.Windows.Media.Color color, JointDefinition definition) : base(body, color)
            {
                Definition = definition;
                Body2 = body2;
                CreateJoint();
            }

            public override void Draw(WriteableBitmap wb, float scale, float posX, float posY)
            {
                var pos1 = Body.GetWorldPoint(new Vec2());
                var pos2 = Body2.GetWorldPoint(new Vec2());
                wb.DrawLineAa(
                    (int)((pos1.X+posX)*scale), 
                    (int)((pos1.Y+posY)*scale),
                    (int)((pos2.X+posX)*scale),
                    (int)((pos2.Y+posY)*scale), 
                    Color, ((int)(scale * 2)).Inbetween(1, 5));
            }

            public override void Step(float dt)
            {
                base.Step(dt);

                cummulativeDt += dt;
                if (cummulativeDt > Definition.MotorInterval)
                {
                    Body.GetWorld().DestroyJoint(Joint);
                    IsMaxLength = !IsMaxLength;
                    CreateJoint();
                    cummulativeDt = 0;
                }
            }

            void CreateJoint()
            {
                Joint = Body.GetWorld().CreateJoint(new DistanceJointDef
                {
                    Body1 = Body,
                    Body2 = Body2,
                    Length = Definition.Length + (IsMaxLength ? Definition.LengthDelta : 0),
                    DampingRatio = 0.9f,
                });
            }
        }
    }
}