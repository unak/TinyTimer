using System;
using System.Windows.Forms;

namespace TinyTimer
{
    class MyTimer
    {
        public delegate void TimerHandler(bool over);

        public bool IsMinus = false;
        private int hours = 0;
        private int minutes = 0;
        private int seconds = 0;
        private int msec = 0;

        private DateTime start;
        private Timer timer;
        private TimerHandler handler;

        public int Hours
        {
            set
            {
                hours = value;
            }
            get
            {
                return hours + (!IsMinus && msec > 0 && seconds >= 59 && minutes >= 59 ? 1 : 0);
            }
        }

        public int Minutes
        {
            set
            {
                minutes = value;
            }
            get
            {
                return minutes + (!IsMinus && msec > 0 && seconds >= 59 ? minutes >= 59 ? -59 : 1 : 0);
            }
        }

        public int Seconds
        {
            set
            {
                seconds = value;
                msec = 0;
            }
            get
            {
                return seconds + (!IsMinus && msec > 0 ? seconds >= 59 ? -59 : 1 : 0);
            }
        }

        public bool Running
        {
            get
            {
                return timer.Enabled;
            }
        }

        public MyTimer(TimerHandler handler)
        {
            timer = new Timer();
            timer.Tick += new EventHandler(Tick);
            this.handler = handler;
        }

        public void Start()
        {
            msec = 0;
            Regularize();
            if (handler != null)
            {
                handler(false);
            }
            start = DateTime.Now;
            timer.Enabled = true;
        }

        public void Stop()
        {
            timer.Enabled = false;
            if (!IsMinus && msec > 0)
            {
                seconds += 1;
                msec = 0;
                Regularize();
            }
        }

        public void Clear()
        {
            Stop();
            IsMinus = false;
            hours = minutes = seconds = msec = 0;
        }

        public void Regularize()
        {
            if (msec >= 1000)
            {
                seconds += msec / 1000;
                msec %= 1000;
            }
            else if (msec < 0)
            {
                seconds -= -msec / 1000 + 1;
                msec = 1000 - -msec % 1000;
            }

            if (seconds >= 60)
            {
                minutes += seconds / 60;
                seconds %= 60;
            }
            else if (seconds < 0)
            {
                minutes -= -seconds / 60 + 1;
                seconds = 60 - -seconds % 60;
            }

            if (minutes >= 60)
            {
                hours += minutes / 60;
                minutes %= 60;
            }
            else if (minutes < 0)
            {
                hours -= -minutes / 60 + 1;
                minutes = 60 - -minutes % 60;
            }
        }

        private void Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            TimeSpan span = new TimeSpan(now.Ticks - start.Ticks);
            if (span.Milliseconds > 0)
            {
                var over = false;
                start = now;
                if (!IsMinus && hours * (60 * 60 * 1000) + minutes * 60 * 1000 + seconds * 60 * 1000 + msec < span.Milliseconds)
                {
                    over = true;
                    IsMinus = true;
                    msec = span.Milliseconds - (hours * (60 * 60 * 1000) + minutes * 60 * 1000 + seconds * 1000 + msec);
                    Regularize();
                }
                else
                {
                    if (IsMinus)
                    {
                        msec += span.Milliseconds;
                    }
                    else
                    {
                        msec -= span.Milliseconds;
                    }
                    Regularize();
                }

                if (handler != null)
                {
                    handler(over);
                }
            }
        }
    }
}
