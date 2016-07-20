using System;
using System.Linq;
using System.Reflection;
using Paranoid;
using System.Windows;
using System.Threading;
using System.Windows.Input;
using Dapper;
using static Paranoid.LongTime;
using static Paranoid.Utils;

namespace Paranoid_Mail
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private readonly EventWaitHandle Evnt;
		public App()
		{

			ulong ID = Str2Hash.StringToHash(System.Reflection.Assembly.GetEntryAssembly().Location);
			string EventName = "ParanoidMailClient_" + ID.ToString();


			if (EventWaitHandle.TryOpenExisting(EventName,out Evnt))
			{
				MessageBox.Show("Another running copy detected, exiting");
				Evnt.Close();
				return;
			}
			Evnt = new EventWaitHandle(false, EventResetMode.ManualReset, EventName);

			DB.Init();
		}

		public static void MakeBindings()
		{
			CommandBinding binding = new CommandBinding(MyCommands.AppQuitCmd, AppQuitCmdExecute, AlwaysCanExecute);
			CommandManager.RegisterClassCommandBinding(typeof (Window), binding);

			binding = new CommandBinding(MyCommands.AccountsCmd, AccountsCmdExecute, AlwaysCanExecute);
			CommandManager.RegisterClassCommandBinding(typeof (Window), binding);
			binding = new CommandBinding(MyCommands.EditAccountCmd, EditAccountCmdExecute, isAccountsExistsCanExecute);
			CommandManager.RegisterClassCommandBinding(typeof (Window), binding);
			binding = new CommandBinding(MyCommands.DeleteAccountCmd, DeleteAccountCmdExecute,
				isAccountsExistsCanExecute);
			CommandManager.RegisterClassCommandBinding(typeof (Window), binding);

			binding = new CommandBinding(MyCommands.ContactsCmd, ContactsCmdExecute, isAccountsExistsCanExecute);
			CommandManager.RegisterClassCommandBinding(typeof (Window), binding);

			binding = new CommandBinding(MyCommands.NewContactCmd, NewContactCmdExecute, isAccountsExistsCanExecute);
			CommandManager.RegisterClassCommandBinding(typeof (Window), binding);
			binding = new CommandBinding(MyCommands.EditContactCmd, EditContactCmdExecute, isAccountsExistsCanExecute);
			CommandManager.RegisterClassCommandBinding(typeof (Window), binding);
			binding = new CommandBinding(MyCommands.DeleteContactCmd, DeleteContactCmdExecute,
				isAccountsExistsCanExecute);
			CommandManager.RegisterClassCommandBinding(typeof (Window), binding);

			binding = new CommandBinding(MyCommands.SendReceiveCmd, SendReceiveCmdExecute, isAccountsExistsCanExecute);
			CommandManager.RegisterClassCommandBinding(typeof (Window), binding);

			binding = new CommandBinding(MyCommands.NewMessageCmd, NewMessageCmdExecute,
				isEstabilishedContactsExistsCanExecute);
			CommandManager.RegisterClassCommandBinding(typeof (Window), binding);

			binding = new CommandBinding(MyCommands.IDCalculatorCmd, IDCalculatorCmdExecute, AlwaysCanExecute);
			CommandManager.RegisterClassCommandBinding(typeof (Window), binding);

			binding = new CommandBinding(MyCommands.ChangeMasterPwdCmd, ChangeMasterPwdExecute, AlwaysCanExecute);
			CommandManager.RegisterClassCommandBinding(typeof(Window), binding);

			binding = new CommandBinding(MyCommands.OptionsCmd, OptionsExecute, AlwaysCanExecute);
			CommandManager.RegisterClassCommandBinding(typeof(Window), binding);

			binding = new CommandBinding(MyCommands.ResendUndeliveredCmd, ResendUndeliveredExecute, isAccountsExistsCanExecute);
			CommandManager.RegisterClassCommandBinding(typeof(Window), binding);
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{
			BackgroundTasks.isExit = true;
			CryptoData.Clear();
			Evnt.Close();
		}


		private static void AppQuitCmdExecute(object sender, ExecutedRoutedEventArgs e)
		{
			BackgroundTasks.isExit = true;
			Application.Current.Shutdown();
		}

		private static void AlwaysCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private static void isAccountsExistsCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = CryptoData.Accounts.Count != 0;
		}

		private static void isEstabilishedContactsExistsCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute =
				CryptoData.Accounts.Count(p => p.Contacts.Count(q => q.Status == ContactStatus.Estabilished) != 0) != 0;
			e.Handled = true;
		}


		private static void EditAccountCmdExecute(object sender, ExecutedRoutedEventArgs e)
		{
			Account Acc = (Account) e.Parameter;
			if (Acc == null) return;
			(new EditAccountDialog(Acc)).ShowDialog();

		}

		private static void DeleteAccountCmdExecute(object sender, ExecutedRoutedEventArgs e)
		{
			Account Acc = (Account) e.Parameter;
			if (Acc == null) return;
			DeleteAccountDialog Dialog = new DeleteAccountDialog(Acc.AccountName);
			bool? result = Dialog.ShowDialog();
			if ((result == null) || (result == false)) return;
			Acc.RequestAccountDeletion();


		}

		private static void AccountsCmdExecute(object sender, ExecutedRoutedEventArgs e)
		{
			(new AccountsWindow()).Show();
		}

		private static void ContactsCmdExecute(object sender, ExecutedRoutedEventArgs e)
		{
			Account Acc = (Account) e.Parameter;
			if (Acc == null && Paranoid_Mail.MainWindow.CurrentContact != null)
				Acc = Paranoid_Mail.MainWindow.CurrentContact.ParentAccount;
			(new ContactsDialog(Acc)).Show();
		}

		private static void NewContactCmdExecute(object sender, ExecutedRoutedEventArgs e)
		{
			(new NewContactDialog((Account) e.Parameter)).ShowDialog();

		}

		private static void DeleteContactCmdExecute(object sender, ExecutedRoutedEventArgs e)
		{
			Contact Cnt = (Contact) e.Parameter;
			if (Cnt == null) return;


			DeleteContactDialog Dialog = new DeleteContactDialog(Cnt.ContactName);
			bool? result = Dialog.ShowDialog();
			if ((result == null) || (result == false)) return;
			Cnt.DeleteContact(true);

		}

		private static void EditContactCmdExecute(object sender, ExecutedRoutedEventArgs e)
		{
			Contact Cnt = (Contact) e.Parameter;
			if (Cnt == null) return;
			if (Cnt.Status == ContactStatus.OtherSideRequested)
			{
				(new IncomingContactRequestDialog(Cnt)).ShowDialog();
			}
			else
				(new EditContactDialog(Cnt)).ShowDialog();

		}

		private static void SendReceiveCmdExecute(object sender, ExecutedRoutedEventArgs e)
		{
			Account Acc = (Account) e.Parameter;
			if (Acc == null) BackgroundTasks.SendReceiveAll();
			else BackgroundTasks.SendReceiveAccount(Acc);

		}

		private static void NewMessageCmdExecute(object sender, ExecutedRoutedEventArgs e)
		{
			(new MessageEditor(MessageEditorStartMode.NewMessageMode, (Contact)e.Parameter ?? Paranoid_Mail.MainWindow.CurrentContact)).Show();
		}

		private static void IDCalculatorCmdExecute(object sender, ExecutedRoutedEventArgs e)
		{
			(new IDCalculatorWindow()).Show();
		}

		private static void ChangeMasterPwdExecute(object sender, ExecutedRoutedEventArgs e)
		{
			(new SetPasswordWindow(false)).ShowDialog();
		}

		private static void ResendUndeliveredExecute(object sender, ExecutedRoutedEventArgs e)
		{
			using (DB DBC = new DB())
			{
				DBC.Conn.Execute("Update Messages set MessageStatus=3 where MessageStatus=5 and RecvTime<=@T",
					new {T = Now - Hours(3)});
				BackgroundTasks.SendReceiveAll();
			}
		}

		private static void OptionsExecute(object sender, ExecutedRoutedEventArgs e)
		{
			(new OptionsDialog()).ShowDialog();
		}
	}

}
