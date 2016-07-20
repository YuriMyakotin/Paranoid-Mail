using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Dapper;

using SevenZip.Compression.LZMA;
using System.IO;
using HashLib.Crypto.SHA3;
using Paranoid_Mail;
#pragma warning disable 1998

namespace Paranoid
{
	/// <summary>
	/// Interaction logic for MessageViewer.xaml
	/// </summary>
	public partial class MessageViewer : Window
	{
		private readonly MessageListItem MsgInfo;
		private MailMessageBody MsgData;
		private readonly Contact CntInfo;

		public MessageViewer(Contact Cnt, MessageListItem MLI)
		{

			MsgInfo = MLI;
			CntInfo = Cnt;
			InitializeComponent();
			if (!MsgInfo.isReceived)
			{
				_btnReply.Visibility=Visibility.Collapsed;
			}
		}


		private void onSizeChangedEvent(object sender, SizeChangedEventArgs e)
		{

			if (e.WidthChanged) Utils.UpdateIntValue("MailViewerSizeX", Convert.ToInt64(e.NewSize.Width));
			if (e.HeightChanged) Utils.UpdateIntValue("MailViewerSizeY", Convert.ToInt64(e.NewSize.Height));

		}

		private void onStateChangedEvent(object sender, EventArgs e)
		{
			Utils.UpdateIntValue("isMailViewerMaximized", this.WindowState == WindowState.Maximized ? 1 : 0);
		}

		private async void MessageViewer_Loaded(object sender, EventArgs e)
		{
			Mouse.OverrideCursor = Cursors.Wait;
			bool result = await LoadMessageBody();
			if (!result)
			{
				Close();
				return;
			}
			SubjectTextBox.Text = MsgData.Subject;
			DateTime dt = Utils.LongToDateTime(MsgData.SendTime);
			TimeTextBox.Text = dt.ToShortDateString() + " " + dt.ToShortTimeString();

			Title = MsgData.Subject.Length == 0 ? "Untitled - " : MsgData.Subject + " - ";
			Title += MsgInfo.isReceived ? "from " + CntInfo.ContactName : "to " + CntInfo.ContactName;

			TextRange t = new TextRange(richTextBox.Document.ContentStart,
								   richTextBox.Document.ContentEnd);


			if (MsgData.TextFormat == MessageTextFormat.RTF)
				using (MemoryStream MS = new MemoryStream(Encoding.Default.GetBytes(MsgData.MessageText)))
				{
					t.Load(MS, DataFormats.Rtf);
				}
			else
				t.Text = MsgData.MessageText;
			if (MsgData.FileAttachments.Count == 0)
			{
				AttachmentsGrid.Visibility = Visibility.Collapsed;
			}
			else
			{
				AttachmentsGrid.Visibility = Visibility.Visible;
				AttachmentsListView.ItemsSource = MsgData.FileAttachments;
				_btnSaveAll.Visibility = Visibility.Visible;
			}
			Mouse.OverrideCursor = Cursors.Arrow;

			if ((MsgStatus) MsgInfo.MessageStatus == MsgStatus.Received)
			{
				using (var DBC=new DB())
				{

					DBC.Conn.Execute(
						"Update Messages Set MessageStatus=2 where FromUser=@FromUser and FromServer=@FromServer and MessageID=@MessageId",
						MsgInfo);
				}
				MainWindow.UpdateMessageStatus(CntInfo,MsgInfo.FromUser,MsgInfo.FromServer,MsgInfo.MessageID,MsgStatus.Readed);
				CntInfo.UnreadMessages -= 1;
				if (CntInfo.UnreadMessages < 0) CntInfo.UnreadMessages = 0;
			}
		}

