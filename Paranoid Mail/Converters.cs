using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Paranoid
{
    public class IsNotZeroToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((int)value == 0) ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }

    public class ContactStatusToImgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ContactStatus) value)
            {
                case ContactStatus.InfoRequested:
                    return Application.Current.Resources["InfoRequestedIcon"];
                case ContactStatus.Estabilished:
                    return Application.Current.Resources["EstabilishedContactIcon"];
                case ContactStatus.UserNotFound:
                    return Application.Current.Resources["UserNotFoundIcon"];
                case ContactStatus.Blocked:
                    return Application.Current.Resources["BlockedContactIcon"];
                case ContactStatus.RequestSent:
                    return Application.Current.Resources["WaitingResponseIcon"];
                case ContactStatus.RequestRejected:
                    return Application.Current.Resources["OtherSideRefusedIcon"];
                case ContactStatus.OtherSideRequested:
                    return Application.Current.Resources["IncomingContactRequestIcon"];
                default:
                    return null;
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }


    public class ContactStatusToStrConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Contact.StatusToStr((ContactStatus) value);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }

    public class AccountSessionStatusToImgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((NetSessionResult) value)
            {
                case NetSessionResult.Ok:
                    return null;
                case NetSessionResult.Busy:
                    return Application.Current.Resources["SyncIcon"];
                default: return Application.Current.Resources["SyncErrorIcon"];
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }

    public class AccountSessionStatusToStrConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((NetSessionResult)value)
            {
                case NetSessionResult.Ok:
                    return string.Empty;
                case NetSessionResult.Busy:
                    return "Send & receive in progress";
                case NetSessionResult.CantConnect:
                    return "Can't connect to server";
                case NetSessionResult.InvalidCredentials:
                    return "Security error";
                case NetSessionResult.InvalidData:
                    return "Data error";
                case NetSessionResult.NetError:
                    return "Network error";


                default: return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }

    public class MessageStatusToImgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((MsgStatus)value)
            {
                case MsgStatus.Outbox:
                    return Application.Current.Resources["OutboxMessageIcon"];
                case MsgStatus.Rejected:
                    return Application.Current.Resources["RejectedMessageIcon"];
                case MsgStatus.Sent:
                    return Application.Current.Resources["SentMessageIcon"];
                case MsgStatus.Delivered:
                    return Application.Current.Resources["DeliveredMessageIcon"];
                case MsgStatus.Received:
                    return Application.Current.Resources["UnreadMessageIcon"];
                case MsgStatus.Readed:
                    return Application.Current.Resources["ReadedMessageIcon"];
                default:
                    return null;
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }

    public class MessageStatusToStrConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((MsgStatus)value)
            {
                case MsgStatus.Outbox:
                    return "Outbox";
                case MsgStatus.Rejected:
                    return "Rejected by server";
                case MsgStatus.Sent:
                    return "Sent";
                case MsgStatus.Delivered:
                    return "Delivered";
                case MsgStatus.Received:
                    return "Received";
                case MsgStatus.Readed:
                    return "Readed";
                default:
                    return null;
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }


    public class MessageStatusToIsUnreadConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (MsgStatus) value == MsgStatus.Received;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }

    public class MessageStatusToIsOutboxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (MsgStatus)value == MsgStatus.Outbox;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }
}