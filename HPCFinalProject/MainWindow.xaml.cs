﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Box2DX.Dynamics;
using Box2DX.Collision;
using Box2DX.Common;
using HPCFinalProject.Drawing;
using HPCFinalProject.Creature;
using System.Collections.Immutable;

namespace HPCFinalProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WriteableBitmap wb;
        Stopwatch chronometer = Stopwatch.StartNew();
        World world;
        IEnumerable<Drawable> drawables;
        IEnumerable<Body> nodeBodies;
        IImmutableList<(CreatureDefinition Creature, float? Distance)> creatures = ImmutableList<(CreatureDefinition, float?)>.Empty;

        long lastElapsedMilli;
        const float scale = 30;
        const float fixedDeltaTime = 1f / 60;
        int imageWidth = 1920;
        int imageHeight = 1080;
        bool resetButtonClicked = false;

        IImmutableList<(CreatureDefinition Creature, float? Distance)> ListBoxCreatures = ImmutableArray<(CreatureDefinition, float?)>.Empty;

        public MainWindow()
        {
            InitializeComponent();
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            wb = new WriteableBitmap(imageWidth, imageHeight, 96, 96, PixelFormats.Bgr32, null);
            image.Source = wb;
            // create new creature
            //var creature = CreatureDefinition.CreateSeedCreature();
            //for (var i = 0; i < 10; i++)
            //{
            //    creature = creature.GetMutatedCreature();
            //}
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        void Window_Closed(object sender, EventArgs e)
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            wb.Clear(Colors.LightBlue);
            if (world == null)
                return;

            var ms = (long)chronometer.Elapsed.TotalMilliseconds;

            var dt = fixedDeltaTime;//(ms - (float)lastElapsedMilli) / 1000;
            world.Step(dt, 8, 1);
            foreach (var drawable in drawables)
            {
                drawable.Step(dt);
            }

            lastElapsedMilli = ms;


            var (posX, posY) = CalculateDistance(nodeBodies);

            // Center on screen
            posX -= imageWidth / (2 * scale);
            posY -= imageHeight / (2 * scale);

            foreach (var drawable in drawables)
            {
                drawable.Draw(wb, scale, -posX, -posY);
            }

            for (var i = 0; i < 20; i++)
            {
                var xScale = 20;
                for (var negative = -1; negative < 2; negative += 2)
                {
                    wb.DrawLineAa(
                        (int)((xScale*negative*i - posX) * scale),
                        (int)((7 - posY) * scale),
                        (int)((xScale*negative*i - posX) * scale),
                        (int)((40 - posY) * scale),
                        i == 0 ? Colors.Orange : Colors.Red,
                        (int)(2 * scale));
                }
            }
        }

        async Task<(int Distance, CreatureDefinition Thing2)> BlahAsync(
            int parameter)
        {
            await Task.Delay(2000);
            return (1, null);
        }

        class BlahReasult
        {
            public CreatureDefinition Creature { get; }
            public float Distance { get; }

        }

        /*
        (int Distance, CreatureDefinition) CalculateCreatureThing(
            CreatureDefinition parameter) {
            System.Threading.Thread.Sleep(2000);
            return (1, 2);
        }
        */

        private (World, IEnumerable<Drawing.Drawable>, IEnumerable<Body>) BuildWorld(CreatureDefinition creatureDefinition)
        {
            var worldAABB = new AABB();
            worldAABB.LowerBound.Set(-9999.0f, -100.0f);
            worldAABB.UpperBound.Set(9999.0f, 1000.0f);
            var world = new World(worldAABB, gravity: new Vec2(0.0f, 10.0f), doSleep: false);

            // Define the ground body.
            var groundBodyDef = new BodyDef();
            groundBodyDef.Position.Set(0.0f, 17.0f);

            // Call the body factory which  creates the ground box shape.
            // The body is also added to the world.
            var groundBody = world.CreateBody(groundBodyDef);

            // Define the ground box shape.
            var groundShapeDef = new PolygonDef
            {
                Friction = 1,
            };

            // The extents are the half-widths of the box.
            groundShapeDef.SetAsBox(9999.0f, 10.0f);

            // Add the ground shape to the ground body.
            groundBody.CreateShape(groundShapeDef);

            var (drawables, nodeBodies) = creatureDefinition.AddToWorld(world);
            drawables = drawables.Concat(new[] {
                new PolygonDrawable(groundBody, Colors.LimeGreen, groundShapeDef),
            }).ToArray();

            return (world, drawables, nodeBodies);
        }

        private static (float X, float Y) CalculateDistance(IEnumerable<Body> bodies)
        {
            float posX = 0;
            float posY = 0;
            // calculate center of all nodes
            foreach (var nodeBody in bodies)
            {
                var bodyPos = nodeBody.GetPosition();
                posX += bodyPos.X;
                posY += bodyPos.Y;
            }
            posX = posX / bodies.Count();
            posY = posY / bodies.Count();
            return (posX, posY);
        }

        private async void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            // Get number of generation loops to do
            Int32.TryParse(GenerationsInput.Text, out var generations);
            Int32.TryParse(NumPerGenerationInput.Text, out var numPerGeneration);
            Int32.TryParse(NumSurvivePerGenerationInput.Text, out var numSurvivePerGeneration);
            Int32.TryParse(SimulationTimeInput.Text, out var simulationTime);
            Console.Write("The ints are: " + generations + numPerGeneration + numSurvivePerGeneration);

            StartButton.IsEnabled = false;

            var stopWatch = Stopwatch.StartNew();

            // Run the generations
            for (var i = 0; i < generations; i++)
            {
                var beParallel = ParallelCheckBox.IsChecked ?? false;
                creatures = await Task.Run(async () =>
                {
                    // Kill the weak
                    if (creatures.Count > numSurvivePerGeneration)
                    {
                        creatures = creatures.RemoveRange(numSurvivePerGeneration, creatures.Count - numSurvivePerGeneration);
                    }
                    // Seeding
                    if (creatures.Count == 0)
                    {
                        creatures = creatures.AddRange(Enumerable.Range(0, numPerGeneration).Select(j =>
                        {
                            return (CreatureDefinition.CreateSeedCreature(), (float?)null);
                        }));
                    }
                    // Shortcut for respecting beParallel
                    ParallelQuery<T> maybeParallel<T>(IEnumerable<T> enumerable) => (beParallel ? enumerable.AsParallel() : enumerable.AsParallel().WithDegreeOfParallelism(1));
                    // make children
                    while (creatures.Count < numPerGeneration)
                    {
                        // Ensure AsOrdered() because otherwise the fastest will win which might mean
                        // the things which roll the die a certain way resulting in always getting
                        // the same children somehow(?).
                        var maybeParallelMakeCreatures = maybeParallel(creatures.ToArray()).AsOrdered();
                        foreach (var newCreature in maybeParallelMakeCreatures.Select(parent => (parent.Creature.GetMutatedCreature(), (float?)null) ))
                        {
                            creatures = creatures.Add(newCreature);
                            if (creatures.Count >= numPerGeneration)
                            {
                                break;
                            }
                        }
                    }
                    // Run the distances
                    return maybeParallel(creatures).Select(creature =>
                    {
                        if (creature.Distance.HasValue)
                        {
                            creature.Distance *= .99f;
                            return creature;
                        }
                        var (world, drawables, bodies) = BuildWorld(creature.Creature);
                       
                        var ms = 0f;
                        var totalDt = simulationTime;
                        var startingXDistance = CalculateDistance(bodies).X;
                        while (ms < totalDt)
                        {
                            var dt = fixedDeltaTime;//(ms - (float)lastElapsedMilli) / 1000;
                            world.Step(dt, 8, 1);
                            foreach (var drawable in drawables)
                            {
                                drawable.Step(dt);
                            }
                            ms += fixedDeltaTime;

                            // Disallow creatures which get too big. Such creatures generally
                            // "cheat" by having the joints stick way out and jump by being
                            // flung out of the ground which they punch really hard. Reject
                            // by setting distance to negative so user can see that it was bad.
                            var xMin = bodies.Select(b => b.GetPosition().X).Min();
                            var xMax = bodies.Select(b => b.GetPosition().X).Max();
                            var yMin = bodies.Select(b => b.GetPosition().Y).Min();
                            var yMax = bodies.Select(b => b.GetPosition().Y).Max();
                            const float maxSize = 50;
                            if (xMax - xMin > maxSize || yMax - yMin > maxSize || yMin < -20) return (creature.Creature, Distance: (float?)-1);
                        }
                        var distanceTraveled = System.Math.Abs(CalculateDistance(bodies).X - startingXDistance);
                        return (creature.Creature, Distance: (float?)distanceTraveled);
                    }).OrderByDescending(result => result.Distance.Value).ToImmutableList();
                });
                GenerationDepthText.Text = $"{int.Parse(GenerationDepthText.Text) + 1}";

                ListBoxCreatures = creatures;
                CreatureList.ItemsSource = creatures.Select((creatureInfo, index) => $"Creature {index}; Distance: {creatureInfo.Distance}").ToArray();

                if (resetButtonClicked)
                {
                    creatures = ImmutableList<(CreatureDefinition, float?)>.Empty;
                    ListBoxCreatures = ImmutableArray<(CreatureDefinition, float?)>.Empty;
                    GenerationDepthText.Text = "0";
                    resetButtonClicked = false;
                    break;
                }
            }



            StartButton.IsEnabled = true;

            // set text on what results I got back
           // NumPerGenerationInput.Text = (numPerGeneration * numPerGeneration).ToString();
            //GenerationDepthText.Text = "huehue";
            TimeToCalculateText.Text = $"{stopWatch.Elapsed.TotalSeconds}";
        }

        private void Reset_Button_Click(object sender, RoutedEventArgs e)
        {
            resetButtonClicked = true;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CreatureList.SelectedIndex < 0 || CreatureList.SelectedIndex >= ListBoxCreatures.Count)
                return;
            var creature = ListBoxCreatures[CreatureList.SelectedIndex];

            // Now somehow show it on the thing!
            (world, drawables, nodeBodies) = BuildWorld(creature.Creature);
        }

        private void SimulationTimeInput_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
