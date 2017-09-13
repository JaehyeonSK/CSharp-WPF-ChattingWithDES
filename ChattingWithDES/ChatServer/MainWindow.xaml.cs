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
using JHServerEngine;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using ChattingWithDES;

namespace ChatServer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        TCPServer server = null;

        // 접속 중인 사용자를 저장하기 위한 객체
        List<Tuple<EndPoint, string>> users = new List<Tuple<EndPoint, string>>();

        public MainWindow()
        {
            InitializeComponent();

            // 시스템의 IP Address를 가져온다.
            var entry = Dns.GetHostEntry(IPAddress.Parse("127.0.0.1"));
            foreach (var e in entry.AddressList)
            {
                if (e.AddressFamily == AddressFamily.InterNetwork)
                {
                    comboIP.Items.Add(e.ToString());
                }
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((string)btnStart.Content == "Start")
                {
                    int port = 0;
                    if (!int.TryParse(textPort.Text, out port))
                    {
                        throw new Exception("잘못된 포트 번호입니다.");
                    }

                    // TCPServer 객체 생성
                    server = new TCPServer(port);

                    // 이벤트 핸들러 등록
                    server.NewClientConnected += Server_NewClientConnected;
                    server.ClientDisconneted += Server_ClientDisconneted;
                    server.MessageReceived += Server_MessageReceived;

                    // 서버 실행 (대기 큐의 크기 10)
                    server.Start(10);

                    btnStart.Content = "Stop";
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        server.Close();
                        btnStart.Content = "Start";
                    });
                }
            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.SocketErrorCode.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // 메시지 받음 콜백 메소드
        private void Server_MessageReceived(object sender, MessageEventArgs e)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();

            ms.Write(e.Data, 0, e.Data.Length);
            ms.Seek(0, SeekOrigin.Begin);
            
            // 메시지의 종류를 확인하기 위해 역직렬화한다.
            var msg = bf.Deserialize(ms) as Message;
            ms.Dispose();

            // 메시지 타입에 따른 분기
            // 사용자 입장 메시지일 경우
            if (msg.Type == Message.MessageType.MSG_JOIN)
            {
                // 유저 리스트에 새로운 유저를 등록한다.
                users.Add(new Tuple<EndPoint, string>(e.RemoteEndPoint, msg.Name));

                // '|' 문자로 구분하여 모든 유저를 하나의 문자열로 합친다.
                string userlist = "";
                for (int i = 0; i < users.Count; i++)
                {
                    userlist += users[i].Item2 + "|";
                }

                // 사용자 입장 메시지 객체를 생성한다.
                Message joinMsg = new Message(Message.MessageType.MSG_JOIN, msg.Name, Encoding.Unicode.GetBytes(userlist));
                
                // 메모리 스트림에 메시지 객체를 직렬화한다.
                ms = new MemoryStream();
                bf.Serialize(ms, joinMsg);

                // 직렬화된 메시지 객체를 접속 중인 모든 사용자에게 전송한다.
                server.SendToAll(ms.ToArray());
                ms.Dispose();
            }
            else // 그 외의 메시지
            {
                // 접속 중인 모든 사용자에게 메시지 객체를 전달한다.
                server.SendToAll(e.Data);
            }
        }

        // 사용자 연결 끊김 콜백 메소드
        private void Server_ClientDisconneted(object sender, MessageEventArgs e)
        {
            // 연결이 끊긴 사용자를 유저 리스트에서 제거한다.
            string name = "";
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Item1 == e.RemoteEndPoint)
                {
                    name = users[i].Item2;
                    users.RemoveAt(i);
                    break;
                }
            }

            if(name == "")
            {
                return;
            }

            // '|' 문자로 구분하여 접속 중인 모든 유저를 하나의 문자열로 합친다.
            string userlist = "";
            for (int i = 0; i < users.Count; i++)
            {
                userlist += users[i].Item2 + "|";
            }
            
            // 사용자 퇴장 메시지 객체를 생성한다.
            Message msg = new Message(Message.MessageType.MSG_LEAVE, name, Encoding.Unicode.GetBytes(userlist));
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();

            // 메모리 스트림에 메시지 객체를 직렬화한다.
            bf.Serialize(ms, msg);

            // 직렬화된 메시지 객체를 접속 중인 모든 사용자에게 전송한다.
            server.SendToAll(ms.ToArray());
            ms.Dispose();
        }

        private void Server_NewClientConnected(object sender, MessageEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(server != null)
            {
                server.Close();
            }
        }
    }
}
