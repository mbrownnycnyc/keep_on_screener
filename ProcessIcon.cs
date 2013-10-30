using System;
using System.Diagnostics;
using System.Windows.Forms;
using keeponscreener.Properties;

namespace keeponscreener
{
	/// <summary>
	/// 
	/// </summary>
	class ProcessIcon : IDisposable
	{
		/// <summary>
		/// The NotifyIcon object.
		/// </summary>
		NotifyIcon ni;

		/// <summary>
		/// Initializes a new instance of the <see cref="ProcessIcon"/> class.
		/// </summary>
		public ProcessIcon()
		{
			// Instantiate the NotifyIcon object.
			ni = new NotifyIcon();
		}

		/// <summary>
		/// Displays the icon in the system tray.
		/// </summary>
		public void Display()
		{
			// Put the icon in the system tray and allow it react to mouse clicks.			
			ni.MouseClick += new MouseEventHandler(ni_MouseClick);
            ni.Icon = Resources.keeponscreener;
			ni.Text = "keep on screener";
			ni.Visible = true;

			// Attach a context menu.
			ni.ContextMenuStrip = new ContextMenus().Create();
		}

        public bool GetMenuItemCheckedStatus(string itemname)
        {
            ContextMenuStrip contextmenu = new ContextMenuStrip();
            contextmenu = ni.ContextMenuStrip;
            ToolStripItem[] results;
            results = contextmenu.Items.Find(itemname,true);
            ToolStripMenuItem tsmi = (ToolStripMenuItem)results[0];
            if (tsmi.Checked == true)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		public void Dispose()
		{
			// When the application closes, this will remove the icon from the system tray immediately.
			ni.Dispose();
		}

		/// <summary>
		/// Handles the MouseClick event of the ni control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		void ni_MouseClick(object sender, MouseEventArgs e)
		{
			// Handle mouse button clicks.
			if (e.Button == MouseButtons.Left)
			{
				// Start Windows Explorer.
				//Process.Start("explorer", null);
			}
		}
	}
}