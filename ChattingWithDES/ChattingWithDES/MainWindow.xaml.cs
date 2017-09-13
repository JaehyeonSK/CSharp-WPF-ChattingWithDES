using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using JHClientEngine;
using DESAlgorithm;
using System.Net;

namespace ChattingWithDES
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        TCPClient client = null;
        ChatMain chatMain = new ChatMain();
        
        public MainWindow()
        {
            InitializeComponent();
        }

        // 입장 버튼 클릭
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            IPAddress ipAddr = null;
            int port = 0;

            if (!IPAddress.TryParse(textIP.Text, out ipAddr))
            {
                throw new Exception("잘못된 형식의 IP주소입니다.");
            }

            if (!int.TryParse(textPort.Text, out port))
            {
                throw new Exception("잘못된 포트 번호입니다.");
            }

            client = new TCPClient(ipAddr, port);

            chatMain.Client = client;
            chatMain.Nickname = textNickname.Text;
            this.Hide();
            chatMain.ShowDialog();
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            client?.Close();
        }
    }
}
