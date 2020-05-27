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

		private int _numberDownloaded;
		private int _numberToDownload;
		private long _doneBytes;
		private long _totalBytes;
		private bool _isPaused;

		private readonly CancellationTokenSource _checkCancelSource = new CancellationTokenSource();

		readonly PatchProvider patchProvider;

		public PrecedeWindow(PatchProvider patchProvider)
		{
			this.patchProvider = patchProvider;
			InitializeComponent();
		}

		private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			ScanProgress.Dispatcher.InvokeAsync(() =>
			{
				DownloadProgress.Value = _doneBytes + e.BytesReceived;
				DownloadProgressLabel.Content = $"{SizeSuffix(_doneBytes + e.BytesReceived)}/{SizeSuffix(_totalBytes)}";
			});
		}

		private void DownloadCompleted(object sender, EventArgs e)
		{
			DownloadLabel.Dispatcher.InvokeAsync(() =>
			{
				_numberDownloaded++;
				DownloadLabel.Content = string.Format(Text.DownloadedOf, _numberDownloaded, _numberToDownload);
			});
		}

		private void ButtonPause_Click(object sender, RoutedEventArgs e) => _isPaused = !_isPaused;

		private void ButtonCancel_Click(object sender, RoutedEventArgs e) => _checkCancelSource.Cancel();

		private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
		{
			await DownloadPrecede();
			Close();
		}

		private async Task DownloadPrecede()
		{
			using (var manager = new DownloadManager())
			{
				try
				{
					var fileOperations = new List<Task>();

					if (patchProvider.ManagementData.ContainsKey("PrecedeVersion") && patchProvider.ManagementData.ContainsKey("PrecedeCurrent"))
					{
						CreateDirectoryIfNoneExists(Path.Combine(PatchProvider.PrecedeFolder, "data", "win32"));

						var listdatas = new PatchListEntry[int.Parse(patchProvider.ManagementData["PrecedeCurrent"]) + 1][];

						for (var index = 0; index < listdatas.Length; index++)
						{

							var filename = $"patchlist{index}.txt";
							var data = await manager.DownloadStringTaskAsync(new Uri(patchProvider.BasePrecede, filename));
							listdatas[index] = PatchListEntry.Parse(data).ToArray();
							await Task.Run(() => File.WriteAllText(Path.Combine(PatchProvider.PrecedeFolder, filename), data));
						}

						manager.DownloadProgressChanged += DownloadProgressChanged;
						manager.DownloadCompleted += DownloadCompleted;

						var groups = (from v in listdatas.SelectMany(x => x) group v by v.Name into d select d.First()).ToArray();
						var precedePath = PatchProvider.PrecedeFolder;

						_totalBytes = groups.Select(x => x.Size).Sum();
						DownloadProgress.Maximum = _totalBytes;
						ScanProgress.Maximum = groups.Length;

						Func<Task> pause = async () =>
						{
							var taskSource = new TaskCompletionSource<object>();
							RoutedEventHandler unpause = (s, e) => taskSource.TrySetResult(null);
							RoutedEventHandler cancel = (s, e) => taskSource.TrySetCanceled();

							try
							{
								ButtonPause.Click += unpause;
								ButtonCancel.Click += cancel;
								manager.PauseDownloads(taskSource.Task);
								await taskSource.Task;
							}
							finally
							{
								ButtonPause.Click -= unpause;
								ButtonCancel.Click -= cancel;
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
								ScanProgressLabel.Content = name;
								var upToDate = await Task.Run(() => IsFileUpToDate(File.Exists(filePath) ? filePath : MakeLocalToGame(name), data.Size, data.Hash));
								ScanProgress.Value = ++index;
								ScanLabel.Content = string.Format(Text.CheckedOf, index, groups.Length);

								if (upToDate)
								{
									_totalBytes -= data.Size;
									DownloadProgress.Maximum = _totalBytes;
								}
								else
								{
									_numberToDownload++;

									var patPath = Path.Combine(precedePath, data.Name);
									fileOperations.Add(manager.DownloadFileTaskAsync(new Uri(patchProvider.BasePrecede, data.Name), patPath).ContinueWith(x =>
									{
										lock (this)
										{
											_doneBytes += data.Size;
										}

										MoveAndOverwriteFile(patPath, filePath);
									}));

									DownloadLabel.Content = string.Format(Text.DownloadedOf, _numberDownloaded, _numberToDownload);
								}
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

					await Task.Run(() => File.WriteAllText(patchProvider.PrecedeTxtPath, $"{patchProvider.ManagementData["PrecedeVersion"]}\t{patchProvider.ManagementData["PrecedeCurrent"]}"));
				}
				catch when (_checkCancelSource.IsCancellationRequested)
				{
					manager.CancelDownloads();
				}
			}
		}
	}
}

