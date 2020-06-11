using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Dogstar.GameEditionManagement;
using Dogstar.Properties;
using Dogstar.Resources;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using static System.Convert;
using static Dogstar.External;
using static Dogstar.Helper;
using static MahApps.Metro.ThemeManager;

namespace Dogstar
{
	// TODO: Maybe save management_beta.txt to documents\sega\phantasystaronline2 because vanilla launcher does it
	// TODO: figure out documents\sega\pso2\download patch lists
	// TODO: Figure out why vanilla launcher keeps deleting version.ver
	// TODO: General download tab needs pause/cancel. Oohhhh boy.
	// TODO: Switch to general download tab when restoring backups.
	// TODO: Fix all them WIN7 UIs

	public partial class MainWindow : IDisposable
	{
		private readonly DownloadManager _generalDownloadManager = new DownloadManager();
		private readonly TabController _gameTabController;

		private CancellationTokenSource _checkCancelSource = new CancellationTokenSource();
		private bool _isPrecedeDownloading;
		private bool _isCheckPaused;
		private double _lastTop;
		private double _lastLeft;

		public string WindowTittle => $"{ApplicationInfo.Name} {ApplicationInfo.Version}";

		GameEditionManager game;
		PsoSettings psoSettings;

		public MainWindow()
		{
			// UNDONE: MAKE REGION SELECTABLE!
			game = new NorthAmericaWin10EditionManager();
			// UNDONE: THIS SHOULD NOT BE HAPPENING HERE!
			psoSettings = new PsoSettings(game);

			ChangeAppStyle(Application.Current, GetAccent(Settings.Default.AccentColor), GetAppTheme(Settings.Default.Theme));
			InitializeComponent();

			_generalDownloadManager.DownloadStarted += DownloadStarted;
			_generalDownloadManager.DownloadProgressChanged += DownloadProgressChanged;

			Topmost = Settings.Default.AlwaysOnTop;
			Colors.SelectedIndex = Array.IndexOf(UiResources.GetColor().Values.ToArray(), Settings.Default.AccentColor);
			Themes.SelectedIndex = Array.IndexOf(UiResources.GetTheme().Values.ToArray(), Settings.Default.Theme);

			_gameTabController = new TabController(GameTabControl);
		}

		#region Events


		private void Debug_MouseDown(object sender, MouseButtonEventArgs e)
		{
			DebugFlyout.IsOpen = !DebugFlyout.IsOpen;
		}

		private void Donate_MouseDown(object sender, MouseButtonEventArgs e)
		{
			DonationFlyout.IsOpen = !DebugFlyout.IsOpen;
		}

		private void Twitter_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Process.Start(Properties.Resources.DogstarTwitter);
		}

