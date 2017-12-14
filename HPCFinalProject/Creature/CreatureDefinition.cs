using Box2DX.Dynamics;
using HPCFinalProject.Drawing;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace HPCFinalProject.Creature
{
    class CreatureDefinition
    {
        static readonly Random random = new Random();
        IImmutableList<NodeDefintion> Nodes { get; }
        IImmutableList<ConstraintDefinition> Constraints { get; }
        const float height = 10.0f;
        const float width = 10.0f;
        const float radiusMin = 0.1f;
        const float radiusMax = 2f;
        const float frictionMin = .001f;
        const float frictionMax = 1f;
        const float densityMin = .001f;
        const float densityMax = 1f;


        public CreatureDefinition(
            IImmutableList<NodeDefintion> nodes, 
            IImmutableList<ConstraintDefinition> constraints)
        {
            Nodes = nodes;
            Constraints = constraints;
        }

        /// <summary>
        /// Create creature with one node for starting
        /// </summary>
        /// <returns></returns>
        public static CreatureDefinition CreateSeedCreature()
        {
            var nodes = ImmutableArray.Create(CreateNewNode(ImmutableArray<NodeDefintion>.Empty));
            var constraints = ImmutableArray<ConstraintDefinition>.Empty;
            return new CreatureDefinition(nodes, constraints);
        }

        private static NodeDefintion CreateNewNode(IImmutableList<NodeDefintion> nodes)
        {
            // try to create a new node 100 times
            // if cannot find an empty space then just give up and move on
            for (var i = 0; i < 100; i++)
            {
                var newNode = new NodeDefintion(
                    posX: random.NextFloat(-width, width),
                    posY: random.NextFloat(-height, height),
                    radius: random.NextFloat(radiusMin, radiusMax),
                    friction: random.NextFloat(frictionMin, frictionMax),
                    density: random.NextFloat(densityMin, densityMax));
                // return new node if no nodes
                if (nodes.Count == 0 || NodesCollidingAllCheck(newNode, nodes))
                {
                    return newNode;
                }
            }
            return null;
        }

        /// <summary>
        /// Make sure the nodes are not colliding
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        private static bool NodesCollidingAllCheck(NodeDefintion node, IImmutableList<NodeDefintion> nodes)
        {
            var notColliding = true;
            foreach (var nodeCheck in nodes)
            {
                if (NodesColliding(node, nodeCheck))
                {
                    notColliding = false;
                }
            }
            return notColliding;
        }

        private static bool NodesColliding(NodeDefintion node1, NodeDefintion node2)
        {
            return Math.Sqrt(Math.Pow(node1.PosX - node2.PosX, 2) + Math.Pow(node1.PosY - node2.PosY, 2)) <=
                (node1.Radius + node2.Radius);
        }

        public (IEnumerable<Drawable>, IEnumerable<Body>) AddToWorld(World world)
        {
            var bodies = new List<Body>();
            var drawables = new List<Drawable>();
            foreach (var node in Nodes)
            {
                var (body, drawable) = node.ToBody(world);
                drawables.Add(drawable);
                bodies.Add(body);
                //yield return drawable;
            }
            return (drawables, bodies);
        }

        public static float Inbetween(float number, float min, float max)
        {
            return Math.Min(max, Math.Max(number, min));
        }

        public CreatureDefinition GetMutatedCreature()
        {
            //CreateNewNode(Nodes);
            // 1/4 chance of making a new node
            var newCreature = this;

            // Modify the nodes already on the creature
            newCreature = new CreatureDefinition(Nodes.Select((node, i) =>
            {
                for (i = 0; i < 10; i++)
                {
                    var newNode = new NodeDefintion(
                        posX: Inbetween(node.PosX + random.NextFloat(-width / 20, width / 20), -width, width),
                        posY: Inbetween(node.PosY + random.NextFloat(-height / 20, height / 20), -height, height),
                        radius: Inbetween(node.Radius + random.NextFloat(-.2f, .2f), radiusMin, radiusMax),
                        friction: Inbetween(node.Friction + random.NextFloat(-.2f, .2f), frictionMin, frictionMax),
                        density: Inbetween(node.Density + random.NextFloat(-.2f, .2f), densityMin, densityMax));
                    if (NodesCollidingAllCheck(newNode, Nodes))
                    {
                        return newNode;
                    }
                }
                // if it can't find a way to make a node without changing things.
                return new NodeDefintion(
                    posX: node.PosX,
                    posY: node.PosY,
                    radius: node.Radius,
                    friction: Inbetween(node.Friction + random.NextFloat(-.2f, .2f), .01f, 1f),
                    density: Inbetween(node.Density + random.NextFloat(-.2f, .2f), .01f, 1f));
            }).ToImmutableList(), Constraints);

            // Add nodes randomly
            for (var i = 0; i < 4; i++)
            {
                if (random.NextFloat(0.0f, 4.0f) < 1.0f)
                {
                    var newNode = CreateNewNode(Nodes);
                    if (newNode != null)
                    {
                        newCreature = new CreatureDefinition(Nodes.Add(newNode), Constraints);
                    }
                }
            }


            return newCreature;
        }
    }
}
