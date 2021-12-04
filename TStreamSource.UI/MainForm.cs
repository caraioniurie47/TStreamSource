#region LICENSE

//  TStreamSource - MPEG Stream Tools
//  Copyright (C) 2011 Iurie Caraion (caraioniurie47@gmail.com)
//  https://github.com/caraioniurie47/TStreamSource

//  This library is free software; you can redistribute it and/or
//  modify it under the terms of the GNU Lesser General Public
//  License as published by the Free Software Foundation; either
//  version 3 of the License, or (at your option) any later version.

//  This library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//  Lesser General Public License for more details.

//  You should have received a copy of the GNU Lesser General
//  Public License along with this library; 
//  If not, see <http://www.gnu.org/licenses/>.

#endregion

namespace TStreamSource.UI
{
    #region Usings

    using System;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    using NLog;

    #endregion

    public partial class MainForm : Form
    {
        #region Logger

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion

        #region Fields

        private VideoPlay videoPlay;

        #endregion

        #region Constructors

        public MainForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Event Handlers

        #region BtnStartClick

        private void BtnStartClick(object sender, EventArgs e)
        {
            if (videoPlay == null)
            {
                videoPlay = new VideoPlay(panelWindow);
                videoPlay.Start();

                btnStart.Text = "Stop";
            }
            else
            {
                btnStart.Text = "Start";

                videoPlay.Dispose();
                videoPlay = null;
            }
        }

        #endregion

        #region MainFormFormClosing

        private void MainFormFormClosing(object sender, FormClosingEventArgs e)
        {
            if (videoPlay != null)
            {
                videoPlay.Dispose();
                videoPlay = null;
            }
        }

        #endregion

        #endregion
    }
}
