using System;
using System.Diagnostics;
using System.Windows.Forms;
using keeponscreener.Properties;
using System.Drawing;

namespace keeponscreener
{
	/// <summary>
	/// 
	/// </summary>
	class ContextMenus
	{
		/// <summary>
		/// Is the About box displayed?
		/// </summary>
		bool isAboutLoaded = false;

		/// <summary>
		/// Creates this instance.
		/// </summary>
		/// <returns>ContextMenuStrip</returns>
		public ContextMenuStrip Create()
		{
			// Add the default menu options.
			ContextMenuStrip menu = new ContextMenuStrip();
			ToolStripMenuItem item;
			ToolStripSeparator sep;

            // a check box option
            item = new ToolStripMenuItem();
            item.Name = "hwnd_spanscreen";
            item.Text = "Allow windows to span screens";
            item.CheckOnClick = true;
            item.Checked = false;
            item.Click += new EventHandler(spanscreen_click);
            menu.Items.Add(item);

            // a check box option
            item = new ToolStripMenuItem();
            item.Name = "hwnd_offscreen";
            item.Text = "Allow windows off screen";
            item.CheckOnClick = true;
            item.Checked = true;
            item.Click += new EventHandler(offscreen_click);
            menu.Items.Add(item);

            // Separator.
			sep = new ToolStripSeparator();
			menu.Items.Add(sep);

			// Exit.
			item = new ToolStripMenuItem();
            item.Name = "exit";
			item.Text = "Exit";
			item.Click += new System.EventHandler(Exit_Click);
			menu.Items.Add(item);

			return menu;
		}


        void spanscreen_click(object sender, EventArgs e)
        {
            //should run a routine to check if any windows are spanning the screen
            //enumerate all non-hidden hwnds
            //
            return;
        }

        void offscreen_click(object sender, EventArgs e)
		{
            //should run a routine to gather all windows on whatever screen they are closest to
            //enumerate all non-hidden hwnds
            //
            return;
		}


		/// <summary>
		/// Handles the Click event of the About control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		void About_Click(object sender, EventArgs e)
		{
			if (!isAboutLoaded)
			{
				isAboutLoaded = true;
				new AboutBox().ShowDialog();
				isAboutLoaded = false;
			}
		}

		/// <summary>
		/// Processes a menu item.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		void Exit_Click(object sender, EventArgs e)
		{
			// Quit without further ado.
            Application.Exit();
		}
	}
}