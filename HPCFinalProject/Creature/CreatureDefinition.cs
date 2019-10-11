using Box2DX.Dynamics;
using HPCFinalProject.Drawing;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace HPCFinalProject.Creature
{
    class CreatureDefinition
    {
        [ThreadStatic]
        static Random random;
        static Random Random => LazyInitializer.EnsureInitialized(ref random);
        ImmutableList<NodeDefintion> Nodes { get; }
        ImmutableList<JointDefinition> Joints { get; }
        const float height = 5.0f;
        const float width = 5.0f;
        internal const float radiusMin = 0.1f;
        internal const float radiusMax = 1f;
        internal const float frictionMin = .7f;
        internal const float frictionMax = 1f;
        internal const float densityMin = .001f;
        internal const float densityMax = 1f;

        internal const float distanceAddMin = 0.0f;
        internal const float distanceAddMax = 10f;
        internal const float distanceAddTimeMin = 0.0f;
        internal const float distanceAddTimeMax = 2f;


        public CreatureDefinition(
            ImmutableList<NodeDefintion> nodes, 
            ImmutableList<JointDefinition> joints)
        {
            Nodes = nodes;
            Joints = joints;
        }

        public CreatureDefinition With(
            ImmutableList<NodeDefintion> nodes = null,
            ImmutableList<JointDefinition> joints = null)
            => new CreatureDefinition(
                nodes ?? Nodes,
                joints ?? Joints);

        /// <summary>
        /// Create creature with one node for starting
        /// </summary>
        /// <returns></returns>
        public static CreatureDefinition CreateSeedCreature()
        {
            var nodes = ImmutableList.Create(CreateNewNode(ImmutableArray<NodeDefintion>.Empty));
            var constraints = ImmutableList<JointDefinition>.Empty;
            return new CreatureDefinition(nodes, constraints);
        }

        private static NodeDefintion CreateNewNode(IImmutableList<NodeDefintion> nodes)
        {
            // try to create a new node 100 times
            // if cannot find an empty space then just give up and move on
            for (var i = 0; i < 100; i++)
            {
                var newNode = new NodeDefintion(
                    posX: Random.NextFloat(-width, width),
                    posY: Random.NextFloat(-height, height),
                    radius: Random.NextFloat(radiusMin, radiusMax),
                    friction: Random.NextFloat(frictionMin, frictionMax),
                    density: Random.NextFloat(densityMin, densityMax));
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
            var change = 0.01f;
            newCreature = newCreature.With(
                nodes: Nodes.Select(node =>
                {
                    if (Random.NextFloat(0.0f, 1.0f) < 0.2f)
                    {
                        return node;
                    }
                    for (var i = 0; i < 10; i++)
                    {
                        var newNode = new NodeDefintion(
                            posX: (node.PosX + Random.NextFloat(-width / 10, width / 10)).Inbetween(-width, width),
                            posY: (node.PosY + Random.NextFloat(-height / 10, height / 10)).Inbetween(-height, height),
                            radius: (node.Radius + Random.NextFloat(-change, change)).Inbetween(radiusMin, radiusMax),
                            friction: (node.Friction + Random.NextFloat(-change, change)).Inbetween(frictionMin, frictionMax),
                            density: (node.Density + Random.NextFloat(-change, change)).Inbetween(densityMin, densityMax));
                        // Exclude original node from collision check list so that the new node can be slightly displaced
                        if (NodesCollidingAllCheck(newNode, Nodes.Remove(node)))
                        {
                            return newNode;
                        }
                    }
                    // if it can't find a way to make a node without pos and radius.
                    return new NodeDefintion(
                        posX: node.PosX,
                        posY: node.PosY,
                        radius: node.Radius,
                        friction: node.Friction + Random.NextFloat(-.1f, .1f).Inbetween(frictionMin, frictionMax),
                        density: node.Density + Random.NextFloat(-.2f, .2f).Inbetween(densityMin, densityMax));
                }).ToImmutableList());


            var nodeChance = 100.0f;
            var nodeLoop = 100;
            // Delete nodes randomly
            {
                for (var i = 0; i < nodeLoop; i++)
                {
                    var nodeCount = newCreature.Nodes.Count;
                    if (nodeCount <= 1)
                    {
                        break;
                    }
                    if (Random.NextFloat(0.0f, nodeChance / 2) < 1f)
                    {
                        newCreature = newCreature.RemoveNode(Random.Next(nodeCount));
                    }
                }
            }

            // Add nodes randomly
            for (var i = 0; i < nodeLoop; i++)
            {
                if (Random.NextFloat(0.0f, nodeChance) < 1.0f)
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
                                var node2Index = Random.Next(0, nodeCount - 1);
                                var newJoint = new JointDefinition(
                                    node1Index: node1Index,
                                    node2Index: node2Index,
                                    length: 0,
                                    lengthDelta: Random.NextFloat(distanceAddMin, distanceAddMax),
                                    motorInterval: Random.NextFloat(distanceAddMin, distanceAddMax));
                                newCreature = newCreature.With(
                                    joints: newCreature.Joints.Add(newJoint));
                            }
                        }
                    }
                }
            }


            var jointChance = 100.0f;
            var jointLoop = 100;
            // delete joints randomly
            {
                for (var i = 0; i < jointLoop; i++)
                {
                    var jointCount = newCreature.Joints.Count;
                    if (jointCount > 1 && Random.NextFloat(0.0f, jointChance) < 1f)
                    {
                        newCreature = newCreature.With(
                            joints: newCreature.Joints.RemoveAt(Random.Next(0, jointCount)));
                    }
                }
            }

            // modify joints
            {
                newCreature = newCreature.With(
                    joints: newCreature.Joints.Select(joint =>
                    {
                        return joint.With(
                            lengthDelta: (joint.LengthDelta + Random.NextFloat(-.1f, .1f)).Inbetween(distanceAddMin, distanceAddMax),
                            motorInterval: (joint.MotorInterval + Random.NextFloat(-.1f, .1f)).Inbetween(distanceAddTimeMin, distanceAddTimeMax)
                            );
                    }).ToImmutableList());
            }

            // add joints randomly
            {
                var jointCount = newCreature.Joints.Count;
                for (var i = 0; i < jointLoop; i++)
                {
                    jointCount = newCreature.Joints.Count;
                    var nodeCount = newCreature.Nodes.Count;
                    if (jointCount < 30 && Random.NextFloat(0.0f, jointChance) < 1f)
                    {
                        var node1Index1 = Random.Next(0, nodeCount);
                        var node2Index1 = Random.Next(0, nodeCount);
                        if (node1Index1 != node2Index1 && !newCreature.Joints.Any(joint =>
                            (joint.Node1Index == node1Index1 && joint.Node2Index == node2Index1) ||
                            (joint.Node2Index == node1Index1 && joint.Node1Index == node2Index1)))
                        {
                            var newJoint = new JointDefinition(
                                            node1Index: node1Index1,
                                            node2Index: node2Index1,
                                            length: 0,
                                            lengthDelta: Random.NextFloat(distanceAddMin, distanceAddMax),
                                            motorInterval: Random.NextFloat(distanceAddMin, distanceAddMax));
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
                }).ToImmutableList());
            return newCreature;
        }

        public CreatureDefinition RemoveNode(
            int index)
        {
            int? mapNodeIndex(int nodeIndex) => nodeIndex < index ? nodeIndex
                : nodeIndex > index ? nodeIndex - 1
                : (int?)null;
            var jointsBuilder = Joints.ToBuilder();
            for (var i = jointsBuilder.Count - 1; i >= 0; i--)
            {
                var joint = jointsBuilder[i];
                var newNode1Index = mapNodeIndex(joint.Node1Index);
                var newNode2Index = mapNodeIndex(joint.Node2Index);
                if (newNode1Index.HasValue
                    && newNode2Index.HasValue)
                {
                    // Update any joints referencing nodes after the removed
                    // one to follow the shifted nodeds.
                    if (newNode1Index != joint.Node1Index
                        || newNode2Index != joint.Node2Index)
                    {
                        jointsBuilder[i] = joint.With(
                            node1Index: newNode1Index,
                            node2Index: newNode2Index);
                    }
                }
                else
                {
                    // Remove any joint referencing the removed node
                    jointsBuilder.RemoveAt(i);
                }
            }
            return With(
                nodes: Nodes.RemoveAt(index),
                joints: jointsBuilder.ToImmutable());
        }
    }
}
