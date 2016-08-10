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
		}

		private void SaveButton_OnClick(object sender, RoutedEventArgs e)
		{
			SendReceiveInterval.Background = Brushes.White;
			FilePartSize.Background = Brushes.White;

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
	}
}
