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
using System.Windows.Shapes;
using JHClientEngine;
using DESAlgorithm;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ChattingWithDES
{
    /// <summary>
    /// ChatMain.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChatMain : Window
    {
        public TCPClient Client { get; set; } = null;
        public string Nickname { get; set; } = "";

        // DES 알고리즘 객체
        DESAlgorithm.DESAlgorithm des = new DESAlgorithm.DESAlgorithm();

        KeyChange keyChange = new KeyChange();

        // 암호화 및 복호화에 사용되는 서브키 배열
        private long[] subkeys = null;

        public ChatMain()
        {
            InitializeComponent();

            // 서브키에 사용할 키 문자열
            textKey.Text = "Test"; 
            
            // 키 문자열을 이용하여 16개의 서브키를 생성한다
            subkeys = des.GenerateSubkeys(BitConverter.ToInt64(Encoding.Unicode.GetBytes(textKey.Text), 0));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 소켓 연결 실패/성공/메시지받음/연결끊김 이벤트 등록
            Client.ConnectFailed += Client_ConnectFailed;
            Client.ConnectSucceed += Client_ConnectSucceed;
            Client.MessageReceived += Client_MessageReceived;
            Client.Disconnected += Client_Disconnected;

            // TCP 클라이언트 시작
            Client.Start();
        }

        // 소켓 연결이 끊김 콜백 메소드
        private void Client_Disconnected(object sender, MessageEventArgs e)
        {
            MessageBox.Show("연결이 끊어졌습니다.");
            this.Close();
        }

        // 소켓 메시지 받음 콜백 메소드
        private void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            // 서브키가 생성되지 않았을 경우 무시하고 반환한다.
            if (subkeys == null)
            {
                return;
            }

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();

            // 수신한 메시지 바이트 배열(암호화된 문자열)을 메모리 스트림에 쓴다.
            ms.Write(e.Data, 0, e.Data.Length);
            ms.Seek(0, SeekOrigin.Begin);

            // 메모리 스트림에 쓴 바이트 배열을 BinaryFormatter 객체를 통해 역직렬화하여 Message 객체를 구성한다.
            Message msg = (Message)bf.Deserialize(ms);

            ms.Dispose();

            // 메시지 타입에 따른 분기 처리
            switch (msg.Type)
            {
                case Message.MessageType.MSG_JOIN: 
                    // 사용자 입장 메시지일 경우
                    // 메시지의 Data 바이트 배열을 String으로 변환하고 '|'로 구분된 현재 접속중인 사용자 이름들을 가져온다.
                    string[] users = Encoding.Unicode.GetString(msg.Data).Split('|');

                    Dispatcher.Invoke(() =>
                    {
                        // 채팅 리스트에 입장 메시지를 출력한다.
                        listChat.Items.Add(msg.Name + "님이 입장하였습니다.");
                        listMembers.Items.Clear();
                        
                        // 현재 접속중인 사용자를 유저 리스트에 추가한다.
                        foreach (var user in users)
                        {
                            listMembers.Items.Add(user);
                        }
                    });
                    break;
                case Message.MessageType.MSG_NORMAL:
                    // 보통 메시지일 경우
                    // 채팅 리스트에 메시지를 출력한다.
                    Dispatcher.Invoke(() =>
                    {
                        listChat.Items.Add("[" + msg.Name + "]" + Encoding.Unicode.GetString(msg.Data));
                    });
                    break;
                case Message.MessageType.MSG_SECURE:
                    // 보안 메시지일 경우(암호화된 메시지)
                    
                    // 블럭 단위로 나눈다. (1 block = 64bit)
                    long[] blocks = new long[msg.Data.Length / 8];

                    // 각 블럭의 데이터를 64bit 정수형으로 변환한다.
                    for (int i = 0; i < msg.Data.Length / 8; i++)
                    {
                        blocks[i] = BitConverter.ToInt64(msg.Data, i * 8);
                    }

                    // 암호문 블럭들을 DES 알고리즘 객체를 이용하여 복호화하여 평문을 얻는다.
                    var result = des.Decrypt(blocks, subkeys);

                    // 복호화된 메시지를 채팅 리스트에 출력한다.
                    Dispatcher.Invoke(() =>
                    {
                        listChat.Items.Add("[" + msg.Name + "]" + result.ToUnicodeString() + " (" + blocks.ToUnicodeString() + ")");
                    });
                    break;
                case Message.MessageType.MSG_LEAVE:
                    // 사용자 퇴장 메시지일 경우
                    // 전체 접속자 목록을 받아온다.
                    users = Encoding.Unicode.GetString(msg.Data).Split('|');

                    // 채팅 리스트에 퇴장 메시지를 출력한 후 유저 리스트를 갱신한다.
                    Dispatcher.Invoke(() =>
                    {
                        listChat.Items.Add(msg.Name + "님이 퇴장하였습니다.");
                        listMembers.Items.Clear();
                        foreach (var user in users)
                        {
                            listMembers.Items.Add(user);
                        }
                    });
                    break;
                default:
                    // 지정한 형식 이외의 메시지일 경우
                    throw new Exception("알 수 없는 형식의 메시지입니다.");
            }
        }

        // 소켓 연결 성공 콜백 메소드
        private void Client_ConnectSucceed(object sender, MessageEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                listChat.Items.Add("서버에 접속하였습니다.");
            });

            // 메시지를 메모리 스트림에 직렬화한다.
            Message msg = new Message(Message.MessageType.MSG_JOIN, Nickname, null);
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, msg);

            // 메시지를 서버로 전송한다.
            Client.Send(ms.ToArray());

            ms.Dispose();
        }

        // 소켓 연결 실패 콜백 메소드
        private void Client_ConnectFailed(object sender, MessageEventArgs e)
        {
            MessageBox.Show("서버에 연결할 수 없습니다.");
            this.Close();
        }

        // 메시지 전송 버튼 클릭
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            // 서브키가 생성되지 않았을 경우 무시하고 반환한다.
            if (subkeys == null)
            {
                return;
            }

            // 1. 메시지를 암호화한다.
            long[] data = des.Encrypt(textChat.Text, subkeys);

            // 2. 메시지의 블럭들을 저장할 리스트 객체를 생성한다.
            List<byte> list = new List<byte>();

            // 3. 암호화된 메시지의 각 블럭들을 리스트에 추가한다.
            foreach (long i in data)
            {
                list.AddRange(BitConverter.GetBytes(i));
            }

            // 4. 보안 타입의 메시지 객체를 생성한다.
            Message msg = new Message(Message.MessageType.MSG_SECURE, Nickname, list.ToArray());
            list.Clear();

            // 5. 메시지 객체를 메모리 스트림에 직렬화 한다.
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, msg);

            // 6. 직렬화된 메시지 객체를 서버로 전송한다.
            Client.Send(ms.ToArray());
            ms.Dispose();

            Dispatcher.Invoke(() =>
            {
                textChat.Text = "";
            });

        }

        private void textChat_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnSend_Click(null, null);
            }
        }

        // 키 변경 버튼 클릭
        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            keyChange.textKey.Text = textKey.Text;
            keyChange.ShowDialog();

            textKey.Text = keyChange.textKey.Text;
            subkeys = keyChange.Subkeys;
        }
    }
}