		private void Github_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Process.Start(Properties.Resources.DogstarGithub);
		}

		private void Information_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Process.Start(Properties.Resources.DogstarSupport);
		}

		private void CancelCheckButton_Click(object sender, RoutedEventArgs e)
		{
			_checkCancelSource.Cancel();
		}

		private void AlwaysOnTop_Changed(object sender, RoutedEventArgs e)
		{
			Topmost = Settings.Default.AlwaysOnTop = AlwaysOnTop.IsChecked.GetValueOrDefault();
		}

		private void Launch_Changed(object sender, RoutedEventArgs e)
		{
			Settings.Default.CloseOnLaunch = Launch.IsChecked.GetValueOrDefault();
		}

		private void DonateToDogstar_Click(object sender, RoutedEventArgs e)
		{
			Process.Start(Properties.Resources.DogstarDonation);
		}

		private void DonateToPolaris_Click(object sender, RoutedEventArgs e)
		{
			Process.Start(Properties.Resources.PolarisDonation);
		}

		private void EnhancementsTile_Click(object sender, RoutedEventArgs e)
		{
			_gameTabController.ChangeTab(EnhancementsTabItem);
		}

		private void BackButton_Click(object sender, RoutedEventArgs e)
		{
			_gameTabController.PreviousTab();
		}

		private void OtherTile_Click(object sender, RoutedEventArgs e)
		{
			_gameTabController.ChangeTab(OtherTabItem);
		}

		private void GameSettingsTile_Click(object sender, RoutedEventArgs e)
		{
			_gameTabController.ChangeTab(GameSettingsTabItem);
		}

		private async void CheckButton_Click(object sender, RoutedEventArgs e)
		{
			await CheckGameFiles(UpdateMethod.FileCheck);
		}

		private async void OtherProxyConfig_Click(object sender, RoutedEventArgs e)
		{
			await ConfigProxy();
		}

		private async void OtherChangeGameDir_Click(object sender, RoutedEventArgs e)
		{
			await SelectGameFolder();
		}

		private void OtherOpenGameDir_Click(object sender, RoutedEventArgs e)
		{
			Process.Start($"file://{Settings.Default.GameFolder}");
		}

		private async void OtherInstallGame_Click(object sender, RoutedEventArgs e)
		{
			await SetupGameInfo();
		}

		private async void metroWindow_Loaded(object sender, RoutedEventArgs e)
		{
			_lastTop = Top;
			_lastLeft = Left;

			if (!Settings.Default.IsGameInstalled)
			{
				string gamefolder = GetTweakerGameFolder();

				if (string.IsNullOrWhiteSpace(gamefolder))
				{
					string workDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
					string pso2Path = Path.Combine("pso2.exe");

					if (File.Exists(pso2Path))
					{
						gamefolder = workDir;
					}
				}

				if (string.IsNullOrWhiteSpace(gamefolder))
				{
					await SetupGameInfo();
				}
				else
				{
					MessageDialogResult result = await this.ShowMessageAsync(Text.GameDetected, $"\"{gamefolder}\"", AffirmNeg, YesNo);

					if (result == MessageDialogResult.Affirmative)
					{
						Settings.Default.GameFolder = gamefolder;
						Settings.Default.IsGameInstalled = true;
					}
					else
					{
						await SetupGameInfo();
					}
				}
			}

			if (Settings.Default.IsGameInstalled)
			{
				if (!Directory.Exists(Settings.Default.GameFolder))
				{
					MessageDialogResult result = await this.ShowMessageAsync(Text.MissingFolder, Text.MovedOrDeleted, AffirmNeg, MovedDeleted);

					if (result == MessageDialogResult.Affirmative)
					{
						await SelectGameFolder();
					}
					else
					{
						Settings.Default.IsGameInstalled = false;
						Settings.Default.GameFolder = string.Empty;
						await SetupGameInfo();
					}
				}
			}

			if (Settings.Default.IsGameInstalled)
			{
				await Task.Run(() => CreateDirectoryIfNoneExists(game.PathProvider.GameConfigFolder));

				// UNDONE: move to PatchProvider
				// UNDONE: we're not doing anything with this???
				string editionPath = Path.Combine(Settings.Default.GameFolder, "edition.txt");

				if (!await game.IsGameUpToDate())
				{
					MessageDialogResult result = await this.ShowMessageAsync(Text.GameUpdate, Text.GameUpdateAvailable, AffirmNeg, YesNo);

					if (result == MessageDialogResult.Affirmative)
					{
						await CheckGameFiles(UpdateMethod.Update);
					}
				}

				if (await game.IsNewPrecedeAvailable() &&
					await this.ShowMessageAsync(Text.PrecedeAvailable, Text.DownloadLatestPreced, AffirmNeg, YesNo) == MessageDialogResult.Affirmative)
				{
					var precedeWindow = new PrecedeWindow(game)
					{
						Owner = this,
						Top = Top + Height,
						Left = Left
					};

					_isPrecedeDownloading = true;
					precedeWindow.Show();
					precedeWindow.Closed += delegate { _isPrecedeDownloading = false; };
				}
			}

			EnableGameButtions(Settings.Default.IsGameInstalled);
			Settings.Default.Save();
		}

		private void MetroWindow_LocationChanged(object sender, EventArgs e)
		{
			foreach (Window window in OwnedWindows)
			{
				window.Top += Top - _lastTop;
				window.Left += Left - _lastLeft;
			}

			_lastTop = Top;
			_lastLeft = Left;
		}

		private void PauseCheckButton_Click(object sender, RoutedEventArgs e)
		{
			PauseCheckButton.Content = Text.Pausing;
			_isCheckPaused = !_isCheckPaused;
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(e.Uri.ToString());
			e.Handled = true;
		}

		private void DownloadStarted(object sender, string e)
		{
			CurrentGeneralDownloadActionLabel.Dispatcher.InvokeAsync(() =>
			{
				CurrentGeneralDownloadActionLabel.Content = Path.GetFileNameWithoutExtension(e);
			});
		}

		private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			CheckDownloadProgressbar.Dispatcher.InvokeAsync(() =>
			{
				GeneralDownloadProgressbar.Value = e.ProgressPercentage;
				CurrentGeneralDownloadSizeActionLabel.Content = $"{SizeSuffix.GetSizeSuffix(e.BytesReceived)}/{SizeSuffix.GetSizeSuffix(e.TotalBytesToReceive)}";
			});
		}

		private async void LaunchButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (_isPrecedeDownloading && Settings.Default.CloseOnLaunch)
				{
					MessageDialogResult result = await this.ShowMessageAsync(Text.DownloadInProgress, Text.LaunchDownloadInProgress, AffirmNeg, YesNo);
					if (result != MessageDialogResult.Affirmative)
					{
						return;
					}
				}

				if (await game.IsInMaintenance())
				{
					MessageDialogResult result = await this.ShowMessageAsync(Text.ServerMaintenance, Text.GameIsDown, AffirmNeg, YesNo);
					if (result != MessageDialogResult.Affirmative)
					{
						return;
					}
				}

				if (await Task.Run(() => game.LaunchGame()) && Settings.Default.CloseOnLaunch && !_isPrecedeDownloading)
				{
					Close();
				}
			}
			catch (Exception ex)
			{
				await this.ShowMessageAsync(string.Empty, ex.Message);
			}
		}

		private void Color_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded)
			{
				Settings.Default.AccentColor = Colors.SelectedValue.ToString();
				ChangeAppStyle(Application.Current, GetAccent(Settings.Default.AccentColor), GetAppTheme(Settings.Default.Theme));
				Settings.Default.Save();
			}
		}

		private void Theme_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded)
			{
				Settings.Default.Theme = Themes.SelectedValue.ToString();
				ChangeAppStyle(Application.Current, GetAccent(Settings.Default.AccentColor), GetAppTheme(Settings.Default.Theme));
				Settings.Default.Save();
			}
		}

		private void EnhancementsTabItem_OnSelected(object sender, RoutedEventArgs e)
		{
		}

		private void EnhancementsTabItem_OnUnSelected(object sender, RoutedEventArgs e)
		{
		}

		private async void AddPluginButton_Click(object sender, RoutedEventArgs e)
		{
			_gameTabController.PreviousTab();
		}

		private void GameSettingsTabItem_OnSelected(object sender, RoutedEventArgs e)
		{
			psoSettings.Reload();

			// Math is used to map the Vsync values to indexes to remove the need for a Switch or an Array
			VsyncComboBox.SelectedIndex = (int)(psoSettings.Vsync / 140f * 5f);
			WindowModeComboBox.SelectedIndex = psoSettings.VirtualFullScreen ? 2 : ToInt32(psoSettings.FullScreen);
			MonitorPlaybackCheckBox.IsChecked = psoSettings.MoviePlay;
			TextureComboBox.SelectedIndex = psoSettings.TextureResolution;
			ShaderQualityCombobox.SelectedIndex = psoSettings.ShaderQuality;
			InterfaceSizeComboBox.SelectedIndex = psoSettings.InterfaceSize;
			MusicSlider.Value = psoSettings.Music;
			SoundSlider.Value = psoSettings.Sound;
			VoiceSlider.Value = psoSettings.Voice;
			VideoSlider.Value = psoSettings.Video;
			SurroundToggle.IsChecked = psoSettings.Surround;
			GlobalFocusToggle.IsChecked = psoSettings.GlobalFocus;

			string resolution = $"{psoSettings.WindowWidth}x{psoSettings.WindowHight}";
			UiResources.GetResolutions().Add(resolution);
			ResolutionsCombobox.SelectedItem = resolution;
		}

		private void GameSettingsTabItem_OnUnSelected(object sender, RoutedEventArgs e)
		{
			psoSettings.Save();
		}

		private void TextureComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded)
			{
				psoSettings.TextureResolution = TextureComboBox.SelectedIndex;
			}
		}

		private void ShaderQualityCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded)
			{
				psoSettings.ShaderQuality = ShaderQualityCombobox.SelectedIndex;
			}
		}

		private void VsyncComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded)
			{
				psoSettings.Vsync = ToInt32(((dynamic)VsyncComboBox.SelectedValue).Content.Replace("Off", "0"));
			}
		}

		private void WindowModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded)
			{
				if (WindowModeComboBox.SelectedIndex == 2)
				{
					psoSettings.FullScreen = false;
					psoSettings.VirtualFullScreen = true;
				}
				else
				{
					psoSettings.FullScreen = ToBoolean(WindowModeComboBox.SelectedIndex);
					psoSettings.VirtualFullScreen = false;
				}
			}
		}

		private void InterfaceSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded)
			{
				psoSettings.InterfaceSize = InterfaceSizeComboBox.SelectedIndex;
			}
		}

		private void MonitorPlaybackCheckBox_Checked(object sender, RoutedEventArgs e)
		{
			psoSettings.MoviePlay = true;
		}

		private void MonitorPlaybackCheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			psoSettings.MoviePlay = false;
		}

		private void ResolutionsCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded)
			{
				string[] resolution = ResolutionsCombobox.SelectedItem.ToString().Split('x');

				psoSettings.WindowWidth = ToInt32(resolution[0]);
				psoSettings.WindowHight = ToInt32(resolution[1]);
			}
		}

		private void MusicSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			psoSettings.Music = (int)MusicSlider.Value;
		}

		private void SoundSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			psoSettings.Sound = (int)SoundSlider.Value;
		}

		private void VoiceSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			psoSettings.Voice = (int)VoiceSlider.Value;
		}

		private void VideoSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			psoSettings.Video = (int)VideoSlider.Value;
		}

		private void SurroundToggle_Checked(object sender, RoutedEventArgs e)
		{
			psoSettings.Surround = true;
		}

		private void SurroundToggle_Unchecked(object sender, RoutedEventArgs e)
		{
			psoSettings.Surround = false;
		}

		private void GlobalFocusToggle_Checked(object sender, RoutedEventArgs e)
		{
			psoSettings.GlobalFocus = true;
		}

		private void GlobalFocusToggle_Unchecked(object sender, RoutedEventArgs e)
		{
			psoSettings.GlobalFocus = false;
		}

		#endregion

		#region Functions

		private void EnableGameButtions(bool isEnabled)
		{
			CheckButton.IsEnabled = isEnabled;
			LaunchButton.IsEnabled = isEnabled;
			GameSettingsTile.IsEnabled = isEnabled;
			//EnhancementsTile.IsEnabled = isEnabled;
			OtherProxyConfig.IsEnabled = isEnabled;
		}

		private async Task<bool> CheckGameFiles(UpdateMethod method)
		{
			_gameTabController.ChangeTab(FileCheckTabItem);

			await game.PatchListProvider.PullManagementData();

			_checkCancelSource = new CancellationTokenSource();
			_isCheckPaused = false;
			CompletedCheckDownloadActionsLabel.Content = string.Empty;
			CompletedCheckActionsLabel.Content = string.Empty;
			CurrentCheckActionLabel.Content = string.Empty;
			CurrentCheckDownloadActionLabel.Content = string.Empty;
			CurrentCheckSizeActionLabel.Content = string.Empty;
			PauseCheckButton.Content = Text.Pause;
			CheckDownloadProgressbar.Value = 0;
			CheckProgressbar.Value = 0;
			var numberDownloaded = 0;
			var numberToDownload = 0;
			var fileOperations = new List<Task>();

			long totalBytesToDownload = 0;
			long totalBytesDownloaded = 0;
			long lastBytesDownloaded = 0;

			using (var manager = new DownloadManager())
			{
				async Task pause()
				{
					var taskSource = new TaskCompletionSource<object>();
					void unpause(object s, RoutedEventArgs e)
					{
						taskSource.TrySetResult(null);
					}

					void cancel(object s, RoutedEventArgs e)
					{
						taskSource.TrySetCanceled();
					}

					try
					{
						PauseCheckButton.Click += unpause;
						CancelCheckButton.Click += cancel;
						manager.PauseDownloads(taskSource.Task);
						await taskSource.Task;
					}
					finally
					{
						PauseCheckButton.Click -= unpause;
						CancelCheckButton.Click -= cancel;
					}
				}

				try
				{
					await Task.Run(() =>
					{
						CreateDirectoryIfNoneExists(game.PathProvider.GameConfigFolder);
						CreateDirectoryIfNoneExists(game.PathProvider.DataFolder);
					});

					string precedePath = Path.Combine(game.PathProvider.PrecedeFolder, "data", "win32");

					if (File.Exists(game.PathProvider.PrecedeTxtPath) && Directory.Exists(precedePath))
					{
						// TODO: the main window should never be explicitly checking ManagementData for *anything*.
						if (!game.PatchListProvider.ManagementData.ContainsKey("PrecedeVersion") || !game.PatchListProvider.ManagementData.ContainsKey("PrecedeCurrent"))
						{
							MessageDialogResult result = await this.ShowMessageAsync(Text.ApplyPrecede, Text.ApplyPrecedeNow, AffirmNeg, YesNo);

							if (result == MessageDialogResult.Affirmative)
							{
								// TODO: not this
								CancelCheckButton.IsEnabled = false;
								PauseCheckButton.IsEnabled = false;

								string[] files = await Task.Run(() => Directory.GetFiles(precedePath));
								CheckProgressbar.Maximum = files.Length;

								foreach (string file in files)
								{
									CurrentCheckActionLabel.Content = Path.GetFileName(file);

									try
									{
										await Task.Run(() => MoveAndOverwriteFile(file, Path.Combine(game.PathProvider.DataFolder, Path.GetFileName(file ?? string.Empty))));
									}
									catch (Exception)
									{
										// ignored
									}

									CompletedCheckActionsLabel.Content = Text.ApplyingPrecede.Format(++CheckProgressbar.Value, CheckProgressbar.Maximum);
								}

								try
								{
									await Task.Run(() => Directory.Delete(game.PathProvider.PrecedeFolder, true));
								}
								catch (Exception)
								{
									await this.ShowMessageAsync(Text.Error, Text.PrecedeDeleteFailed);
								}

								// TODO: not this
								CancelCheckButton.IsEnabled = true;
								PauseCheckButton.IsEnabled = true;

								method = UpdateMethod.FileCheck;
							}
						}
					}

					string launcherList = await manager.DownloadStringTaskAsync(game.PatchListProvider.LauncherListUrl);
					string patchList = await manager.DownloadStringTaskAsync(game.PatchListProvider.PatchListUrl);
					string listAlways = await manager.DownloadStringTaskAsync(game.PatchListProvider.PatchListAlwaysUrl);

					PatchListEntry[] launcherListData = PatchListEntry.Parse(launcherList).ToArray();
					PatchListEntry[] patchListData = PatchListEntry.Parse(patchList).ToArray();
					PatchListEntry[] patchListAlways = PatchListEntry.Parse(listAlways).ToArray();

					if (method == UpdateMethod.Update && Directory.Exists(game.PathProvider.GameConfigFolder))
					{
						var entryComparer = new PatchListEntryComparer();

						if (File.Exists(game.PathProvider.LauncherListPath))
						{
							IEnumerable<PatchListEntry> storedLauncherList = await Task.Run(() => PatchListEntry.Parse(File.ReadAllText(game.PathProvider.LauncherListPath)));
							launcherListData = launcherListData.Except(storedLauncherList, entryComparer).ToArray();
						}

						if (File.Exists(game.PathProvider.PatchListPath))
						{
							IEnumerable<PatchListEntry> storedNewList = await Task.Run(() => PatchListEntry.Parse(File.ReadAllText(game.PathProvider.PatchListPath)));
							patchListData = patchListData.Except(storedNewList, entryComparer).ToArray();
						}

						if (File.Exists(game.PathProvider.PatchListAlwaysPath))
						{
							IEnumerable<PatchListEntry> storedAlwaysList = await Task.Run(() => PatchListEntry.Parse(File.ReadAllText(game.PathProvider.PatchListAlwaysPath)));
							patchListAlways = patchListAlways.Except(storedAlwaysList, entryComparer).ToArray();
						}

					}

					PatchListEntry[] lists = launcherListData.Concat(patchListData.Concat(patchListAlways)).ToArray();
					PatchListEntry[] groups = (from v in lists group v by v.Name into d select d.First()).ToArray();

					CheckProgressbar.Maximum = groups.Length;

					void setTopLabel()
					{
						CompletedCheckDownloadActionsLabel.Content = Text.DownloadedOf
							.Format(numberDownloaded, numberToDownload, SizeSuffix.GetSizeSuffix(totalBytesDownloaded), SizeSuffix.GetSizeSuffix(totalBytesToDownload));
					}

					manager.DownloadStarted += (s, e) =>
					{
						CurrentCheckDownloadActionLabel.Dispatcher.InvokeAsync(() =>
						{
							lastBytesDownloaded = 0;
							CheckDownloadProgressbar.Maximum = 100;
							CurrentCheckDownloadActionLabel.Content = Path.GetFileNameWithoutExtension(e);
						});
					};

					manager.DownloadProgressChanged += (s, e) =>
					{
						CheckDownloadProgressbar.Dispatcher.InvokeAsync(() =>
						{
							CheckDownloadProgressbar.Value = e.ProgressPercentage;

							totalBytesDownloaded += e.BytesReceived - lastBytesDownloaded;
							lastBytesDownloaded = e.BytesReceived;
							setTopLabel();

							CurrentCheckSizeActionLabel.Content = $"{SizeSuffix.GetSizeSuffix(e.BytesReceived)}/{SizeSuffix.GetSizeSuffix(e.TotalBytesToReceive)}";
						});
					};

					manager.DownloadCompleted += (s, e) =>
					{
						CheckDownloadProgressbar.Dispatcher.InvokeAsync(() =>
						{
							numberDownloaded++;
							setTopLabel();
						});
					};

					for (var index = 0; index < groups.Length;)
					{
						_checkCancelSource.Token.ThrowIfCancellationRequested();

						if (_isCheckPaused)
						{
							PauseCheckButton.Content = Text.Resume;
							await pause();
							PauseCheckButton.Content = Text.Pause;
						}
						else
						{
							PatchListEntry data = groups[index];
							CurrentCheckActionLabel.Content = Path.GetFileNameWithoutExtension(data.Name);
							string filePath = MakeLocalToGame(Path.ChangeExtension(data.Name, null));

							bool upToDate = await Task.Run(() => IsFileUpToDate(filePath, data.Size, data.Hash));
							CheckProgressbar.Value = ++index;
							CompletedCheckActionsLabel.Content = Text.CheckedOf.Format(index, groups.Length);

							if (upToDate)
							{
								continue;
							}

							string patPath = MakeLocalToGame(data.Name);
							Directory.CreateDirectory(Path.GetDirectoryName(patPath));

							void pat(Task t)
							{
								MoveAndOverwriteFile(patPath, filePath);
							}

							Uri fileUri;

							if (data.Source == PatchListSource.None)
							{
								var fakeSource = PatchListSource.Master;
								// HACK: Type check is a sloppy hack for NA -- all patches come from Patch on NA, not master. (As far as we can tell?)
								if (patchListData.Contains(data) || launcherListData.Contains(data) || game.GetType() == typeof(NorthAmericaWin10EditionManager))
								{
									fakeSource = PatchListSource.Patch;
								}

								fileUri = await game.PatchListProvider.BuildFileUri(fakeSource, data.Name);
							}
							else
							{
								fileUri = await game.PatchListProvider.BuildFileUri(data);
							}

							fileOperations.Add(manager.DownloadFileTaskAsync(fileUri, patPath).ContinueWith(pat));

							numberToDownload++;
							totalBytesToDownload += data.Size;

							setTopLabel();
						}
					}

					numberToDownload++;
					fileOperations.Add(manager.DownloadFileTaskAsync(game.PatchListProvider.VersionFileUrl, game.PathProvider.VersionFilePath));

					Task downloads = Task.WhenAll(fileOperations);

					while (!downloads.IsCompleted && !downloads.IsCanceled && !downloads.IsFaulted)
					{
						_checkCancelSource.Token.ThrowIfCancellationRequested();

						if (_isCheckPaused)
						{
							await pause();
						}
						else
						{
							// TODO: do we really want to do this? why not yield?
							await Task.Delay(16);
						}
					}

					await downloads;

					await Task.Run(() =>
					{
						File.WriteAllText(game.PathProvider.LauncherListPath, launcherList);
						File.WriteAllText(game.PathProvider.PatchListPath, patchList);
						File.WriteAllText(game.PathProvider.PatchListAlwaysPath, listAlways);

						if (File.Exists(game.PathProvider.VersionFilePath))
						{
							SetTweakerRemoteVersion(File.ReadAllText(game.PathProvider.VersionFilePath));
						}
					});
				}
				catch when (_checkCancelSource.IsCancellationRequested)
				{
					manager.CancelDownloads();
					_gameTabController.PreviousTab();
					return false;
				}

			}

			if (GameTabItem.IsSelected)
			{
				FlashWindow(this, true);

				// HACK: holy magic number bat man
				if (numberDownloaded > 4)
				{
					// HACK: holy magic number bat man
					await this.ShowMessageAsync(Text.Updated, Text.FilesDownloaded.Format(numberDownloaded - 4));
				}
				else
				{
					await this.ShowMessageAsync(Text.Complete, Text.AllFilesValid);
				}
			}

			_gameTabController.PreviousTab();

			return true;
		}

		public async Task<bool> ConfigProxy()
		{
			try
			{
				string url = await this.ShowInputAsync(Text.EnterProxy, string.Empty);

				if (url != null)
				{
					return await SetupProxy(url);
				}
			}
			catch (Exception ex)
			{
				await this.ShowMessageAsync(Text.Error, ex.Message);
			}

			return false;
		}

		public async Task<bool> SetupProxy(string url)
		{
			using (var client = new AquaHttpClient())
			{
				string json = await client.DownloadStringTaskAsync(new Uri(url));
				dynamic jsonData = JsonConvert.DeserializeObject(json);

				if (jsonData.version != 1)
				{
					await this.ShowMessageAsync(string.Empty, Text.BadProxyVersion);
					return false;
				}

				string hostAddress = (await Dns.GetHostAddressesAsync((string)jsonData.host))?.FirstOrDefault()?.ToString();
				string[] file = StripProxyEntries(File.ReadAllLines(HostsPath)).ToArray();
				IEnumerable<string> lines = file.Concat(new[]
				{
					"# Dogstar Proxy Start", $"{hostAddress} gs001.pso2gs.net # {jsonData.name} Ship 01", $"{hostAddress} gs016.pso2gs.net # {jsonData.name} Ship 02", $"{hostAddress} gs031.pso2gs.net # {jsonData.name} Ship 03", $"{hostAddress} gs046.pso2gs.net # {jsonData.name} Ship 04", $"{hostAddress} gs061.pso2gs.net # {jsonData.name} Ship 05", $"{hostAddress} gs076.pso2gs.net # {jsonData.name} Ship 06", $"{hostAddress} gs091.pso2gs.net # {jsonData.name} Ship 07", $"{hostAddress} gs106.pso2gs.net # {jsonData.name} Ship 08", $"{hostAddress} gs121.pso2gs.net # {jsonData.name} Ship 09", $"{hostAddress} gs136.pso2gs.net # {jsonData.name} Ship 10", "# Dogstar Proxy End"
				});

				File.WriteAllLines(HostsPath, lines);

				client.DownloadFileAsync(new Uri((string)jsonData.publickeyurl), Path.Combine(Settings.Default.GameFolder, "publickey.blob"));

				string gameHost = (await Dns.GetHostAddressesAsync("gs001.pso2gs.net"))?.FirstOrDefault()?.ToString();

				if (gameHost != hostAddress)
				{
					await this.ShowMessageAsync(Text.Failed, Text.ProxyRevert);
					File.WriteAllLines(HostsPath, file);
					return false;
				}

				return true;
			}
		}

		private async Task<bool> InstallGame(string path)
		{
			try
			{
				CreateDirectoryIfNoneExists(path);
				Settings.Default.GameFolder = path;

				if (await CheckGameFiles(UpdateMethod.FileCheck))
				{
					Settings.Default.IsGameInstalled = true;
					Settings.Default.Save();
				}

				return true;
			}
			catch (Exception)
			{
				Settings.Default.GameFolder = string.Empty;
				Settings.Default.IsGameInstalled = false;
				Settings.Default.Save();
			}

			return false;
		}

		private async Task SetupGameInfo()
		{
			MessageDialogResult result = await this.ShowMessageAsync(Text.HaveInstalled, string.Empty, AffirmNeg, YesNo);

			if (result != MessageDialogResult.Affirmative)
			{
				result = await this.ShowMessageAsync(Text.WouldYoulikeToInstall, string.Empty, AffirmNeg, YesNo);

				if (result == MessageDialogResult.Affirmative)
				{
					string path = await this.ShowFolderSelectAsync(string.Empty, Text.SelectInstallLocation, Properties.Resources.DefaultInstallDir);

					while (!string.IsNullOrWhiteSpace(path) && !await InstallGame(Path.Combine(path, "PHANTASYSTARONLINE2", "pso2_bin")))
					{
						if (await this.ShowMessageAsync(string.Empty, Uri.IsWellFormedUriString(path, UriKind.Absolute) ? Text.InvalidPathTryAgain : Text.GoneWrong, AffirmNeg, YesNo) == MessageDialogResult.Negative)
						{
							break;
						}

						path = await this.ShowFolderSelectAsync(string.Empty, Text.SelectInstallLocation, Properties.Resources.DefaultInstallDir);
					}
				}
			}
			else
			{
				await SelectGameFolder();
			}

			EnableGameButtions(Settings.Default.IsGameInstalled);
		}

		private async Task SelectGameFolder()
		{
			var result = MessageDialogResult.Affirmative;

			while (result == MessageDialogResult.Affirmative)
			{
				string gamePath = await this.ShowFileSelectAsync(string.Empty, Text.SelectExe, Properties.Resources.GameFilter);

				if (await Task.Run(() => !string.IsNullOrWhiteSpace(gamePath) && File.Exists(gamePath)))
				{
					Settings.Default.GameFolder = Path.GetDirectoryName(gamePath);
					Settings.Default.IsGameInstalled = true;
					Settings.Default.Save();
					EnableGameButtions(Settings.Default.IsGameInstalled);
					result = MessageDialogResult.Negative;
				}
				else if (gamePath == null)
				{
					result = MessageDialogResult.Negative;
				}
				else
				{
					result = await this.ShowMessageAsync(string.Empty, Text.InvalidPathTryAgain, AffirmNeg, YesNo);
				}
			}
		}

		public void Dispose()
		{
			_generalDownloadManager.Dispose();
		}
		#endregion Functions


	}
}
