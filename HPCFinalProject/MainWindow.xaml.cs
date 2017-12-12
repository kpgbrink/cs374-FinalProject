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

namespace HPCFinalProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WriteableBitmap wb;
        Stopwatch chronometer = Stopwatch.StartNew();

        public MainWindow()
        {
            InitializeComponent();
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            wb = new WriteableBitmap(800, 600, 96, 96, PixelFormats.Bgr32, null);
            image.Source = wb;
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
            wb.FillRectangle(2, 50, 200, 400 + (int)((ms % 4000)*120/4000), Colors.LawnGreen);
            wb.DrawRectangle(2, 50, 200, 400, Colors.PeachPuff);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
