using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

namespace DogStar
{
	class DownloadManager : IDisposable
	{
		private readonly WebClient _client = new WebClient();
		private readonly Queue<Action> _actions = new Queue<Action>();

		public event EventHandler DownloadCompleted = delegate { };
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
		}

		private Task Noot(Task task)
		{
			lock (_actions)
			{
				if (_actions.Count > 0)
				{
					return Task.Run(_actions.Dequeue()).ContinueWith(Shit);
				}

				return Task.Delay(0);
			}
		}

		private void Shit(Task task)
		{
			task.ContinueWith(Noot);
		}

		public Task<byte[]> DownloadDataTaskAsync(Uri address)
		{
			lock (_actions)
			{
				if (_client.IsBusy)
				{
					var taskSource = new TaskCompletionSource<byte[]>();
					_actions.Enqueue(async () => taskSource.TrySetResult(await _client.DownloadDataTaskAsync(address)));
					return taskSource.Task;
				}
				else
				{
					var task = _client.DownloadDataTaskAsync(address);
					Task.Run(() => Shit(task));
					return task;
				}
			}
		}

		public Task<string> DownloadStringTaskAsync(Uri address)
		{
			lock (_actions)
			{
				if (_client.IsBusy)
				{
					var taskSource = new TaskCompletionSource<string>();
					_actions.Enqueue(async () => taskSource.TrySetResult(await _client.DownloadStringTaskAsync(address)));
					return taskSource.Task;
				}
				else
				{
					var task = _client.DownloadStringTaskAsync(address);
					Task.Run(() => Shit(task));
					return task;
				}
			}
		}

		public Task DownloadFileTaskAsync(Uri address, string fileName)
		{
			lock (_actions)
			{
				if (_client.IsBusy)
				{
					var taskSource = new TaskCompletionSource<object>();
					_actions.Enqueue(async () => await _client.DownloadFileTaskAsync(address, fileName).ContinueWith(x => taskSource.TrySetResult(null)));
					return taskSource.Task;
				}
				else
				{
					var task = _client.DownloadFileTaskAsync(address, fileName);
					Task.Run(() => Shit(task));
					return task;
				}
			}
		}

		public void Dispose()
		{
			_client.Dispose();
		}
	}
}
