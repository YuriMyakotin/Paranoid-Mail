using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Reflection;
using System.Windows.Controls.Primitives;
using Dapper;
using HashLib.Crypto.SHA3;
using Paranoid_Mail;
using SevenZip.Compression.LZMA;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;

namespace Paranoid
{
    /// <summary>
    /// Interaction logic for MessageEditor.xaml
    /// </summary>
    public partial class MessageEditor : Window
    {
        private readonly ObservableCollection<Contact> ToContacts=new ObservableCollection<Contact>();

        private readonly ObservableCollection<Attachment> FileAttachments=new ObservableCollection<Attachment>();

        private readonly MessageListItem OriginalMsgInfo;

        private MessageTextFormat MsgTextFormat;


        private readonly Contact OriginalContact;



        public MessageEditor(MessageEditorStartMode EditorStartMode=MessageEditorStartMode.NewMessageMode, Contact ToContact=null,MailMessageBody OriginalMsg=null,MessageListItem OriginalMessageInfo=null)
        {

            //if reply or forward - ToContact mean original message contact and must be set

            MessageEditorStartMode StartMode=EditorStartMode;

            InitializeComponent();
            Loaded += MainWindow_Loaded;
            ToListView.ItemsSource = ToContacts;

            if ((ToContact!=null)&&(StartMode!=MessageEditorStartMode.ForwardMode)) ToContacts.Add(ToContact);

            if (Utils.GetIntValue("isMailEditorMaximized", 0) != 0)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                Height = (int)Utils.GetIntValue("MailEditorSizeY", 600);
                Width = (int)Utils.GetIntValue("MailEditorSizeX", 800);
            }

            OriginalMsgInfo = OriginalMessageInfo;

            if ((OriginalMsg == null)||(ToContact==null)||(OriginalMsgInfo==null)) StartMode = MessageEditorStartMode.NewMessageMode;



            if (StartMode == MessageEditorStartMode.NewMessageMode)
            {
                MsgTextFormat = (MessageTextFormat) Utils.GetIntValue("DefaultMessageTextFormat", 1);
            }
            else
            {
                OriginalContact = ToContact;
                if (OriginalMsg != null)
                {
                    MsgTextFormat = OriginalMsg.TextFormat;
                    DateTime dt = Utils.LongToDateTime(OriginalMsg.SendTime);


                    if (MsgTextFormat == MessageTextFormat.PlainText)
                    {
                        TextRange TR = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                        TR.Text = "\r\n------------------------------ Original Message ------------------------------\rCreated: " +
                        dt.ToShortDateString() + " " + dt.ToShortTimeString() + "\rSubject: " + OriginalMsg.Subject + "\r\n" + OriginalMsg.MessageText;

                    }

                    else
                    {
                        FlowDocument FD = richTextBox.Document;
                        Paragraph P=new Paragraph();
                        FD.Blocks.Add(P);

                        P = new Paragraph();
                        FD.Blocks.Add(P);

                        P = new Paragraph();
                        P.Inlines.Add("------------------------------Original Message------------------------------");
                        FD.Blocks.Add(P);

                        P = new Paragraph();
                        P.Inlines.Add("Created: " +dt.ToShortDateString() + " " + dt.ToShortTimeString());
                        FD.Blocks.Add(P);

                        P = new Paragraph();
                        P.Inlines.Add("Subject: " + OriginalMsg.Subject);
                        FD.Blocks.Add(P);

                        P = new Paragraph();
                        FD.Blocks.Add(P);
                        P = new Paragraph();
                        FD.Blocks.Add(P);
                        P = new Paragraph();
                        FD.Blocks.Add(P);

                        TextRange TR = new TextRange(richTextBox.Document.ContentEnd, richTextBox.Document.ContentEnd);
                        using (MemoryStream MS = new MemoryStream(Encoding.Default.GetBytes(OriginalMsg.MessageText)))
                        {
                            TR.Load(MS, DataFormats.Rtf);
                        }

                    }

                    richTextBox.CaretPosition = richTextBox.Document.ContentStart;

                    if (StartMode == MessageEditorStartMode.ForwardMode)
                    {
                        foreach (Attachment Att in OriginalMsg.FileAttachments)
                        {
                            if (!Att.isAllPartsReceived) continue;
                            Attachment NewAtt = new Attachment
                            {
                                Compression = Att.Compression,
                                FileName = Att.FileName,
                                ForwardedParts = Att.FileParts,
                                Hash = Att.Hash,
                                Size = Att.Size,
                                Src = AttachmentSource.Forward

                            };
                            FileAttachments.Add(NewAtt);
                        }

                        SubjectTextBox.Text = OriginalMsg.Subject.StartsWith("FW: ")
                            ? OriginalMsg.Subject
                            : "FW: " + OriginalMsg.Subject;
                    }
                    else
                    {
                        SubjectTextBox.Text = OriginalMsg.Subject.StartsWith("RE: ")
                            ? OriginalMsg.Subject
                            : "RE: " + OriginalMsg.Subject;
                    }
                }
            }


