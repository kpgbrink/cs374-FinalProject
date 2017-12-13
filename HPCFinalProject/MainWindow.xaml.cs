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
        long lastElapsedMilli;

        public MainWindow()
        {
            InitializeComponent();
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            wb = new WriteableBitmap(200, 200, 96, 96, PixelFormats.Bgr32, null);
            image.Source = wb;
            // create new creature
            var creature = CreatureDefinition.CreateSeedCreature();
            (world, drawables) = BuildWorld(creature);
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        void Window_Closed(object sender, EventArgs e)
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            wb.Clear(Colors.Black);
            var ms = (long)chronometer.Elapsed.TotalMilliseconds;

            world.Step((ms - (float)lastElapsedMilli)/1000, 8, 1);

            lastElapsedMilli = ms;
            foreach(var drawable in drawables)
            {
                drawable.Draw(wb);
            }

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            generationsTextBox.Text = "hi";

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

        (int Distance, CreatureDefinition) CalculateCreatureThing(
            CreatureDefinition parameter) {
            System.Threading.Thread.Sleep(2000);
            return (1, 2);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private (World, IEnumerable<Drawing.Drawable>) BuildWorld(CreatureDefinition creatureDefinition)
        {
            AABB worldAABB = new AABB();
            worldAABB.LowerBound.Set(-9999.0f, -100.0f);
            worldAABB.UpperBound.Set(9999.0f, 1000.0f);
            var world = new World(worldAABB, gravity: new Vec2(0.0f, 10.0f), doSleep: false);

            // Define the ground body.
            var groundBodyDef = new BodyDef();
            groundBodyDef.Position.Set(0.0f, 150.0f);

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

            // Add circle that falls
            // Define the dynamic body. We set its position and call the body factory.
            var bodyDef = new BodyDef();
            bodyDef.Position.Set(100.0f, 20.0f);
            var body = world.CreateBody(bodyDef);

            // Define another box shape for our dynamic body.
            var shapeDef = new CircleDef
            {
                Radius = 10.0f,

                // Set the box density to be non-zero, so it will be dynamic.
                Density = 1.0f,

                // Override the default friction.
                Friction = 0.3f,
            };

            // Add the shape to the body.
            body.CreateShape(shapeDef);

            // Now tell the dynamic body to compute it's mass properties base
            // on its shape.
            body.SetMassFromShapes();


            var drawables = creatureDefinition.AddToWorld(world).Concat( new[] {
                new PolygonDrawable(groundBody, Colors.LimeGreen, groundShapeDef),
            }).ToArray();

         
            return (world, drawables);
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }
    }
}
