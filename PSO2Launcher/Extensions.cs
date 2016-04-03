using System;
using System.Reflection;
using System.Threading.Tasks;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace DogStar
{
	public static class Extensions
	{
		public static string[] LineSplit(this string str) => str.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

		public static async Task<string> ShowFileSelectAsync(this MetroWindow window, string title, string message, string filter, MetroDialogSettings settings = null)
		{
			settings = settings ?? window.MetroDialogOptions;
			var dialog = new FileSelectDialog(window, settings) { Title = title, Message = message, Filter = filter };
			var result = await window.ShowMetroDialogAsync(dialog).ContinueWith(x => dialog.WaitForButtonPressAsync()).Unwrap();
			await window.HideMetroDialogAsync(dialog);
			return result;
		}

		public static async Task<string> ShowFolderSelectAsync(this MetroWindow window, string title, string message, string rootFolder, MetroDialogSettings settings = null)
		{
			settings = settings ?? window.MetroDialogOptions;
			var dialog = new FolderSelectDialog(window, settings) { Title = title, Message = message, RootFolder = rootFolder };
			var result = await window.ShowMetroDialogAsync(dialog).ContinueWith(x => dialog.WaitForButtonPressAsync()).Unwrap();
			await window.HideMetroDialogAsync(dialog);
			return result;
		}
	}
}