		private async Task<bool> LoadMessageBody()
		{
			if (MsgInfo.MessageStatus == (int)MsgStatus.Bad)
				return false;

			using (var DBC=new DB())
			{

				byte[] MsgBody = DBC.Conn.QueryFirstOrDefault<byte[]>(
					"Select MessageBody from Messages where MessageType=10 and FromUser=@FromUser and FromServer=@FromServer and MessageID=@MessageId",
					MsgInfo);
				if (MsgBody == null) return false;



				MsgData =
					Utils.BytesToObject<MailMessageBody>(
						SevenZipHelper.Decompress(ParanoidHelpers.DecryptMessage(CntInfo.StorageKey, MsgBody)));

				if (MsgInfo.isReceived)
						foreach (Attachment Att in MsgData.FileAttachments)
						{
							foreach (long FilePartID in Att.FileParts)
							{
								if (
									DBC.Conn.QueryFirstOrDefault<int>(
										"Select count(*) from Messages where MessageType=11 and FromUser=@FromUsr and FromServer=@FromSrv and MessageID=@MessageID",
										new { FromUsr = MsgInfo.FromUser, FromSrv = MsgInfo.FromServer, MessageID = CntInfo.TranslateMsgID(FilePartID)}) == 0)
								{
									Att.isAllPartsReceived = false;
									break;
								}

							}
							if (Att.isAllPartsReceived)
							{
								foreach (long FilePartID in Att.FileParts)
								{
									DBC.Conn.Execute(
										"Update Messages set MessageStatus=2 where MessageType=11 and FromUser=@FromUsr and FromServer=@FromSrv and MessageID=@MessageID ",
										new {
											FromUsr = MsgInfo.FromUser,
											FromSrv = MsgInfo.FromServer,
											MessageID = CntInfo.TranslateMsgID(FilePartID) });
								}
							}

						}
			}
			return MsgData != null;
		}

		private void ReplyCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = MsgInfo.isReceived;
		}


