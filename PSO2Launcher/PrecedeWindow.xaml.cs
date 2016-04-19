using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Dogstar.Resources;
using static Dogstar.Helper;

namespace Dogstar
{
	/// <summary>
	/// Interaction logic for PrecedeWindow.xaml
	/// </summary>
	public partial class PrecedeWindow
	{
		// TODO: Message box on completion

		private int numberDownloaded;
		private int numberToDownload;
		private long doneBytes;
		private long totalBytes;
		private bool _isPaused;

		private readonly CancellationTokenSource _checkCancelSource = new CancellationTokenSource();

		public PrecedeWindow()
		{
			InitializeComponent();
		}

		private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			ScanProgress.Dispatcher.InvokeAsync(() =>
			{
				DownloadProgress.Value = doneBytes + e.BytesReceived;
				DownloadProgressLabel.Content = $"{SizeSuffix(doneBytes + e.BytesReceived)}/{SizeSuffix(totalBytes)}";
			});
		}

		private void DownloadCompleted(object sender, EventArgs e)
		{
			DownloadLabel.Dispatcher.InvokeAsync(() =>
			{
				numberDownloaded++;
				DownloadLabel.Content = string.Format(Text.DownloadedOf, numberDownloaded, numberToDownload);
			});
		}

		private void buttonPause_Click(object sender, RoutedEventArgs e) => _isPaused = !_isPaused;

		private void buttonCancel_Click(object sender, RoutedEventArgs e) => _checkCancelSource.Cancel();

		private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
		{
			await ZZZ();
			Close();
		}

		private async Task ZZZ()
		{
			using (var manager = new DownloadManager())
			{
				try
				{
					var fileOperations = new List<Task>();

					if (ManagementData.ContainsKey("PrecedeVersion") && ManagementData.ContainsKey("PrecedeCurrent"))
					{
						CreateDirectoryIfNoneExists(Path.Combine(PrecedeFolder, "data", "win32"));

						var listdatas = new PatchListEntry[int.Parse(ManagementData["PrecedeCurrent"]) + 1][];

						for (int index = 0; index < listdatas.Length; index++)
						{

							var filename = $"patchlist{index}.txt";
							var data = await manager.DownloadStringTaskAsync(new Uri(BasePrePatch, filename));
							listdatas[index] = ParsePatchList(data).ToArray();
							await Task.Run(() => File.WriteAllText(Path.Combine(PrecedeFolder, filename), data));
						}

						manager.DownloadProgressChanged += DownloadProgressChanged;
						manager.DownloadCompleted += DownloadCompleted;

						var groups = (from v in listdatas.SelectMany(x => x) group v by v.Name into d select d.First()).ToArray();
						var precedePath = PrecedeFolder;

						totalBytes = groups.Select(x => x.Size).Sum();
						DownloadProgress.Maximum = totalBytes;
						ScanProgress.Maximum = groups.Length;

						Func<Task> pause = async () =>
						{
							var taskSource = new TaskCompletionSource<object>();
							RoutedEventHandler unpause = (s, e) => taskSource.TrySetResult(null);
							RoutedEventHandler cancel = (s, e) => taskSource.TrySetCanceled();

							try
							{
								buttonPause.Click += unpause;
								buttonCancel.Click += cancel;
								manager.PauseDownloads(taskSource.Task);
								await taskSource.Task;
							}
							finally
							{
								buttonPause.Click -= unpause;
								buttonCancel.Click -= cancel;
							}
						};

						for (var index = 0; index < groups.Length;)
						{
							_checkCancelSource.Token.ThrowIfCancellationRequested();

							if (_isPaused)
							{
								await pause();
							}
							else
							{
								var data = groups[index];
								var name = Path.ChangeExtension(data.Name, null);
								var filePath = Path.Combine(precedePath, name);
								ScanLabel.Content = name;
								var upToDate = await Task.Run(() => IsFileUpToDate(File.Exists(filePath) ? filePath : MakeLocalToGame(name), data.Size, data.Hash));
								ScanProgress.Value = ++index;
								ScanProgressLabel.Content = string.Format(Text.CheckedOf, index, groups.Length);

								if (upToDate)
								{
									totalBytes -= data.Size;
									DownloadProgress.Maximum = totalBytes;
								}
								else
								{
									numberToDownload++;

									var patPath = Path.Combine(precedePath, data.Name);
									fileOperations.Add(manager.DownloadFileTaskAsync(new Uri(BasePrePatch, data.Name), patPath).ContinueWith(x =>
									{
										lock (this)
										{
											doneBytes += data.Size;
										}

										MoveAndOverwriteFile(patPath, filePath);
									}));

									DownloadLabel.Content = string.Format(Text.DownloadedOf, numberDownloaded, numberToDownload);
								}

								index++;
							}
						}

						var downloads = Task.WhenAll(fileOperations);

						while (!downloads.IsCompleted && !downloads.IsCanceled && !downloads.IsFaulted)
						{
							_checkCancelSource.Token.ThrowIfCancellationRequested();

							if (_isPaused)
							{
								await pause();
							}
							else
							{
								await Task.Delay(16);
							}
						}

						await downloads;
					}

					await Task.Run(() => File.WriteAllText(PrecedeTxtPath, $"{ManagementData["PrecedeVersion"]}\t{ManagementData["PrecedeCurrent"]}"));
				}
				catch when (_checkCancelSource.IsCancellationRequested)
				{
					manager.CancelDownloads();
				}
			}
		}
	}
}

