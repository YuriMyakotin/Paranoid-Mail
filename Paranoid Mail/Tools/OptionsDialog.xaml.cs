using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;

namespace Paranoid
{
	/// <summary>
	/// Interaction logic for OptionsDialog.xaml
	/// </summary>
	public partial class OptionsDialog : Window
	{
		public OptionsDialog()
		{
			InitializeComponent();
			List<NamedValue> updatesComboBoxList = new List<NamedValue>
			{
				new NamedValue(0, "Never (not recommended)"),
				new NamedValue(1, "Critical updates only"),
				new NamedValue(2, "Critical and major releases"),
				new NamedValue(3, "All releases (default)")
			};

			List<NamedValue> newMessagesFormatComboBoxList = new List<NamedValue>
			{
				new NamedValue(0, "Plain text"),
				new NamedValue(1, "Rich text"),
			};

			UpdateCheckTypes.ItemsSource = updatesComboBoxList;
			NewMessagesFormatCombobox.ItemsSource = newMessagesFormatComboBoxList;

			UpdateCheckTypes.SelectedValue = (int)Utils.GetIntValue("AutoUpdateMode", 3);

			NewMessagesFormatCombobox.SelectedValue = (int)Utils.GetIntValue("DefaultMessageTextFormat", 1);

			SendReceiveInterval.Text = Utils.GetIntValue("SendReceiveInterval", 180).ToString();
			FilePartSize.Text = Utils.GetIntValue("FilePartSize", 2097152).ToString();

			if (CryptoData.FileName == "::DB")
			{
				PortableButton.IsChecked = true;
			}
			else
			{
				OtherPlaceButton.IsChecked = true;
				FileNameTextBox.Text = CryptoData.FileName;

			}

		}

		private void SaveButton_OnClick(object sender, RoutedEventArgs e)
		{
			SendReceiveInterval.Background = Brushes.White;
			FilePartSize.Background = Brushes.White;
			FileNameTextBox.Background = Brushes.White;

			string OldKeyFileName = CryptoData.FileName;
		    if (OldKeyFileName == "::DB")
		    {
		        if (OtherPlaceButton.IsChecked == true)
		        {
		            if (FileNameTextBox.Text.Length < 2)
		            {
		                FileNameTextBox.Background = Brushes.Red;
		                return;

		            }
		            CryptoData.FileName = FileNameTextBox.Text;
		            if (!CryptoData.SaveKeys())
		            {
		                FileNameTextBox.Background = Brushes.Red;
		                return;
		            }
                    Utils.UpdateStringValue("KeyFileName",CryptoData.FileName);
                    Utils.DeleteValue(ValueType.BinaryType, "KeyData");

		        }
		    }
		    else
		    {
		        CryptoData.FileName = PortableButton.IsChecked == true ? "::DB" : FileNameTextBox.Text;

		        if (CryptoData.FileName != OldKeyFileName)
		        {
		            if (!CryptoData.SaveKeys())
		            {
		                FileNameTextBox.Background = Brushes.Red;
		                return;
		            }
                    Utils.UpdateStringValue("KeyFileName", CryptoData.FileName);
                    try
		            {
		                File.Delete(OldKeyFileName);
		            }
		            catch (Exception E)
		            {
		                MessageBox.Show(E.Message);
		            }
		        }
		    }



			int SendReceiveIntervalValue, FilePartSizeValue;
			if (!ValidateValue(SendReceiveInterval, 30, int.MaxValue, out SendReceiveIntervalValue))
			{
				SendReceiveInterval.Background = Brushes.Red;
				return;
			}

			if (!ValidateValue(FilePartSize, 819200, 8388608, out FilePartSizeValue))
			{
				FilePartSize.Background = Brushes.Red;
				return;
			}


			Utils.UpdateIntValue("SendReceiveInterval", SendReceiveIntervalValue);
			Utils.UpdateIntValue("FilePartSize", FilePartSizeValue);
			Utils.UpdateIntValue("AutoUpdateMode",(int) UpdateCheckTypes.SelectedValue);
			Utils.UpdateIntValue("DefaultMessageTextFormat", (int)NewMessagesFormatCombobox.SelectedValue);

			Close();
		}

		private static bool ValidateValue(TextBox src, int MinValue, int MaxValue, out int Value)
		{
			if (!int.TryParse(src.Text,out Value)) return false;
			return (Value >= MinValue) && (Value <= MaxValue);
		}

		private void PortableButton_OnChecked(object sender, RoutedEventArgs e)
		{
			BrowseButton.IsEnabled = false;
			FileNameTextBox.IsEnabled = false;
		}

		private void OtherPlaceButton_OnChecked(object sender, RoutedEventArgs e)
		{
			BrowseButton.IsEnabled = true;
			FileNameTextBox.IsEnabled = true;
		}

		private void BrowseButton_OnClick(object sender, RoutedEventArgs e)
		{
			string CurrentName = FileNameTextBox.Text;
			if (CurrentName.Length < 2) CurrentName = "paranoid.key";

			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
			{
				FileName = System.IO.Path.GetFileName(CurrentName),
				InitialDirectory = System.IO.Path.GetDirectoryName(CurrentName),
				Filter = "All files (*.*)|*.*"
			};
			bool? result = dlg.ShowDialog();
			if (result == true) FileNameTextBox.Text = dlg.FileName;
		}
	}
}