		private void AlwaysCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void SaveAttachmentsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if ((MsgData != null) && (MsgData.FileAttachments.Count != 0))
			{
				e.CanExecute = MsgData.FileAttachments.Count(p => p.isAllPartsReceived) != 0;
			}
			else
			{
				e.CanExecute = false;
			}
		}

		private void ReplyCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			MessageEditor ME = new MessageEditor(MessageEditorStartMode.ReplyMode, CntInfo, MsgData, MsgInfo);
			ME.Show();
			Close();
		}

		private void ForwardCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			MessageEditor ME = new MessageEditor(MessageEditorStartMode.ForwardMode, CntInfo, MsgData,MsgInfo);
			ME.Show();
		}


		private void DeleteCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{

			if (MessageBox.Show("Message will be permanently deleted, continue?", "Delete message", MessageBoxButton.YesNo,
				MessageBoxImage.Exclamation, MessageBoxResult.No)!=MessageBoxResult.Yes) return;
			Mouse.OverrideCursor = Cursors.Wait;
			using (var DBC=new DB())
			{

				foreach (Attachment Att in MsgData.FileAttachments)
					foreach (long FilePartID in Att.FileParts)
					{
						long MsgID = MsgInfo.isReceived ? CntInfo.TranslateMsgID(FilePartID) : FilePartID;
						DBC.Conn.Execute("Delete from Messages where MessageType=11 and FromUser=@FromUsr and FromServer=@FromSrv and MessageID=@MessageID",
								new { FromUsr = MsgInfo.FromUser, FromSrv = MsgInfo.FromServer, MessageID = MsgID });
					}

				DBC.Conn.Execute(
						"Delete from Messages where MessageType=10 and FromUser=@FromUser and FromServer=@FromServer and MessageID=@MessageId",
						MsgInfo);
				if (DB.DatabaseType == DBType.SQLite) DBC.Conn.Execute("Vacuum");
			}
			Close();
			if (CntInfo == MainWindow.CurrentContact)
				MainWindow.MainWindowMessagesList.Remove(MsgInfo);
			Mouse.OverrideCursor = Cursors.Arrow;
		}


		private void SaveAllAttachmentCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog
			{
				Description = "Select folder for saving all attachments",
				ShowNewFolderButton = true
			};

			if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				string SelectedFolder = dlg.SelectedPath;
				YesNoToAll lastReply = YesNoToAll.No;
				bool isMultiple = MsgData.FileAttachments.Count(p => p.isAllPartsReceived) > 1;
				foreach (Attachment Att in MsgData.FileAttachments)
				{
					string FullFileName = Path.Combine(SelectedFolder, Att.FileName);
					if (File.Exists(FullFileName))
					{
						switch (lastReply)
						{
							case YesNoToAll.NoToAll:
								continue;
							case YesNoToAll.YesToAll:
								WriteAttachment(Att,FullFileName);
								break;
							default:
								try
								{
									FileInfo FI = new FileInfo(FullFileName);

									ExistingFileReplaceDialog FRD =
										new ExistingFileReplaceDialog(
											FullFileName + " ("+Utils.SizeToStr(FI.Length)+")",
											Att.FileName+ Att.FileSizeStr, isMultiple);
									FRD.ShowDialog();
									lastReply = FRD.Result;
									if ((lastReply == YesNoToAll.Yes) || (lastReply == YesNoToAll.YesToAll))
									{
										WriteAttachment(Att, FullFileName);
									}
								}
								catch (Exception Ex)
								{
									MessageBox.Show(Ex.Message);
								}
								break;
						}
					}
					else
					{
						WriteAttachment(Att, FullFileName);
					}
				}

			}


		}


		private void AttachmentsListView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			Attachment Att = ((FrameworkElement)e.OriginalSource).DataContext as Attachment;
			if ((Att != null)&&(Att.isAllPartsReceived))
				SaveAttachment(Att);
		}


		private void SaveAttachment(Attachment Att)
		{
			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
			{
				FileName = Att.FileName,
				Filter = "All files (*.*)|*.*"
			};
			bool? result = dlg.ShowDialog();
			if (result != true) return;

			WriteAttachment(Att, dlg.FileName);


		}

		private void WriteAttachment(Attachment Att, string FileName)
		{
			try
			{
				using (FileStream FS = new FileStream(FileName, FileMode.Create))
				{
					if (!WriteFileData(Att, FS))
					{
						MessageBox.Show("File saving error: received data part damaged or missing");
						FS.Close();
						File.Delete(FileName);
					};
					FS.Close();
				}
			}
			catch (Exception Ex)
			{
				MessageBox.Show("File saving error: " + Ex.Message);
			}
		}

		private bool WriteFileData(Attachment Att, FileStream FS)
		{
			Skein256 SK=new Skein256();
			using (var DBC=new DB())
			{

				foreach (long FilePartID in Att.FileParts)
				{
					long MsgID = MsgInfo.isReceived ? CntInfo.TranslateMsgID(FilePartID) : FilePartID;
					byte[] FilePartBytes =
						DBC.Conn.QuerySingleOrDefault<byte[]>("Select MessageBody from Messages where MessageType=11 and FromUser=@FromUsr and FromServer=@FromSrv and MessageID=@MessageID",
							new { FromUsr=MsgInfo.FromUser, FromSrv=MsgInfo.FromServer, MessageID=MsgID });

					if (FilePartBytes == null) return false;

					byte[] DecodedBytes =
						ParanoidHelpers.DecryptMessage(CntInfo.StorageKey,FilePartBytes);
					if (DecodedBytes == null) return false;
					switch (Att.Compression)
					{
						case FileCompression.LZMA:
								DecodedBytes = SevenZipHelper.Decompress(DecodedBytes);
								if (DecodedBytes == null) return false;
								break;
					}
					FS.Write(DecodedBytes, 0, DecodedBytes.Length);
					SK.TransformBytes(DecodedBytes);

				}
			}

			byte[] NewHash = (SK.TransformFinal()).GetBytes();
			SK.Initialize();

			return Chaos.NaCl.CryptoBytes.ConstantTimeEquals(NewHash,Att.Hash);
		}

	}
}
