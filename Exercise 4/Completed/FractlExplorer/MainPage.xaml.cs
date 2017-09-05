using System;
using Xamarin.Forms;
using FractlExplorer;
using System.Numerics;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using FractlExplorer.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FractlExplorer
{
    public partial class MainPage : ContentPage
    {
        const int maxIterations = 100;

        Complex center = new Complex(0, 0), moving;
        double scale = 3.0, pinchScale;
        int[] colors = { Color.Black.ToABGR(), Color.Red.ToABGR(), Color.Blue.ToABGR(), Color.Yellow.ToABGR(), Color.Green.ToABGR() };
        int[] memoryBuffer;
        double viewWidth, viewHeight;
        CancellationTokenSource tokenSource;
            
        public MainPage()
        {
            InitializeComponent();
        }

        void OnImagePanned(object sender, PanUpdatedEventArgs e)
        {
            double increment = scale / viewWidth;

            if (e.StatusType == GestureStatus.Started)
            {
                moving = center;
            }
            else if (e.StatusType == GestureStatus.Running)
            {
                center = new Complex(
                    moving.Real + (-1 * e.TotalX * increment),
                    moving.Imaginary + (-1 * e.TotalY * increment));
                // regenerate fractl on move
                StartRenderAsync();
            }
            else if (e.StatusType == GestureStatus.Completed)
            {
                StartRenderAsync();
            }
        }

        void OnImagePinched(object sender, PinchGestureUpdatedEventArgs e)
        {
            if (e.Status == GestureStatus.Started)
            {
                Point pt = new Point(e.ScaleOrigin.X * viewWidth, e.ScaleOrigin.Y * viewHeight);
                center = FromPoint(pt);
                pinchScale = scale;
            }
            else if (e.Status == GestureStatus.Running)
            {
                double additive = (e.Scale-1) * pinchScale;
                scale += additive * -1;
                scale = Math.Min (5, Math.Max(.1, scale));  
                // regenerate fractl on scale
                StartRenderAsync();
            }
            else if (e.Status == GestureStatus.Completed)
            {
                StartRenderAsync();  
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (SetImageSize(width, height))
                StartRenderAsync();
        }

        Complex FromPoint(Point p)
        {
            double increment = scale / viewWidth;
            Point centerPixel = new Point(viewWidth / 2, viewHeight / 2);
            double xdelta = p.X - centerPixel.X;
            double ydelta = p.Y - centerPixel.Y;
            return center + new Complex(xdelta*increment, ydelta*increment);
        }

        void OnReset(object sender, EventArgs e)
        {
            center = new Complex(0, 0);
            scale = 3.0;

            StartRenderAsync();
        }

        private Task StartRenderAsync()
        {
            if (tokenSource != null)
                tokenSource.Cancel();

            tokenSource = new CancellationTokenSource();
            return GenerateFractlAsync(maxIterations, tokenSource.Token);
        }

        private async Task GenerateFractlAsync(int maxIterations, CancellationToken cancelToken)
        {
            Stopwatch sw = Stopwatch.StartNew();

            Mandelbrot mandelbrotGenerator = new Mandelbrot();

            double increment = scale / viewWidth;
            double left = center.Real - (viewWidth / 2) * increment;
            double row = center.Imaginary - (viewHeight / 2) * increment;

            var tasks = PartitionRows(Environment.ProcessorCount)
                .Select(range => Task.Run(() =>
                    {
                        double startRow = row + (range.Item1 * increment);
                        for (int y = range.Item1; y < range.Item2; y++)
                        {
                            cancelToken.ThrowIfCancellationRequested();
                            mandelbrotGenerator.RenderRow(left, startRow, increment, 
                                maxIterations, colors, memoryBuffer, (int)(y*viewWidth), (int)viewWidth);
                            startRow += increment;
                        }
                    }, cancelToken)).ToArray();

            Task renderTask = null;
            try
            {
                renderTask = Task.WhenAll(tasks);
                await renderTask;

            }
            catch (OperationCanceledException)
            {
                // Canceled -return
                return;
            }
            catch // All exceptions
            {
                // Pull out all exceptions
                AggregateException aex = renderTask.Exception;
                if (aex != null) {
                    string message = string.Join(Environment.NewLine, 
                        aex.Flatten().InnerExceptions.Select(x => x.Message));
                    await DisplayAlert("Render Data", message, "OK");
                }
            }

            RefreshScreen();

            sw.Stop();
            timer.Text = sw.Elapsed.ToString();
        }

        IEnumerable<Tuple<int,int>> PartitionRows(int segments)
        {
            int pos = 0;
            int partitions = (int) Math.Ceiling(viewHeight / (double)segments);
            for (int i = 0; i < segments-1; i++, pos += partitions)
                yield return Tuple.Create(pos, pos+partitions);

            yield return Tuple.Create(pos, (int)viewHeight);
        }

        void RefreshScreen()
        {
            IImageCreator ic = DependencyService.Get<IImageCreator>();
            Task<Stream> t = ic.CreateAsync(memoryBuffer, (int) viewWidth, (int) viewHeight);

            this.imageHost.Source = ImageSource.FromStream(() => t.Result);
        }

        private bool SetImageSize(double width, double height)
        {
            IImageCreator ic = DependencyService.Get<IImageCreator>();
            width *= ic.ScaleFactor;
            height *= ic.ScaleFactor;

            if (viewWidth != width || viewHeight != height || memoryBuffer == null)
            {
                viewWidth = width;
                viewHeight = height;

                memoryBuffer = new int[(int)(viewWidth * viewHeight)];

                if (imageHost != null) {
                    imageHost.Source = null;
                    imageHost.WidthRequest = viewWidth;
                    imageHost.HeightRequest = viewHeight;
                    return true;
                }
            }
            return false;
        }
    }
}

