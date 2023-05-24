using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using HuionPadLib;
using System.Drawing;
using HuionSignature.Utils;
using System.IO;

namespace HuionSignature
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private HuionPadLib.HuionPad hPad = new HuionPad();
        List<HuionPadLib.PointPressure> points = new List<HuionPadLib.PointPressure>();
        private bool captureRunning = true;
        private bool deviceConnected = false;

        Bitmap bmp = new Bitmap(500, 250);
        public MainWindow()
        {
            InitializeComponent();
            bmp.SetResolution(300, 300);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            connectDevice();
        }
        
        private void btnDisconnectDevice_Click(object sender, RoutedEventArgs e)
        {
            disconnectDevice();
        }

        private void btnRestartDevice_Click(object sender, RoutedEventArgs e)
        {
            connectDevice();
        }

        private void btnStartSignature(object sender, RoutedEventArgs e)
        {
            if (deviceConnected == false)
            {
                MessageBox.Show("Pad de assinatura não está conectado!");
                return;
            }

            clearSignature();

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.FillRectangle(Brushes.White, 0, 0, bmp.Width, bmp.Height);
            }

            btnStart.IsEnabled = false;
            btnSave.IsEnabled = false;
            btnDisconnectDevice.IsEnabled = false;
            btnStop.IsEnabled = true;
            btnClear.IsEnabled = false;

            hPad.startCapture();

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (this.captureRunning == false)
                        break;

                    HuionPadLib.PointPressure[] pointsData = hPad.getCaptureData();
                    
                    if (pointsData.Length == 0)
                        //drawingSignature();
                        continue;
                    else
                    {
                        //this.points.AddRange(pointsData);
                        drawingSignature(pointsData);
                    }
                }

                this.captureRunning = true;
            });
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            clearSignature();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            bmp = Util.ToGrayscale(bmp);

            bmp.Save($"{Guid.NewGuid().ToString()}-output.png", System.Drawing.Imaging.ImageFormat.Png);
            byte[] imageToByteArray = ImageToByteArray(bmp);
            MessageBox.Show("Imagem salva!!");
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            btnClear.IsEnabled = true;
            btnSave.IsEnabled=true;
            btnStart.IsEnabled = true;
            btnDisconnectDevice.IsEnabled = true;
            btnStop.IsEnabled = false;
            captureRunning = false;
            hPad.stopCapture();

            imageSignature.Source = Util.ToBitmapImage(bmp);
        }

        private void clearSignature()
        {
            try
            {
                btnClear.IsEnabled = false;
                btnSave.IsEnabled = false;
                points.Clear();
                bmp = new Bitmap(500, 250);
                bmp.SetResolution(300, 300);
                this.imageSignature.Source = null;
                hPad.clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void disconnectDevice()
        {
            try
            {
                hPad.disconnect();
                deviceConnected = false;
                btnRestartDevice.IsEnabled = true;
                btnDisconnectDevice.IsEnabled = false;
                btnStart.IsEnabled = false;
                btnSave.IsEnabled = false;
                btnStop.IsEnabled = false;
                btnClear.IsEnabled = false;
                MessageBox.Show("Pad de assinatura desconectado com sucesso!");
            }
            catch
            {
                MessageBox.Show("Pad de assinatura não está conectado!");
            }
        }

        private void connectDevice()
        {
            try
            {
                hPad.connect();
                deviceConnected = true;
                btnDisconnectDevice.IsEnabled = true;
                btnStart.IsEnabled = true;
                btnRestartDevice.IsEnabled = false;
                MessageBox.Show("Pad de assinatura conectado com sucesso!");
            }
            catch
            {
                MessageBox.Show("Pad de assinatura não está conectado!");
            }
        }

        private void drawingSignature(HuionPadLib.PointPressure[] points)
        {
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Pen pen = new Pen(Color.Black, 2);

                for (int i = 0; i < points.Length - 1; i++)
                {
                    if (i != points.Length - 1)
                    {
                        float newX = (float)Math.Round((points[i].getX() * 499f) / 7999f, 0);
                        float newY = (float)Math.Round((points[i].getY() * 249f) / 7999f, 0);

                        float newXNext = (float)Math.Round((points[i + 1].getX() * 499f) / 7999f, 0);
                        float newYNext = (float)Math.Round((points[i + 1].getY() * 249f) / 7999f, 0);

                        g.DrawLine(pen, newX, newY, newXNext, newYNext);

                        
                    }
                }

                Dispatcher.Invoke(() =>
                {
                    this.imageSignature.Source = Util.ToBitmapImage(bmp);
                });

                this.points.Clear();
            }
        }

        private byte[] ImageToByteArray(Bitmap bitmap)
        {
            MemoryStream stream = new MemoryStream();

            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);

            byte[] bytes = stream.ToArray();

            stream.Dispose();

            return bytes;
        }

    }
}
