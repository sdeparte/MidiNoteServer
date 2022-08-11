using System;
using System.Collections.Generic;
using System.Threading;

namespace MidiNoteServer.Service
{
    class InputPerSecondService
    {
        private readonly double _floatingInterval;
        private readonly double _threshold;

        private readonly List<DateTime> _inputs = new List<DateTime>();

        private bool _thresholdRaised = false;

        private readonly Timer _timer;

        public event EventHandler<bool> ThresholdRaised;

        public InputPerSecondService(double floatingInterval, double threshold)
        {
            _floatingInterval = floatingInterval;
            _threshold = threshold;

            _timer = new Timer(ControllerTimerCallback, null, 0, 250);
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public void AddInput()
        {
            _inputs.Add(DateTime.Now);
        }

        private void ControllerTimerCallback(object source)
        {
            DateTime now = DateTime.Now;

            for (int i = _inputs.Count - 1; i >= 0; i--)
            {
                DateTime datetime = _inputs[i];

                if ((now - datetime).TotalSeconds > _floatingInterval)
                {
                    _inputs.RemoveRange(0, i + 1);
                    break;
                }
            }

            double ips = _inputs.Count / _floatingInterval;

            if (ips > _threshold && !_thresholdRaised)
            {
                _thresholdRaised = true;
                Console.WriteLine($" [{DateTime.Now}] -- InputPerSecond : Threshod raised : {_thresholdRaised}");
                ThresholdRaised?.Invoke(this, _thresholdRaised);
            }
            else if (ips <= _threshold && _thresholdRaised)
            {
                _thresholdRaised = false;
                Console.WriteLine($" [{DateTime.Now}] -- InputPerSecond : Threshod raised : {_thresholdRaised}");
                ThresholdRaised?.Invoke(this, _thresholdRaised);
            }
        }
    }
}
