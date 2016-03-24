using System.Windows;
using DogStar.Properties;

namespace DogStar
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
		void Application_Startup(object sender, StartupEventArgs e)
		{
			if (Settings.Default.UpgradeCheck)
			{
				Settings.Default.Upgrade();
				Settings.Default.UpgradeCheck = false;
				Settings.Default.Save();
			}

			foreach (var arg in e.Args)
			{
				switch (arg)
				{
					case "-pso2":
						Helper.LaunchGame();
						Shutdown();
						break;
				}
			}
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{
			if (e.ApplicationExitCode == 0)
			{
				Settings.Default.Save();
			}
		}
	}
}
