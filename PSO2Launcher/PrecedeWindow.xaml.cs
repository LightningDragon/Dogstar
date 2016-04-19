using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

using static Dogstar.Helper;

namespace Dogstar
{
	/// <summary>
	/// Interaction logic for PrecedeWindow.xaml
	/// </summary>
	public partial class PrecedeWindow
	{
		public PrecedeWindow()
		{
			InitializeComponent();
		}

		private void DownloadStarted(object sender, string e)
		{
			DownloadProgress.Dispatcher.InvokeAsync(() =>
			{
				DownloadProgress.Maximum = 100;
				DownloadProgress.IsIndeterminate = false;
				//CurrentGeneralDownloadSizeActionLable.Visibility = Visibility.Visible;
				//CurrentGeneralDownloadActionlabel.Content = Path.GetFileNameWithoutExtension(e);
			});
		}

		private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			ScanProgress.Dispatcher.InvokeAsync(() =>
			{
				DownloadProgress.Value = e.ProgressPercentage;
				//CurrentGeneralDownloadSizeActionLable.Content = $"{SizeSuffix(e.BytesReceived)}/{SizeSuffix(e.TotalBytesToReceive)}";
			});
		}

		private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
		{
			using (var mannager = new DownloadManager())
			{
				mannager.DownloadStarted += DownloadStarted;
				mannager.DownloadProgressChanged += DownloadProgressChanged;

				if (ManagementData.ContainsKey("PrecedeVersion") && ManagementData.ContainsKey("PrecedeCurrent"))
				{
					var listdatas = new PatchListEntry[int.Parse(ManagementData["PrecedeCurrent"]) + 1][];

					for (int index = 0; index < listdatas.Length; index++)
					{
						listdatas[index] = ParsePatchList(await mannager.DownloadStringTaskAsync(new Uri(BasePrePatch, $"patchlist{index}.txt"))).ToArray();
					}

					var groups = (from v in listdatas.SelectMany(x => x) group v by v.Name into d select d.First()).ToArray();

					for (var index = 0; index < groups.Length;)
					{
						var data = groups[index];
						//CurrentCheckActionlabel.Content = Path.GetFileNameWithoutExtension(data.Name);
						var filePath = MakeLocalToGame(System.IO.Path.ChangeExtension(data.Name, null));

						var upToDate = await Task.Run(() => IsFileUpToDate(filePath, data.Size, data.Hash));
						//CheckProgressbar.Value = ++index;
						//CompletedCheckActionslabel.Content = string.Format(Text.CheckedOf, index, groups.Length);

						if (!upToDate)
						{
							//var patPath = MakeLocalToGame(data.Name);

							//fileOperations.Add(downloadManager.DownloadFileTaskAsync(newlistdata.Contains(data) || launcherlistdata.Contains(data) ? new Uri(BasePatch, data.Name) : new Uri(BasePatchOld, data.Name), patPath).ContinueWith(x => MoveAndOverwriteFile(patPath, filePath)));

							//numberToDownload++;
							//CompletedCheckDownloadActionslabel.Content = string.Format(Text.DownloadedOf, numberDownloaded, numberToDownload);
						}
					}
				}
			}
		}
	}
}
