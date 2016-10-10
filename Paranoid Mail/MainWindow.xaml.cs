using Paranoid;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Ribbon;
using System.Windows.Input;
using Dapper;



namespace Paranoid_Mail
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : RibbonWindow
	{
		public static Contact CurrentContact;
		public static ObservableCollection<MessageListItem> MainWindowMessagesList;

		public MainWindow()
		{
			if (!Startup.Start())
			{
				Application.Current.Shutdown();
				return;
			}

			App.MakeBindings();

			InitializeComponent();


			if (Utils.GetIntValue("isMaximized", 0) != 0)
			{
				WindowState = WindowState.Maximized;
			}
			else
			{
				Height = (int)Utils.GetIntValue("SizeY", 600);
				Width = (int)Utils.GetIntValue("SizeX", 800);
			}
			MainGrid.ColumnDefinitions[0].Width=new GridLength(Utils.GetIntValue("ContactsAreaWidth",200));

			Startup.GetUnreadCounts();

			Task SendReceiveTask = new Task(BackgroundTasks.PeriodicTask);
			SendReceiveTask.Start();

		}

		private void onSizeChangedEvent(object sender, SizeChangedEventArgs e)
		{

			if (e.WidthChanged) Utils.UpdateIntValue("SizeX", Convert.ToInt64(e.NewSize.Width));
			if (e.HeightChanged) Utils.UpdateIntValue("SizeY", Convert.ToInt64(e.NewSize.Height));

		}

		private void onStateChangedEvent(object sender, EventArgs e)
		{
			Utils.UpdateIntValue("isMaximized", WindowState == WindowState.Maximized ? 1 : 0);
		}

		private void onContactsAreaWidthChangedEvent(object sender, SizeChangedEventArgs e)
		{

			if (e.WidthChanged) Utils.UpdateIntValue("ContactsAreaWidth", Convert.ToInt64(e.NewSize.Width));
		}


		private void MainWindow_OnContentRendered(object sender, EventArgs e)
		{
			if (CryptoData.Accounts.Count != 0) return;

			if (MessageBox.Show("You have no registered accounts, want create new one?", "Register new account",
					MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
				return;
			RegisterAccount RA = new RegisterAccount();
			RA.ShowDialog();

		}



		private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{

			if (FoldersTreeView.SelectedItem.GetType() == typeof (Contact))
			{
				Contact Cnt = (Contact)FoldersTreeView.SelectedItem;
				if (Cnt.Status==ContactStatus.OtherSideRequested)
						(new IncomingContactRequestDialog(Cnt)).ShowDialog();


				SetCurrentContact(Cnt);
			}
			else
			{
				ClearCurrentContact();
			}


		}

		private void SetCurrentContact(Contact Cnt)
		{
			CurrentContact = Cnt;
			ContactNameTextBox.Text = Cnt.ContactName;
			ContactNameTextBox.ToolTip = Cnt.ContactAddress;
			using (DB DBC=new DB())
			{

				MainWindowMessagesList = new ObservableCollection<MessageListItem>(
						DBC.Conn.Query<MessageListItem>(
							"Select FromUser,FromServer,MessageID, MessageStatus,RecvTime, length(MessageBody) as MsgDataLen from Messages where MessageType=10 and MessageStatus<>0 and ((FromServer=@AccID and FromUser=@CntID) or (ToServer=@AccID and ToUser=@CntID)) order by RecvTime",
							new {AccID = Cnt.ParentAccount.AccountID, CntID = Cnt.ContactID}));
					MessagesListBox.ItemsSource = MainWindowMessagesList;

				if (MainWindowMessagesList.Count > 1)
				{
					MessagesListBox.SelectedIndex = MessagesListBox.Items.Count - 1;
					MessagesListBox.ScrollIntoView(MessagesListBox.SelectedItem);
				}

			}

		}

		private void ClearCurrentContact()
		{
			CurrentContact = null;
			MainWindowMessagesList = null;
			MessagesListBox.ItemsSource = null;
			ContactNameTextBox.Text = string.Empty;
			ContactNameTextBox.ToolTip = string.Empty;

		}

		private void MessagesListBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			MessageListItem Cnt = ((FrameworkElement)e.OriginalSource).DataContext as MessageListItem;
			if (Cnt != null)
			{
				MessageViewer MV = new MessageViewer(CurrentContact, Cnt);
				MV.Show();
			}
		}

		public static void UpdateMessageStatus(Contact Cnt, long FromUsr,long FromSrv,long MsgID,MsgStatus NewStatus)
		{
			if (Cnt != CurrentContact) return;
			MessageListItem MLI =
				MainWindowMessagesList.FirstOrDefault(
					p => p.FromUser == FromUsr && p.FromServer == FromSrv && p.MessageID == MsgID);
			if (MLI != null) MLI._MsgStatus = NewStatus;
		}

		public static void AddMessageToList(Contact Cnt, MessageListItem MLI)
		{
			if (Cnt != CurrentContact) return;
			Application.Current.Dispatcher.Invoke(() => MainWindowMessagesList.Add(MLI));
		}



		private void MainWindow_OnClosed(object sender, EventArgs e)
		{
			Application.Current.Shutdown();
		}


		private void AboutMenu_OnClick(object sender, RoutedEventArgs e)
		{
			AboutWindow AW=new AboutWindow();
			AW.ShowDialog();
		}

	}
}
