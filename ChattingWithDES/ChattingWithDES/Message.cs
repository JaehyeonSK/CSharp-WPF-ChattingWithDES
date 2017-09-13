using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChattingWithDES
{
    [Serializable]
    public class Message
    {
        public MessageType Type { get; }
        public string Name { get; set; }
        public byte[] Data { get; set; }

        public Message(MessageType type, string name, byte[] data)
        {
            this.Type = type;
            this.Name = name;
            this.Data = data;
        }

        // 메시지 타입
        // MSG_JOIN: 사용자의 입장을 알리는 메시지
        // MSG_SECURE: DES 알고리즘으로 암호화된 메시지
        // MSG_NORMAL: 암호화되지 않은 일반 메시지
        // MSG_LEAVE: 사용자의 퇴장을 알리는 메시지
        public enum MessageType
        {
            MSG_JOIN, MSG_SECURE, MSG_NORMAL, MSG_LEAVE
        };
    }
}
