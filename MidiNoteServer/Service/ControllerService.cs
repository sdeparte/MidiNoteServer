using SharpDX.XInput;
using System;
using System.Threading;

namespace MidiNoteServer.Service
{
    class ControllerService
    {
        private const int deadband = 2500;

        private readonly MidiController _midiController;
        private readonly Controller _controller;
        private readonly Timer _timer;
        private readonly InputPerSecondService _inputPerSecondService;

        private bool _lastStateRightThumb = false;

        private bool _lastStateButtonA = false;
        private bool _lastStateButtonB = false;
        private bool _lastStateButtonX = false;
        private bool _lastStateButtonY = false;

        private bool _lastStateRightThumbXPositive = false;
        private bool _lastStateRightThumbXNegative = false;
        private bool _lastStateRightThumbYPositive = false;
        private bool _lastStateRightThumbYNegative = false;

        private bool _lastStateButtonRightShoulder = false;
        private bool _lastStateButtonRightTrigger = false;

        private bool _lastStateLeftThumb = false;

        private bool _lastStateButtonDPadUp = false;
        private bool _lastStateButtonDPadDown = false;
        private bool _lastStateButtonDPadLeft = false;
        private bool _lastStateButtonDPadRight = false;

        private bool _lastStateLeftThumbXPositive = false;
        private bool _lastStateLeftThumbXNegative = false;
        private bool _lastStateLeftThumbYPositive = false;
        private bool _lastStateLeftThumbYNegative = false;

        private bool _lastStateButtonLeftShoulder = false;
        private bool _lastStateButtonLeftTrigger = false;

        public ControllerService(MidiController midiController)
        {
            _midiController = midiController;

            Controller[] controllers = new[] {
                new Controller(UserIndex.One),
                new Controller(UserIndex.Two),
                new Controller(UserIndex.Three),
                new Controller(UserIndex.Four)
            };

            foreach (var selectControler in controllers)
            {
                if (selectControler.IsConnected)
                {
                    _controller = selectControler;
                    break;
                }
            }

            if (_controller == null)
            {
                throw new Exception("No XInput controller installed");
            }

            _inputPerSecondService = new InputPerSecondService(5.0, 4.0);
            _inputPerSecondService.ThresholdRaised += InputPerSecondService_ThresholdRaised;

            _timer = new Timer(ControllerTimerCallback, null, 0, 50);
        }

        public void Dispose()
        {
            _inputPerSecondService?.Dispose();
            _timer?.Dispose();
        }


        private void ControllerTimerCallback(Object source)
        {
            Gamepad gamepad = _controller.GetState().Gamepad;

            GamepadButtonFlags buttonFlags = gamepad.Buttons;

            Point leftThumb = new Point(
                x: (Math.Abs((float)gamepad.LeftThumbX) < deadband) ? 0 : (float)gamepad.LeftThumbX / short.MinValue * -100,
                y: (Math.Abs((float)gamepad.LeftThumbY) < deadband) ? 0 : (float)gamepad.LeftThumbY / short.MaxValue * 100
            );

            Point rightThumb = new Point(
                x: (Math.Abs((float)gamepad.RightThumbX) < deadband) ? 0 : (float)gamepad.RightThumbX / short.MaxValue * 100,
                y: (Math.Abs((float)gamepad.RightThumbY) < deadband) ? 0 : (float)gamepad.RightThumbY / short.MaxValue * 100
            );

            TestButtonState(rightThumb.X != 0 || rightThumb.Y != 0, ref _lastStateRightThumb, 120, false);

            TestButtonState(buttonFlags.HasFlag(GamepadButtonFlags.A), ref _lastStateButtonA, 100);
            TestButtonState(buttonFlags.HasFlag(GamepadButtonFlags.B), ref _lastStateButtonB, 101);
            TestButtonState(buttonFlags.HasFlag(GamepadButtonFlags.X), ref _lastStateButtonX, 102);
            TestButtonState(buttonFlags.HasFlag(GamepadButtonFlags.Y), ref _lastStateButtonY, 103);

            TestButtonState(rightThumb.X > 50, ref _lastStateRightThumbXPositive, 104, false);
            TestButtonState(rightThumb.X < -50, ref _lastStateRightThumbXNegative, 105, false);
            TestButtonState(rightThumb.Y > 50, ref _lastStateRightThumbYPositive, 106, false);
            TestButtonState(rightThumb.Y < -50, ref _lastStateRightThumbYNegative, 107, false);

            TestButtonState(buttonFlags.HasFlag(GamepadButtonFlags.RightShoulder), ref _lastStateButtonRightShoulder, 108);
            TestButtonState(gamepad.RightTrigger > 0, ref _lastStateButtonRightTrigger, 109);

            TestButtonState(buttonFlags.HasFlag(GamepadButtonFlags.DPadUp) || buttonFlags.HasFlag(GamepadButtonFlags.DPadDown) || buttonFlags.HasFlag(GamepadButtonFlags.DPadLeft) || buttonFlags.HasFlag(GamepadButtonFlags.DPadRight), ref _lastStateLeftThumb, 121, false);

            TestButtonState(buttonFlags.HasFlag(GamepadButtonFlags.DPadUp), ref _lastStateButtonDPadUp, 110);
            TestButtonState(buttonFlags.HasFlag(GamepadButtonFlags.DPadDown), ref _lastStateButtonDPadDown, 111);
            TestButtonState(buttonFlags.HasFlag(GamepadButtonFlags.DPadLeft), ref _lastStateButtonDPadLeft, 112);
            TestButtonState(buttonFlags.HasFlag(GamepadButtonFlags.DPadRight), ref _lastStateButtonDPadRight, 113);

            TestButtonState(leftThumb.X > 50, ref _lastStateLeftThumbXPositive, 114, false);
            TestButtonState(leftThumb.X < -50, ref _lastStateLeftThumbXNegative, 115, false);
            TestButtonState(leftThumb.Y > 50, ref _lastStateLeftThumbYPositive, 116, false);
            TestButtonState(leftThumb.Y < -50, ref _lastStateLeftThumbYNegative, 117, false);

            TestButtonState(buttonFlags.HasFlag(GamepadButtonFlags.LeftShoulder), ref _lastStateButtonLeftShoulder, 118);
            TestButtonState(gamepad.LeftTrigger > 0, ref _lastStateButtonLeftTrigger, 119);
        }

        private void InputPerSecondService_ThresholdRaised(object sender, bool thresholdRaised)
        {
            if (thresholdRaised)
            {
                _ = _midiController.SendMidiNoteAsync(12);
            }
            else
            {
                _ = _midiController.SendMidiNoteAsync(12);
            }
        }

        private void TestButtonState(bool condition, ref bool lastState, int midiNote, bool inputPerSecond = true)
        {
            if (condition && !lastState)
            {
                _midiController.SendMidiNote(midiNote, 127);

                if (inputPerSecond)
                {
                    _inputPerSecondService.AddInput();
                }

                lastState = true;
            }
            else if (!condition && lastState)
            {
                _midiController.SendMidiNote(midiNote, 0);
                lastState = false;
            }
        }
    }
}
