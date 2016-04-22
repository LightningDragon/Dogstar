using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

namespace Dogstar
{
	public partial class FileSelectDialog
	{
		private readonly OpenFileDialog _openFileDialog = new OpenFileDialog();

		public string Result => PathBox.Text;

		public string Filter
		{
			get { return _openFileDialog.Filter; }
			set { _openFileDialog.Filter = value; }
		}

		public string Message
		{
			get { return MessageBlock.Text; }
			set { MessageBlock.Text = value; }
		}

		public FileSelectDialog(MetroWindow parentWindow, MetroDialogSettings settings) : base(parentWindow, settings)
		{
			InitializeComponent();
			AffirmativeButton.Content = settings.AffirmativeButtonText;
			NegativeButton.Content = settings.NegativeButtonText;
		}

		private void FileBrowse_Click(object sender, RoutedEventArgs e)
		{
			if (_openFileDialog.ShowDialog(OwningWindow).GetValueOrDefault())
			{
				PathBox.Text = _openFileDialog.FileName;
			}
		}

		public Task<string> WaitForButtonPressAsync()
		{
			var taskSource = new TaskCompletionSource<string>();

			RoutedEventHandler buttonClick = (sender, e) =>
			{
				taskSource.TrySetResult(ReferenceEquals(sender, AffirmativeButton) ? PathBox.Text : string.Empty);
			};

			AffirmativeButton.Click += buttonClick;
			NegativeButton.Click += buttonClick;

			return taskSource.Task.ContinueWith(x => { AffirmativeButton.Click -= buttonClick; NegativeButton.Click -= buttonClick; return x; }).Unwrap();
		}
	}
}
