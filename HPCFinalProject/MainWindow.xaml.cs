using System;
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
        readonly IList<CreatureDefinition> creatures = new List<CreatureDefinition>();

        long lastElapsedMilli;
        const float scale = 30;
        const float fixedDeltaTime = 1f / 60;
        int imageWidth = 1920;
        int imageHeight = 1080;

        IImmutableList<CreatureDefinition> ListBoxCreatures = ImmutableArray<CreatureDefinition>.Empty;

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


            float posX = 0;
            float posY = 0;
            // calculate center of all nodes
            foreach (var nodeBody in nodeBodies)
            {
                var bodyPos = nodeBody.GetPosition();
                posX += bodyPos.X;
                posY += bodyPos.Y;
            }
            posX = posX / nodeBodies.Count() - imageWidth/(2*scale);
            posY = posY / nodeBodies.Count() - imageHeight/(2*scale);

            foreach(var drawable in drawables)
            {
                drawable.Draw(wb, scale, -posX, -posY);
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
            groundBodyDef.Position.Set(0.0f, 15.0f);

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
                await Task.Run(async () =>
                {
                    // Kill the weak
                    while (creatures.Count > numSurvivePerGeneration)
                    {
                        creatures.RemoveAt(creatures.Count - 1);
                    }
                    // Seeding
                    if (creatures.Count == 0)
                    {
                        while (creatures.Count < numPerGeneration)
                        {
                            creatures.Add(CreatureDefinition.CreateSeedCreature());
                        }
                    }
                    // make children
                    while (creatures.Count < numPerGeneration)
                    {
                        foreach (var creature in creatures.ToArray())
                        {
                            creatures.Add(creature.GetMutatedCreature());
                            if (creatures.Count >= numPerGeneration)
                            {
                                break;
                            }
                        }
                    }
                });
                GenerationDepthText.Text = $"{int.Parse(GenerationDepthText.Text) + 1}";
                ListBoxCreatures = creatures.ToImmutableArray();
                CreatureList.ItemsSource = ListBoxCreatures.Select((creature, index) => $"Creature {index}").ToArray();
            }



            StartButton.IsEnabled = true;

            // set text on what results I got back
           // NumPerGenerationInput.Text = (numPerGeneration * numPerGeneration).ToString();
            //GenerationDepthText.Text = "huehue";
            TimeToCalculateText.Text = $"{stopWatch.Elapsed.TotalSeconds}";
        }

        /*
       private async void Button_Click(object sender, RoutedEventArgs e)
       {
           GenerationDepthText.Text = "hi";

           var result = await Task.Run(async () =>
           {
               await Task.Delay(4000);

               var parallel = true;
               if (parallel)
               {
                   foreach (var generationNumber in Enumerable.Range(1, 1000))
                   {
                       var resultsForThisGeneration = await Task.WhenAll(
                           allMyCreatures.Select(i => Task.Run(() => Blah(i))));

                       // Process generation results (non-parallel).
                       foreach (var (distance, x) in resultsForThisGeneration)
                       {
                           Console.WriteLine(distance);
                       }
                   }
                   var results = await Task.WhenAll(
                       Enumerable.Range(1, 20).Select(i => Task.Run(() => BlahAsync(i))));

                   var resultsPlinq = Enumerable.Range(1, 20).AsParallel().Select(i => Blah(i)).OrderBy(x => -x.Distance).ToArray();
               }
               else
               {

               }

               return DateTime.Now.Second;
           });
           generationsTextBox.Text = $"result: {}";
       }*/

        private void Reset_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CreatureList.SelectedIndex < 0 || CreatureList.SelectedIndex >= ListBoxCreatures.Count)
                return;
            var creature = ListBoxCreatures[CreatureList.SelectedIndex];

            // Now somehow show it on the thing!
            (world, drawables, nodeBodies) = BuildWorld(creature);
        }
    }
}
