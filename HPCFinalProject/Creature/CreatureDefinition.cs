using Box2DX.Dynamics;
using HPCFinalProject.Drawing;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;


namespace HPCFinalProject.Creature
{
    class CreatureDefinition
    {
        static readonly Random random = new Random();
        IImmutableList<NodeDefintion> Nodes { get; }
        IImmutableList<ConstraintDefinition> Constraints { get; }
        const float height = 5.0f;
        const float width = 5.0f;

        public CreatureDefinition(IImmutableList<NodeDefintion> nodes, IImmutableList<ConstraintDefinition> constraints)
        {
            Nodes = nodes;
            Constraints = constraints;
        }

        public static CreatureDefinition CreateSeedCreature()
        {
            var nodes = ImmutableArray.Create(CreateNewNode(ImmutableArray<NodeDefintion>.Empty));
            var constraints = ImmutableArray<ConstraintDefinition>.Empty;
            return new CreatureDefinition(nodes, constraints);
        }

        private static NodeDefintion CreateNewNode(
            IImmutableList<NodeDefintion> nodes)
        {
            return new NodeDefintion(
                posX: random.NextFloat(-width, width), 
                posY: random.NextFloat(-height, height), 
                radius: random.NextFloat(1f, 5f),
                friction: random.NextFloat(.001f, 1f),
                density: random.NextFloat(.1f, 1f));
        }

        public IEnumerable<Drawable> AddToWorld(World world)
        {
            var bodies = new List<Body>();
            foreach (var node in Nodes)
            {
                var (body, drawable) = node.ToBody(world);
                bodies.Add(body);
                yield return drawable;
            }
        }

        public CreatureDefinition GetMutatedCreature()
        {

            //CreateNewNode(Nodes);


            return this;
        }
    }
}