            if (MsgTextFormat == MessageTextFormat.PlainText)
            {
                PlainTextBtn.IsChecked = true;
                FormattingGroup.Visibility = Visibility.Collapsed;

            }
            else
                RichTextBtn.IsChecked = true;



            SetWindowTitle();
            AttachmentsListView.ItemsSource = FileAttachments;
            if (FileAttachments.Count != 0) AttachmentsGrid.Visibility = Visibility.Visible;

            CommandBinding PasteBind=new CommandBinding(ApplicationCommands.Paste,RichTextBoxPaste_Executed,RichTextBoxPaste_CanExecute);
            richTextBox.CommandBindings.Add(PasteBind);
        }

        private static double[] FontSizes => new[] {
            6.0, 8.0, 9.0, 10.0, 11.0, 12.0, 14.0, 16.0, 18.0, 20.0,22.0, 24.0,26.0,28.0, 36.0,48.0,72.0
        };

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _fontFamily.ItemsSource = Fonts.SystemFontFamilies;
            _fontSize.ItemsSource = FontSizes;
        }


        private void onSizeChangedEvent(object sender, SizeChangedEventArgs e)
        {

            if (e.WidthChanged) Utils.UpdateIntValue("MailEditorSizeX", Convert.ToInt64(e.NewSize.Width));
            if (e.HeightChanged) Utils.UpdateIntValue("MailEditorSizeY", Convert.ToInt64(e.NewSize.Height));

        }

        private void onStateChangedEvent(object sender, EventArgs e)
        {
            Utils.UpdateIntValue("isMailEditorMaximized", WindowState == WindowState.Maximized ? 1 : 0);
        }




        private void FontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                FontFamily editValue = (FontFamily) e.AddedItems[0];
                ApplyPropertyValueToSelectedText(TextElement.FontFamilyProperty, editValue);
            }
            catch
            {

            }
        }

        private void FontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ApplyPropertyValueToSelectedText(TextElement.FontSizeProperty, e.AddedItems[0]);
            }
            catch { }
        }

        private void RichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateVisualState();
            }
            catch
            {

            }
        }
        private void UpdateVisualState()
        {
            UpdateToggleButtonState();
            UpdateSelectionListType();
            UpdateSelectedFontFamily();
            UpdateSelectedFontSize();
        }

        private void UpdateToggleButtonState()
        {
            UpdateItemCheckedState(_btnBold, TextElement.FontWeightProperty, FontWeights.Bold);
            UpdateItemCheckedState(_btnItalic, TextElement.FontStyleProperty, FontStyles.Italic);
            UpdateItemCheckedState(_btnUnderline, Inline.TextDecorationsProperty, TextDecorations.Underline);

            UpdateItemCheckedState(_btnAlignLeft, Block.TextAlignmentProperty, TextAlignment.Left);
            UpdateItemCheckedState(_btnAlignCenter, Block.TextAlignmentProperty, TextAlignment.Center);
            UpdateItemCheckedState(_btnAlignRight, Block.TextAlignmentProperty, TextAlignment.Right);
            UpdateItemCheckedState(_btnAlignJustify, Block.TextAlignmentProperty, TextAlignment.Justify);

        }
        void UpdateItemCheckedState(ToggleButton button, DependencyProperty formattingProperty, object expectedValue)
        {
            object currentValue = richTextBox.Selection.GetPropertyValue(formattingProperty);
            button.IsChecked = (currentValue != DependencyProperty.UnsetValue) && (currentValue != null && currentValue.Equals(expectedValue));
        }

        private void UpdateSelectionListType()
        {
            Paragraph startParagraph = richTextBox.Selection.Start.Paragraph;
            Paragraph endParagraph = richTextBox.Selection.End.Paragraph;
            if (startParagraph != null && endParagraph != null && (startParagraph.Parent is ListItem) && (endParagraph.Parent is ListItem) && ReferenceEquals(((ListItem)startParagraph.Parent).List, ((ListItem)endParagraph.Parent).List))
            {
                var list = ((ListItem)startParagraph.Parent).List;
                if (list == null) return;
                TextMarkerStyle markerStyle = list.MarkerStyle;
                if (markerStyle == TextMarkerStyle.Disc)
                {
                    _btnBullets.IsChecked = true;
                }
                else if (markerStyle == TextMarkerStyle.Decimal)
                {
                    _btnNumbers.IsChecked = true;
                }
            }
            else
            {
                _btnBullets.IsChecked = false;
                _btnNumbers.IsChecked = false;
            }
        }

        private void UpdateSelectedFontFamily()
        {
            object value = richTextBox.Selection.GetPropertyValue(TextElement.FontFamilyProperty);
            FontFamily currentFontFamily = (FontFamily)((value == DependencyProperty.UnsetValue) ? null : value);
            if (currentFontFamily != null)
            {
                _fontFamily.SelectedItem = currentFontFamily;
            }
        }

        private void UpdateSelectedFontSize()
        {
            object value = richTextBox.Selection.GetPropertyValue(TextElement.FontSizeProperty);
            _fontSize.SelectedValue = (value == DependencyProperty.UnsetValue) ? null : value;
        }


        void ApplyPropertyValueToSelectedText(DependencyProperty formattingProperty, object value)
        {
            if (value == null)
                return;

            richTextBox.Selection.ApplyPropertyValue(formattingProperty, value);
        }


        private void FontColorGallery_OnSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            object color = FontColorGallery.SelectedItem;
            Color? newColor = null;

            var info = color as PropertyInfo;
            if (info != null)
            {
                newColor = info.GetValue(null) as Color?;
            }
            else if (color is Color)
            {
                newColor = (Color)color;
            }
            if (newColor == null) return;

            richTextBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush((Color)newColor));
        }

        private void BackColorGallery_OnSelectionChangedChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            object color = BackColorGallery.SelectedItem;
            Color? newColor = null;

            var info = color as PropertyInfo;
            if (info != null)
            {
                newColor = info.GetValue(null) as Color?;
            }
            else if (color is Color)
            {
                newColor = (Color)color;
            }
            if (newColor == null) return;

            richTextBox.Selection.ApplyPropertyValue(TextElement.BackgroundProperty,
                new SolidColorBrush((Color)newColor));
        }

        private void ToButton_OnClick(object sender, RoutedEventArgs e)
        {
            SelectRecipientsDialog SRD=new SelectRecipientsDialog(ToContacts);
            SRD.ShowDialog();
        }

        private void ToListView_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                Contact Cnt = ((FrameworkElement)e.OriginalSource).DataContext as Contact;
                if (Cnt != null) ToContacts.Remove(Cnt);
            }
        }

        private void SubjectTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SetWindowTitle();
        }

        private void SetWindowTitle()
        {
            Title = SubjectTextBox.Text.Length == 0 ? "Untitled - Message" : SubjectTextBox.Text + " - Message";
            Title += MsgTextFormat == MessageTextFormat.PlainText ? " (Plain Text)" : " (Rich Text)";
        }

        private void PlainTextBtn_OnClick(object sender, RoutedEventArgs e)
        {
            MsgTextFormat=MessageTextFormat.PlainText;
            SetWindowTitle();
            TextRange t = new TextRange(richTextBox.Document.ContentStart,
                                    richTextBox.Document.ContentEnd);

            string PlainTxt = t.Text;
            t.Text = PlainTxt;
            //t = new TextRange(richTextBox.Document.ContentStart,
                                     //richTextBox.Document.ContentEnd);
            t.ApplyPropertyValue(Paragraph.MarginProperty,new Thickness(0));

            richTextBox.IsUndoEnabled = false;
            richTextBox.IsUndoEnabled = true;
            FormattingGroup.Visibility=Visibility.Collapsed;
        }

        private void RichTextBtn_OnClick(object sender, RoutedEventArgs e)
        {
            MsgTextFormat = MessageTextFormat.RTF;
            SetWindowTitle();
            FormattingGroup.Visibility = Visibility.Visible;
        }

        private void AttachButton_OnClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog OFD = new Microsoft.Win32.OpenFileDialog {Multiselect = true};
            if (OFD.ShowDialog() == true)
            {
               AddAttachments(OFD.FileNames);
            }
            if (FileAttachments.Count!=0) AttachmentsGrid.Visibility=Visibility.Visible;

        }

        private void RichTextBox_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                AddAttachments(files);
            }
            if (FileAttachments.Count != 0) AttachmentsGrid.Visibility = Visibility.Visible;
        }

        private void AttachmentsList_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                Attachment A = ((FrameworkElement)e.OriginalSource).DataContext as Attachment;
                if (A != null) FileAttachments.Remove(A);
            }
            if (FileAttachments.Count == 0) AttachmentsGrid.Visibility = Visibility.Collapsed;
        }


        private void AddAttachments(string[] filenames)
        {
            bool isMultiple = (filenames.Length > 1);
            YesNoToAll lastReply=YesNoToAll.No;
            foreach (string str in filenames)
            {
                Attachment NewAttachment = new Attachment
                {
                    FileName = Path.GetFileName(str),
                    Src = AttachmentSource.File,
                    FullFilePath = str
                };

                FileInfo fi = new FileInfo(str);
                NewAttachment.Size = fi.Length;

                Attachment OldAttachment =
                    FileAttachments.SingleOrDefault(p => p.FileName == NewAttachment.FileName);

                if (OldAttachment == null)
                    FileAttachments.Add(NewAttachment);
                else
                {
                    switch (lastReply)
                    {
                        case YesNoToAll.NoToAll:
                            break;
                        case YesNoToAll.YesToAll:
                            FileAttachments.Remove(OldAttachment);
                            FileAttachments.Add(NewAttachment);
                            break;
                        default:
                            ExistingFileReplaceDialog FRD = new ExistingFileReplaceDialog(OldAttachment.FullNameStr + OldAttachment.FileSizeStr, NewAttachment.FullFilePath + NewAttachment.FileSizeStr, isMultiple);
                            FRD.ShowDialog();
                            lastReply = FRD.Result;
                            if ((lastReply == YesNoToAll.Yes) || (lastReply == YesNoToAll.YesToAll))
                            {
                                FileAttachments.Remove(OldAttachment);
                                FileAttachments.Add(NewAttachment);
                            }
                            break;
                    }
                }
            }
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
            e.Handled = true;
        }

        private void SendMessage_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (ToContacts.Count > 0);
        }

        private void SendMessage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            TextRange TR=new TextRange(richTextBox.Document.ContentStart,richTextBox.Document.ContentEnd);
            string MsgText;

            if (MsgTextFormat==MessageTextFormat.PlainText) MsgText=TR.Text;
            else
            {
                MemoryStream ms = new MemoryStream();
                TR.Save(ms, DataFormats.Rtf);
                MsgText = Encoding.Default.GetString(ms.ToArray());
            }

            int FilePartSize = (int)Utils.GetIntValue("FilePartSize", 2097152);

            List<MailMessageSendingData> MsgList = new List<MailMessageSendingData>();

            foreach (Contact cnt in ToContacts)
            {
                MsgList.Add(new MailMessageSendingData
                {
                    ToContact = cnt,
                    MessageBody = new MailMessageBody
                    {
                        Subject = SubjectTextBox.Text,
                        TextFormat = MsgTextFormat,
                        MessageText = MsgText,
                        MessageFlags = 0
                    }
                });
            }


            if (FileAttachments.Count > 0)
            {
                foreach (Attachment Att in FileAttachments)
                {
                    byte[] CurrentHash;
                    FileCompression CurrentCompression;
                    foreach (MailMessageSendingData MMSD in MsgList)
                    {
                        MMSD.CurrentAttachment = new Attachment
                        {
                            FileName = Att.FileName,
                            Size = Att.Size
                        };
                    }

                    if (Att.Src == AttachmentSource.Forward)
                    {
                        CurrentHash = Att.Hash;
                        CurrentCompression = Att.Compression;
                        using (DB DBC=new DB())
                        {

                            foreach (long FilePartMsgID in Att.ForwardedParts)
                            {
                                long MessageId = OriginalMsgInfo.FromServer != 0 ? OriginalContact.TranslateMsgID(FilePartMsgID) : FilePartMsgID;
                                byte[] FilePartBytes =
                                    DBC.Conn.QuerySingleOrDefault<byte[]>(
                                        "Select MessageBody from Messages where FromUser=@FromUsr and FromServer=@FromSrv and MessageID=@MsgID",
                                        new {FromUsr=OriginalMsgInfo.FromUser, FromSrv=OriginalMsgInfo.FromServer, MsgId=MessageId});

                                byte[] DecodedBytes = ParanoidHelpers.DecryptMessage(OriginalContact.StorageKey, FilePartBytes);


                                foreach (MailMessageSendingData MMSD in MsgList)
                                {
                                    byte[] EncryptedData = ParanoidHelpers.MakeEncryptedMessage(
                                        MMSD.ToContact.StorageKey, DecodedBytes);

                                    MMSD.CurrentAttachment.FileParts.Add(
                                        Message.PostMessage(MMSD.ToContact.ParentAccount.AccountID, 0,
                                            MMSD.ToContact.ContactID, MMSD.ToContact.ParentAccount.AccountID,
                                            (int)MsgType.FileAttachmentPart, EncryptedData));

                                }
                                for (int i = 0; i < DecodedBytes.Length; i++) DecodedBytes[i] = 0;
                            }
                        }
                    }
                    else //file
                    {
                        FileStream FS;
                        try
                        {
                            FS = new FileStream(Att.FullFilePath, FileMode.Open, FileAccess.Read);
                        }
                        catch (Exception Ex)
                        {
                            MessageBox.Show(Ex.Message);
                            continue;
                        }
                        long FileLenght = FS.Length;
                        long CurrentFilePos = 0;
                        bool isFirstPart = true;
                        CurrentCompression = FileCompression.LZMA;
                        Blake256 Bl = new Blake256();
                        Random Rnd = new Random();

                        while (CurrentFilePos < FileLenght)
                        {
                            int CurrentPartLenght = Rnd.Next((int)(FilePartSize * 0.9), (int)(FilePartSize * 1.1));
                            if (FileLenght < CurrentFilePos + CurrentPartLenght)
                                CurrentPartLenght = (int)(FileLenght - CurrentFilePos);
                            byte[] FilePartData = new byte[CurrentPartLenght];
                            try
                            {
                                FS.Read(FilePartData, 0, CurrentPartLenght);
                            }
                            catch (Exception Ex)
                            {
                                MessageBox.Show("Error reading "+ Att.FullFilePath+"\n\r"+Ex.Message+"\r\nSkipping this file");
                                using (DB DBC=new DB())
                                {

                                    foreach (MailMessageSendingData MMSD in MsgList)
                                    {
                                        foreach (long ID in MMSD.CurrentAttachment.FileParts)
                                        {
                                            DBC.Conn.Execute(
                                                "Delete from Messages where FromUser=@FromUsr and FromServer=0 and MessageID=@MsgID",
                                                new {FromUsr = MMSD.ToContact.ParentAccount.AccountID, MsgID = ID});
                                        }
                                        MMSD.CurrentAttachment = null;
                                    }
                                }

                                break;
                            }
                            Bl.TransformBytes(FilePartData);

                            if (CurrentCompression != FileCompression.Uncompressed)
                            {
                                byte[] CompressedPartData = SevenZipHelper.Compress(FilePartData);
                                if (!isFirstPart)
                                    FilePartData = CompressedPartData;
                                else
                                {
                                    if (CompressedPartData.Length >= FilePartData.Length * 0.9)
                                    //compress only if compressed file size <=90% of original
                                    {
                                        CurrentCompression = FileCompression.Uncompressed;
                                    }
                                    else
                                    {
                                        FilePartData = CompressedPartData;
                                    }
                                    isFirstPart = false;
                                }
                            }

                            foreach (MailMessageSendingData MMSD in MsgList)
                            {
                                byte[] EncodedBytes = ParanoidHelpers.MakeEncryptedMessage(
                                    MMSD.ToContact.StorageKey, FilePartData);

                                MMSD.CurrentAttachment.FileParts.Add(
                                    Message.PostMessage(MMSD.ToContact.ParentAccount.AccountID, 0,
                                        MMSD.ToContact.ContactID, MMSD.ToContact.ParentAccount.AccountID,
                                        (int)MsgType.FileAttachmentPart, EncodedBytes));

                            }
                            CurrentFilePos += CurrentPartLenght;
                        }
                        CurrentHash = (Bl.TransformFinal()).GetBytes();
                    }

                    foreach (MailMessageSendingData MMSD in MsgList)
                    {
                        if (MMSD.CurrentAttachment != null)
                        {
                            MMSD.CurrentAttachment.Compression = CurrentCompression;
                            MMSD.CurrentAttachment.Hash = CurrentHash;
                            MMSD.MessageBody.FileAttachments.Add(MMSD.CurrentAttachment);
                        }
                    }


                }


            }

            foreach (MailMessageSendingData MMSD in MsgList)
            {
                MMSD.MessageBody.SendTime = Utils.DateTimeToLong(DateTime.Now);
                byte[] MsgData = SevenZipHelper.Compress(Utils.ObjectToBytes(MMSD.MessageBody));
                byte[] EncryptedData = ParanoidHelpers.MakeEncryptedMessage(MMSD.ToContact.StorageKey, MsgData);

                Message MSG = new Message
                {
                    FromUser = MMSD.ToContact.ParentAccount.AccountID,
                    FromServer = 0,
                    ToUser = MMSD.ToContact.ContactID,
                    ToServer = MMSD.ToContact.ParentAccount.AccountID,
                    MessageType = (int)MsgType.MailMessage,
                    MessageBody = EncryptedData,
                    MessageStatus = (int)MsgStatus.Outbox,
                    MessageID = Utils.MakeMsgID()
                };
                Message.SaveToDB(MSG);
                //
                for (int i = 0; i < MsgData.Length; i++) MsgData[i] = 0;

                MainWindow.AddMessageToList(MMSD.ToContact, new MessageListItem(MSG));
            }

            IEnumerable<Account> CallAccs = (from p in MsgList
                            group p by p.ToContact.ParentAccount
                into groups
                            select groups.First().ToContact.ParentAccount);

            foreach (Account Acc in CallAccs) BackgroundTasks.SendReceiveAccount(Acc);
            Close();
            Mouse.OverrideCursor = Cursors.Arrow;

        }

        private void RichTextBoxPaste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Clipboard.ContainsData(DataFormats.Text) ||
                           ((MsgTextFormat == MessageTextFormat.RTF) && (Clipboard.ContainsData(DataFormats.Rtf)));
            e.Handled = true;
        }

        private void RichTextBoxPaste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            richTextBox.Paste();
            if (MsgTextFormat == MessageTextFormat.PlainText)
            {
                TextRange TR = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                string PlainTxt = TR.Text;
                TR.Text = PlainTxt;
            }

        }
    }
}
