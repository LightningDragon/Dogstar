using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

namespace Dogstar
{
	public class DownloadManager : IDisposable
	{
		// TODO: OnError event
		// TODO: Retry download on connection loss (forever)

		private readonly AquaHttpClient _client = new AquaHttpClient();
		private readonly Queue<Action> _actions = new Queue<Action>();

		private bool _isDownloading;
		private Task _paused = Task.Delay(0);

		public event EventHandler DownloadCompleted = delegate { };
		public event EventHandler<string> DownloadStarted = delegate { };
		public event DownloadDataCompletedEventHandler DownloadDataCompleted = delegate { };
		public event DownloadStringCompletedEventHandler DownloadStringCompleted = delegate { };
		public event AsyncCompletedEventHandler DownloadFileCompleted = delegate { };
		public event DownloadProgressChangedEventHandler DownloadProgressChanged = delegate { };

		public DownloadManager()
		{
			_client.DownloadProgressChanged += (s, e) => DownloadProgressChanged(s, e);
			_client.DownloadDataCompleted += (s, e) => DownloadDataCompleted(s, e);
			_client.DownloadStringCompleted += (s, e) => DownloadStringCompleted(s, e);
			_client.DownloadFileCompleted += (s, e) => DownloadFileCompleted(s, e);
			_client.DownloadDataCompleted += (s, e) => DownloadCompleted(s, e);
			_client.DownloadStringCompleted += (s, e) => DownloadCompleted(s, e);
			_client.DownloadFileCompleted += (s, e) => DownloadCompleted(s, e);
			DownloadCompleted += ContinueOnComplete;
		}

		private void ContinueOnComplete(object sender, EventArgs args)
		{
			try
			{
				lock (_actions)
				{
					if (_actions.Count > 0)
					{
						_paused.Wait();
						Task.Run(_actions.Dequeue());
					}
					else
					{
						_isDownloading = false;
					}
				}
			}
			catch
			{
				CancelDownloads();
			}
		}

		public void PauseDownloads(Task task)
		{
			lock (_actions)
			{
				_paused = task;
			}
		}

		public void CancelDownloads()
		{
			lock (_actions)
			{
				_actions.Clear();
				_client.CancelAsync();
			}
		}

		public Task<byte[]> DownloadDataTaskAsync(Uri address)
		{
			lock (_actions)
			{
				var taskSource = new TaskCompletionSource<byte[]>();

				Action action = async () =>
				{
					await Task.Run(() => DownloadStarted(this, address.ToString()));
					taskSource.TrySetResult(await _client.DownloadDataTaskAsync(address));
				};

				if (_actions.Count > 0 || _client.IsBusy || _isDownloading)
				{
					_actions.Enqueue(action);
				}
				else
				{
					_isDownloading = true;
					Task.Run(() => action());
				}

				return taskSource.Task;
			}
		}

		public Task<string> DownloadStringTaskAsync(Uri address)
		{
			lock (_actions)
			{
				var taskSource = new TaskCompletionSource<string>();

				Action action = async () =>
				{
					await Task.Run(() => DownloadStarted(this, address.ToString()));
					taskSource.TrySetResult(await _client.DownloadStringTaskAsync(address));
				};

				if (_actions.Count > 0 || _client.IsBusy || _isDownloading)
				{
					_actions.Enqueue(action);
				}
				else
				{
					_isDownloading = true;
					Task.Run(() => action());
				}

				return taskSource.Task;
			}
		}

		public Task DownloadFileTaskAsync(Uri address, string fileName)
		{
			lock (_actions)
			{
				var taskSource = new TaskCompletionSource<object>();

				Action action = async () =>
				{
					await Task.Run(() => DownloadStarted(this, address.ToString()));
					await _client.DownloadFileTaskAsync(address, fileName).ContinueWith(x => taskSource.TrySetResult(null));
				};

				if (_actions.Count > 0 || _client.IsBusy || _isDownloading)
				{
					_actions.Enqueue(action);
				}
				else
				{
					_isDownloading = true;
					Task.Run(() => action());
				}

				return taskSource.Task;
			}
		}

		public void Dispose()
		{
			_client.Dispose();
		}
	}
}
