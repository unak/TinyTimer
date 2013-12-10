using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Media;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TinyTimer
{
    public partial class FormMain : Form
    {
        private bool minus = false;
        private int hours = 0;
        private int minutes = 0;
        private int seconds = 0;
        private int msec = 0;
        private DateTime start;

        private bool moving;
        private Point startPos;
        private Point lastPos;

        public FormMain()
        {
            InitializeComponent();
            minus = false;
            hours = minutes = seconds = msec = 0;
            SetTimerText();
        }

        private void FormMain_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = false;
            if (!timer.Enabled)
            {
                if (e.KeyChar >= '0' && e.KeyChar <= '9' && hours < 10)
                {
                    minus = false;
                    hours = hours * 10 + minutes / 10;
                    minutes = minutes % 10 * 10 + seconds / 10;
                    seconds = seconds % 10 * 10 + e.KeyChar - '0';
                    msec = 0;

                    e.Handled = true;
                }
                else if (e.KeyChar == '\b')
                {
                    minus = false;
                    msec = 0;
                    seconds = seconds / 10 + minutes % 10 * 10;
                    minutes = minutes / 10 + hours % 10 * 10;
                    hours /= 10;

                    e.Handled = true;
                }

                if (e.Handled)
                {
                    SetTimerText(false);
                    btnStart.Focus();
                }
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            soundWorker.CancelAsync();
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
            if (timer.Enabled)
            {
                // stopping
                timer.Enabled = false;
                btnStart.Text = "Start";
                btnStart.BackColor = Color.DeepSkyBlue;
                soundWorker.CancelAsync();
                seconds += (!minus && msec > 0) ? 1 : 0;
                msec = 0;
            }
            else if (hours != 0 || minutes != 0 || seconds != 0)
            {
                // starting
                msec = 0;
                minutes += seconds / 60;
                seconds %= 60;
                hours += minutes / 60;
                minutes %= 60;

                SetTimerText();

                start = DateTime.Now;
                timer.Enabled = true;
                btnStart.Text = "Stop";
                btnStart.BackColor = Color.OrangeRed;
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            timer.Enabled = false;
            btnStart.Text = "Start";
            btnStart.BackColor = Color.DeepSkyBlue;
            minus = false;
            hours = minutes = seconds = msec = 0;
            SetTimerText();
            soundWorker.CancelAsync();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            TimeSpan span = new TimeSpan(now.Ticks - start.Ticks);
            if (span.Milliseconds > 0)
            {
                start = now;
                if (!minus && hours * (60 * 60 * 1000) + minutes * 60 * 1000 + seconds * 60 * 1000 + msec < span.Milliseconds)
                {
                    minus = true;
                    msec = span.Milliseconds - (hours * (60 * 60 * 1000) + minutes * 60 * 1000 + seconds * 1000 + msec);
                    seconds = msec / 1000;
                    msec %= 1000;
                    minutes = seconds / 60;
                    seconds %= 60;
                    hours = minutes / 60;
                    minutes %= 60;

                    soundWorker.RunWorkerAsync();
                }
                else if (minus)
                {
                    msec += span.Milliseconds;
                    while (msec >= 1000)
                    {
                        msec -= 1000;
                        seconds++;
                    }
                    while (seconds >= 60)
                    {
                        seconds -= 60;
                        minutes++;
                    }
                    while (minutes >= 60)
                    {
                        minutes -= 60;
                        hours++;
                    }
                }
                else
                {
                    msec -= span.Milliseconds;
                    while (msec < 0)
                    {
                        msec += 1000;
                        seconds--;
                    }
                    while (seconds < 0)
                    {
                        seconds += 60;
                        minutes--;
                    }
                    while (minutes < 0)
                    {
                        minutes += 60;
                        hours--;
                    }
                }

                SetTimerText();
            }
        }

        private void soundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var player = new SoundPlayer(@"C:\Windows\Media\ringout.wav");
            while (!soundWorker.CancellationPending)
            {
                player.PlaySync();
            }
        }

        private void SetTimerText(bool regularize)
        {
            lblTimer.ForeColor = minus ? Color.Red : Color.Black;
            if (regularize)
            {
                minutes += seconds / 60;
                seconds %= 60;
                hours += minutes / 60;
                minutes %= 60;
            }
            int sec = seconds;
            int min = minutes;
            int h = hours;
            if (regularize)
            {
                sec += (!minus && msec > 0) ? 1 : 0;
                min += sec / 60;
                sec %= 60;
                h += min / 60;
                min %= 60;
            }
            lblTimer.Text = string.Format("{0}:{1:d2}:{2:d2}", h, min, sec);
        }

        private void SetTimerText()
        {
            SetTimerText(true);
        }
    }
}
