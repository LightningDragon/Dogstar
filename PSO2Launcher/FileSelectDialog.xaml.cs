using System;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

namespace DogStar
{
	public partial class FileSelectDialog
	{
		private readonly OpenFileDialog _openFileDialog = new OpenFileDialog();

		public string FilePath => PathBox.Text;

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

		public Task<string> WaitForButtonPressAsync()
		{
			var taskSource = new TaskCompletionSource<string>();
			Action cleanUp = null;

			RoutedEventHandler buttonClick = (sender, e) =>
			{
				cleanUp();
				taskSource.TrySetResult(PathBox.Text);
			};

			cleanUp = () =>
			{
				AffirmativeButton.Click -= buttonClick;
				NegativeButton.Click -= buttonClick;
			};

			AffirmativeButton.Click += buttonClick;
			NegativeButton.Click += buttonClick;

			return taskSource.Task;
		}

		private void FileBrowse_Click(object sender, RoutedEventArgs e)
		{
			if (_openFileDialog.ShowDialog(OwningWindow).GetValueOrDefault())
			{
				PathBox.Text = _openFileDialog.FileName;
			}
		}
	}
}
