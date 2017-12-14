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
        IImmutableList<JointDefinition> Joints { get; }
        const float height = 5.0f;
        const float width = 5.0f;
        internal const float radiusMin = 0.1f;
        internal const float radiusMax = 1f;
        internal const float frictionMin = .001f;
        internal const float frictionMax = 1f;
        internal const float densityMin = .001f;
        internal const float densityMax = 1f;

        internal const float distanceAddMin = 0.0f;
        internal const float distanceAddMax = 10f;
        internal const float distanceAddTimeMin = 0.0f;
        internal const float distanceAddTimeMax = 10f;


        public CreatureDefinition(
            IImmutableList<NodeDefintion> nodes, 
            IImmutableList<JointDefinition> joints)
        {
            Nodes = nodes;
            Joints = joints;
        }

        public CreatureDefinition With(
            IImmutableList<NodeDefintion> nodes = null,
            IImmutableList<JointDefinition> joints = null)
            => new CreatureDefinition(
                nodes ?? Nodes,
                joints ?? Joints);

        /// <summary>
        /// Create creature with one node for starting
        /// </summary>
        /// <returns></returns>
        public static CreatureDefinition CreateSeedCreature()
        {
            var nodes = ImmutableArray.Create(CreateNewNode(ImmutableArray<NodeDefintion>.Empty));
            var constraints = ImmutableArray<JointDefinition>.Empty;
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
            return (node1.Pos - node2.Pos).Length() <=
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
            foreach (var joint in Joints)
            {
                var drawable = joint.ToJoint(world, bodies);
                drawables.Insert(0, drawable);

            }
            return (drawables, bodies);
        }

        public CreatureDefinition GetMutatedCreature()
        {
            //CreateNewNode(Nodes);
            // 1/4 chance of making a new node
            var newCreature = this;



            // Modify the nodes already on the creature
            newCreature = newCreature.With(
                nodes: Nodes.Select(node =>
                {
                    for (var i = 0; i < 10; i++)
                    {
                        var newNode = new NodeDefintion(
                            posX: (node.PosX + random.NextFloat(-width / 20, width / 20)).Inbetween(-width, width),
                            posY: (node.PosY + random.NextFloat(-height / 20, height / 20)).Inbetween(-height, height),
                            radius: (node.Radius + random.NextFloat(-.2f, .2f)).Inbetween(radiusMin, radiusMax),
                            friction: (node.Friction + random.NextFloat(-.2f, .2f)).Inbetween(frictionMin, frictionMax),
                            density: (node.Density + random.NextFloat(-.2f, .2f)).Inbetween(densityMin, densityMax));
                        if (NodesCollidingAllCheck(newNode, Nodes))
                        {
                            return newNode;
                        }
                    }
                    // if it can't find a way to make a node without pos and radius.
                    return new NodeDefintion(
                        posX: node.PosX,
                        posY: node.PosY,
                        radius: node.Radius,
                        friction: node.Friction + random.NextFloat(-.2f, .2f).Inbetween(.01f, 1f),
                        density: node.Density + random.NextFloat(-.2f, .2f).Inbetween(.01f, 1f));
                }).ToImmutableList());

            // Delete nodes randomly
            {
                var nodeCount = newCreature.Nodes.Count;
                if (nodeCount > 1)
                {
                    if (random.NextFloat(0.0f, 5.0f) < 1f)
                    {
                        newCreature = newCreature.With(
                            nodes: newCreature.Nodes.RemoveAt(nodeCount - 1),
                            joints: newCreature.Joints.Where(j => j.Node1Index != nodeCount - 1 && j.Node2Index != nodeCount -1).ToImmutableArray()
                            );
                    }
                }
            }

            // Add nodes randomly
            for (var i = 0; i < 2; i++)
            {
                if (random.NextFloat(0.0f, 4.0f) < 1.0f)
                {
                    var newNode = CreateNewNode(Nodes);
                    if (newNode != null)
                    {
                        newCreature = newCreature.With(
                            nodes: Nodes.Add(newNode));
                        // add joint to new node
                        var nodeCount = newCreature.Nodes.Count;
                        if (nodeCount > 1) {

                            for (var e = 0; e < 2; e++)
                            {
                                var node1Index = nodeCount - 1;
                                var node2Index = random.Next(0, nodeCount - 1);
                                var newJoint = new JointDefinition(
                                    node1Index: node1Index,
                                    node2Index: node2Index,
                                    length: 0,
                                    lengthDelta: random.NextFloat(distanceAddMin, distanceAddMax),
                                    motorInterval: random.NextFloat(distanceAddMin, distanceAddMax));
                                newCreature = newCreature.With(
                                    joints: newCreature.Joints.Add(newJoint));
                            }
                        }
                    }
                }
            }
            // delete joints randomly
            {
                var jointCount = newCreature.Joints.Count;
                if (jointCount > 1 && random.NextFloat(0.0f, 5.0f) < 1f) {
                    newCreature = newCreature.With(
                        joints: newCreature.Joints.RemoveAt(random.Next(0, jointCount)));
                }
            }

            // add joints randomly
            {
                var jointCount = newCreature.Joints.Count;
                var nodeCount = newCreature.Nodes.Count;
                if (jointCount < 30 && random.NextFloat(0.0f, 5.0f) < 1f)
                {
                    for (var i = 0; i < 10; i++)
                    {
                        var node1Index1 = random.Next(0, nodeCount);
                        var node2Index1 = random.Next(0, nodeCount);
                        if (node1Index1 != node2Index1 && !newCreature.Joints.Any(joint =>
                            (joint.Node1Index == node1Index1 && joint.Node2Index == node2Index1) ||
                            (joint.Node2Index == node1Index1 && joint.Node1Index == node2Index1))) {
                            var newJoint = new JointDefinition(
                                           node1Index: node1Index1,
                                           node2Index: node2Index1,
                                           length: 0,
                                           lengthDelta: random.NextFloat(distanceAddMin, distanceAddMax),
                                           motorInterval: random.NextFloat(distanceAddMin, distanceAddMax));
                            newCreature = newCreature.With(
                                joints: newCreature.Joints.Add(newJoint));
                        }
                    }
                    
                }
            }


            // fixup joint lenths
            newCreature = newCreature.With(
                joints: newCreature.Joints.Select(joint =>
                {
                    var node1 = newCreature.Nodes[joint.Node1Index];
                    var node2 = newCreature.Nodes[joint.Node2Index];
                    var distanceBetweenTwoNodes = (node1.Pos - node2.Pos).Length();

                    return joint.With(length: distanceBetweenTwoNodes);
                }).ToImmutableArray());
            return newCreature;
        }
    }
}
