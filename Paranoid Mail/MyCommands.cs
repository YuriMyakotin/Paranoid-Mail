using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace Paranoid
{
	public static class MyCommands
	{
		public static readonly RoutedUICommand AppQuitCmd=new RoutedUICommand("Exit","Exit",typeof(MyCommands));

		public static readonly RoutedUICommand AccountsCmd = new RoutedUICommand("Accounts", "Accounts", typeof(MyCommands));
		public static readonly RoutedUICommand EditAccountCmd = new RoutedUICommand("Change Account", "Change Account", typeof(MyCommands));
		public static readonly RoutedUICommand DeleteAccountCmd = new RoutedUICommand("Delete Account", "Delete Account", typeof(MyCommands));

		public static readonly RoutedUICommand ContactsCmd=new RoutedUICommand("Contacts","Contacts",typeof(MyCommands));
		public static readonly RoutedUICommand NewContactCmd = new RoutedUICommand("New Contact", "New Contact", typeof(MyCommands));
		public static readonly RoutedUICommand EditContactCmd = new RoutedUICommand("Edit Contact", "Edit Contact", typeof(MyCommands));
		public static readonly RoutedUICommand DeleteContactCmd = new RoutedUICommand("Delete Contact", "Change Contact", typeof(MyCommands));

		public static readonly RoutedUICommand SendReceiveCmd = new RoutedUICommand("Send & Receive", "Send & Receive", typeof(MyCommands));

		public static readonly RoutedUICommand NewMessageCmd = new RoutedUICommand("New Message", "New Message", typeof(MyCommands));

		public static readonly RoutedUICommand SendMessageCmd = new RoutedUICommand("Send Message", "Send Message", typeof(MyCommands));

		public static readonly RoutedUICommand ReplyMessageCmd = new RoutedUICommand("Send Message", "Send Message", typeof(MyCommands));
		public static readonly RoutedUICommand ForwardMessageCmd = new RoutedUICommand("Send Message", "Send Message", typeof(MyCommands));
		public static readonly RoutedUICommand DeleteMessageCmd = new RoutedUICommand("Send Message", "Send Message", typeof(MyCommands));
		public static readonly RoutedUICommand SaveAllAttachmentsCmd = new RoutedUICommand("Send Message", "Send Message", typeof(MyCommands));

		public static readonly RoutedUICommand OptionsCmd = new RoutedUICommand("Options", "Options", typeof(MyCommands));
		public static readonly RoutedUICommand IDCalculatorCmd = new RoutedUICommand("ID Calculator", "ID Calculator", typeof(MyCommands));
		public static readonly RoutedUICommand ChangeMasterPwdCmd = new RoutedUICommand("Change master password", "Change master password", typeof(MyCommands));
		public static readonly RoutedUICommand ResendUndeliveredCmd = new RoutedUICommand("Resend undelivered messages", "Resend undelivered messages", typeof(MyCommands));
	}

}
