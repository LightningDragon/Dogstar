using System.Collections.Generic;
using System.Windows.Controls;

namespace Dogstar
{
	class TabController
	{
		private readonly TabControl _tabControl;
		private readonly Stack<TabItem> _tabHistory = new Stack<TabItem>();

		public TabController(TabControl tabControl)
		{
			_tabControl = tabControl;
		}

		public void ChangeTab(TabItem tab)
		{
			if (tab != null && _tabControl.Items.Contains(tab))
			{
				_tabHistory.Push((TabItem)_tabControl.SelectedItem);
				tab.IsSelected = true;
			}
		}

		public void PreviousTab()
		{
			if (_tabHistory.Count > 0)
			{
				_tabHistory.Pop().IsSelected = true;
			}
		}
	}
}
