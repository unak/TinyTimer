﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace TinyTimer
{
    public partial class FormMain : Form
    {
        MyTimer timer;

        private bool moving = false;
        private Point startPos;
        private Point lastPos;

        private string title = null;

        public FormMain()
        {
            InitializeComponent();

            timer = new MyTimer((bool over)=>{
                if (over)
                {
                    soundWorker.RunWorkerAsync();
                }
                SetTimerText();
            });

            lblTimer.BackColor = Color.LightGray;
            SetTimerText();
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCLBUTTONDBLCLK = 0xA3;
            const int HTCAPTION = 2;
            if (m.Msg == WM_NCLBUTTONDBLCLK && m.WParam.ToInt32() == HTCAPTION)
            {
                var dialog = new FormInputDialog(title);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    title = dialog.Title;
                    if (string.IsNullOrEmpty(title))
                    {
                        this.Text = "TinyTimer";
                        title = null;
                    }
                    else
                    {
                        this.Text = string.Format("{0} - TinyTimer", title);
                    }
                    tipForm.SetToolTip(lblTimer, title);
                }
                dialog.Dispose();
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        private void FormMain_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = false;
            if (!timer.Running)
            {
                if (e.KeyChar >= '0' && e.KeyChar <= '9' && timer.Hours < 10)
                {
                    timer.IsMinus = false;
                    timer.Hours = timer.Hours * 10 + timer.Minutes / 10;
                    timer.Minutes = timer.Minutes % 10 * 10 + timer.Seconds / 10;
                    timer.Seconds = timer.Seconds % 10 * 10 + e.KeyChar - '0';

                    e.Handled = true;
                }
                else if (e.KeyChar == '\b')
                {
                    timer.IsMinus = false;
                    timer.Seconds = timer.Seconds / 10 + timer.Minutes % 10 * 10;
                    timer.Minutes = timer.Minutes / 10 + timer.Hours % 10 * 10;
                    timer.Hours /= 10;

                    e.Handled = true;
                }

                if (e.Handled)
                {
                    SetTimerText();
                    btnStart.Focus();
                }
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            soundWorker.CancelAsync();
        }

        private void FormMain_Activated(object sender, EventArgs e)
        {
            if (timer.Running)
            {
                lblTimer.BackColor = Color.Yellow;
            }
            else
            {
                lblTimer.BackColor = Color.Khaki;
            }
        }

        private void FormMain_Deactivate(object sender, EventArgs e)
        {
            if (timer.Running)
            {
                lblTimer.BackColor = SystemColors.Window;
            }
            else
            {
                lblTimer.BackColor = Color.LightGray;
            }
        }

        private void lblTimer_DoubleClick(object sender, EventArgs e)
        {
            if (this.FormBorderStyle == FormBorderStyle.FixedSingle)
            {
                this.FormBorderStyle = FormBorderStyle.None;
                lblTimer.BorderStyle = BorderStyle.FixedSingle;
                this.Height = lblTimer.Height;
            }
            else
            {
                this.Height = lblTimer.Height + btnStart.Height;
                lblTimer.BorderStyle = BorderStyle.Fixed3D;
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
            }
        }

        private void lblTimer_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && this.FormBorderStyle == FormBorderStyle.None)
            {
                moving = true;
                startPos = lastPos = Control.MousePosition;
            }
        }

        private void lblTimer_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && moving)
            {
                moving = false;
            }
        }

        private void lblTimer_MouseCaptureChanged(object sender, EventArgs e)
        {
            if (moving && !this.Capture)
            {
                moving = false;
                this.Left = startPos.X;
                this.Top = startPos.Y;
            }
        }

        private void lblTimer_MouseMove(object sender, MouseEventArgs e)
        {
            if (moving)
            {
                Point curPos = Control.MousePosition;
                this.Left += curPos.X - lastPos.X;
                this.Top += curPos.Y - lastPos.Y;
                lastPos = curPos;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (timer.Running)
            {
                // stopping
                timer.Stop();
                lblTimer.BackColor = Color.LightGray;
                btnStart.Text = "Start";
                btnStart.BackColor = Color.DeepSkyBlue;
                soundWorker.CancelAsync();
            }
            else if (timer.IsMinus || timer.Hours != 0 || timer.Minutes != 0 || timer.Seconds != 0)
            {
                // starting
                SetTimerText();

                timer.Start();
                lblTimer.BackColor = Color.Yellow;
                btnStart.Text = "Stop";
                btnStart.BackColor = Color.OrangeRed;
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            timer.Clear();
            lblTimer.BackColor = Color.LightGray;
            btnStart.Text = "Start";
            btnStart.BackColor = Color.DeepSkyBlue;
            SetTimerText();
            soundWorker.CancelAsync();
        }

        private void soundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var player = new SoundPlayer(@"C:\Windows\Media\ringout.wav");
            while (!soundWorker.CancellationPending)
            {
                player.PlaySync();
            }
        }

        private void SetTimerText()
        {
            lblTimer.ForeColor = timer.IsMinus ? Color.Red : Color.Black;
            lblTimer.Text = string.Format("{0}:{1:d2}:{2:d2}", timer.Hours, timer.Minutes, timer.Seconds);
        }
    }
}
