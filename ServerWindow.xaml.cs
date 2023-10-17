using System;
using System.Net.Sockets;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Net;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace NetworkProgramming
{
    /// <summary>
    /// Логика взаимодействия для ServerWindow.xaml
    /// </summary>
    public partial class ServerWindow : Window
    {
        private Socket? listenSocket;  // "слухаючий" сокет - очікує запитів
        private IPEndPoint? endPoint;  // точка, як він "слухає"         

        public ServerWindow()
        {
            InitializeComponent();
        }

        private void SwitchServer_Click(object sender, RoutedEventArgs e)
        {
            if(listenSocket == null)
            {
                try
                {
                    IPAddress ip = IPAddress.Parse(HostTextBox.Text);
                    int port = Convert.ToInt32(PortTextBox.Text);
                    endPoint = new(ip, port);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(
                        "Неправильні параметри конфігурації: "
                        + ex.Message);
                    return;
                }
                listenSocket = new(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp
                    );

                new Thread(StartServer).Start();

                SwitchServer.Background = new SolidColorBrush(Colors.Red);
                SwitchServer.Content = "Вимкнути";

                StatusLabel.Background = new SolidColorBrush(Colors.Green);
                StatusLabel.Content = "Увімкнено";
            }
            else
            {
                listenSocket.Close();

                SwitchServer.Background = new SolidColorBrush(Colors.Green);
                SwitchServer.Content = "Увiмкнути";

                StatusLabel.Background = new SolidColorBrush(Colors.Red);
                StatusLabel.Content = "Вимкнено";
            }
        }

        private void StartServer()
        {
            if (listenSocket == null || endPoint == null)
            {
                MessageBox.Show("Спроба запуску без ініціалізації даних ");
                return;
            }

            try
            {
                listenSocket.Bind(endPoint);
                listenSocket.Listen(10);
                Dispatcher.Invoke(() => ServerLog.Text += "Сервер запущен\n");

                byte[] buffer = new byte[1024];
                while (true)
                {
                    Socket socket = listenSocket.Accept();
                    MemoryStream memoryStream = new();
                    do
                    {
                        int n = socket.Receive(buffer);
                        memoryStream.Write(buffer, 0, n);
                    } while (socket.Available > 0);
                    String str = Encoding.UTF8.GetString(memoryStream.ToArray());
                    Dispatcher.Invoke(() => ServerLog.Text += $"{DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                listenSocket = null;
                Dispatcher.Invoke(() => ServerLog.Text += "Сервер зупинено\n");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listenSocket?.Close();
        }
    }
}
