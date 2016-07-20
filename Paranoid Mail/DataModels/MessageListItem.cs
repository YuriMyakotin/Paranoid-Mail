using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paranoid
{
    public class MessageListItem : INotifyPropertyChanged
    {
        public long FromUser { get; set; }
        public long FromServer { get; set; }
        public long MessageID { get; set; }
        public int MessageStatus { get; set; }
        public long RecvTime { get; set; }
        public int MsgDataLen { get; set; }

        public MessageListItem(Message Msg)
        {
            FromUser = Msg.FromUser;
            FromServer = Msg.FromServer;
            MessageID = Msg.MessageID;
            RecvTime = Msg.RecvTime;
            MsgDataLen = Msg.MessageBody.Length;
            MessageStatus = Msg.MessageStatus;
        }

        public MessageListItem()
        {

        }


        public string InfoStr => LongTime.ToLocalTimeStr(RecvTime)+"  "+ Utils.SizeToStr(MsgDataLen);

        public bool isReceived
        {
            get
            {
                switch ((MsgStatus) MessageStatus)
                {
                    case MsgStatus.Delivered:
                    case MsgStatus.Outbox:
                    case MsgStatus.Sent:
                    case MsgStatus.Rejected:
                        return false;

                    default:
                        return true;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public MsgStatus _MsgStatus
        {
            get { return (MsgStatus) MessageStatus; }
            set
            {
                MessageStatus = (int) value;
                OnPropertyChanged("_MsgStatus");
            }
        }
    }
}
