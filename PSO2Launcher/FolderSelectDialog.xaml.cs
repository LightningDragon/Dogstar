using System;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Dogstar
{
	public partial class FolderSelectDialog
	{
		private readonly OpenFolderDialog _openFolderDialog = new OpenFolderDialog();

		public string Result => PathBox.Text;

		public string RootFolder
		{
			get { return _openFolderDialog.RootFolder; }
			set { _openFolderDialog.RootFolder = value; }
		}

		public string Message
		{
			get { return MessageBlock.Text; }
			set { MessageBlock.Text = value; }
		}

		public FolderSelectDialog(MetroWindow parentWindow, MetroDialogSettings settings) : base(parentWindow, settings)
		{
			InitializeComponent();
			AffirmativeButton.Content = settings.AffirmativeButtonText;
			NegativeButton.Content = settings.NegativeButtonText;
		}

		private void FileBrowse_Click(object sender, RoutedEventArgs e)
		{
			if (_openFolderDialog.ShowDialog(OwningWindow).GetValueOrDefault())
			{
				PathBox.Text = _openFolderDialog.SelectedPath;
			}
		}

		public Task<string> WaitForButtonPressAsync()
		{
			var taskSource = new TaskCompletionSource<string>();
			Action cleanUp = null;

			RoutedEventHandler buttonClick = (sender, e) =>
			{
				cleanUp();
				taskSource.TrySetResult(sender == AffirmativeButton ? PathBox.Text : string.Empty);
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
	}
}
