using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Wilsons {
    public class GlobalHooks : IDisposable {
        public delegate IntPtr WindowEventHandler(IntPtr Handle);
        public delegate IntPtr SysCommandEventHandler(int SysCommand, int lParam);
        public delegate IntPtr ActivateShellWindowEventHandler();
        public delegate IntPtr TaskmanEventHandler();
        public delegate IntPtr BasicHookEventHandler(IntPtr Handle1, IntPtr Handle2);
        public delegate IntPtr WndProcEventHandler(IntPtr Handle, IntPtr Message, IntPtr wParam, IntPtr lParam);

        // Functions imported from our unmanaged DLL
        [DllImport("GlobalCbtHook.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool InitializeShellHook(int threadID, IntPtr DestWindow);
        [DllImport("GlobalCbtHook.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void UninitializeShellHook();

        // API call needed to retreive the value of the messages to intercept from the unmanaged DLL
        [DllImport("user32.dll")]
        private static extern int RegisterWindowMessage(string lpString);
        [DllImport("user32.dll")]
        private static extern IntPtr GetProp(IntPtr hWnd, string lpString);

        // Handle of the window intercepting messages
        private IntPtr _Handle;

        private ShellHook _Shell;
        private HwndSource source;
        private bool dispose = false;

        public GlobalHooks(IntPtr Handle) {
            _Handle = Handle;

            source = HwndSource.FromHwnd(Handle);
            source.AddHook(WndProc);

            _Shell = new ShellHook(_Handle);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            System.Diagnostics.Debug.WriteLine(_Shell.ProcessWindowMessage(msg, wParam, lParam).ToString());
            return IntPtr.Zero;
        }

        public void Dispose() {
            if (dispose) return;
            dispose = true;

            _Shell.Stop();

            if (source != null) {
                source.RemoveHook(WndProc);
                source.Dispose();
            }
        }

        ~GlobalHooks() {
            Dispose();
        }

        #region Accessors

        public ShellHook Shell {
            get { return _Shell; }
        }

        #endregion

        public abstract class Hook {
            protected bool _IsActive = false;
            protected IntPtr _Handle;

            public Hook(IntPtr Handle) {
                _Handle = Handle;
            }

            public void Start() {
                if (!_IsActive) {
                    _IsActive = true;
                    OnStart();
                }
            }

            public void Stop() {
                if (_IsActive) {
                    OnStop();
                    _IsActive = false;
                }
            }

            ~Hook() {
                Stop();
            }

            public bool IsActive {
                get { return _IsActive; }
            }

            protected abstract void OnStart();
            protected abstract void OnStop();
            public abstract IntPtr ProcessWindowMessage(int msg, IntPtr wParam, IntPtr lParam);
        }

        public class ShellHook : Hook {
            // Values retrieved with RegisterWindowMessage
            private int _MsgID_Shell_ActivateShellWindow;
            private int _MsgID_Shell_GetMinRect;
            private int _MsgID_Shell_Language;
            private int _MsgID_Shell_Redraw;
            private int _MsgID_Shell_Taskman;
            private int _MsgID_Shell_WindowActivated;
            private int _MsgID_Shell_WindowCreated;
            private int _MsgID_Shell_WindowDestroyed;

            public event ActivateShellWindowEventHandler ActivateShellWindow;
            public event WindowEventHandler GetMinRect;
            public event WindowEventHandler Language;
            public event WindowEventHandler Redraw;
            public event TaskmanEventHandler Taskman;
            public event WindowEventHandler WindowActivated;
            public event WindowEventHandler WindowCreated;
            public event WindowEventHandler WindowDestroyed;

            public ShellHook(IntPtr Handle)
                : base(Handle) {
            }

            protected override void OnStart() {
                // Retreive the message IDs that we'll look for in WndProc
                _MsgID_Shell_ActivateShellWindow = RegisterWindowMessage("AUTUMN_HSHELL_ACTIVATESHELLWINDOW");
                _MsgID_Shell_GetMinRect = RegisterWindowMessage("AUTUMN_HSHELL_GETMINRECT");
                _MsgID_Shell_Language = RegisterWindowMessage("AUTUMN_HSHELL_LANGUAGE");
                _MsgID_Shell_Redraw = RegisterWindowMessage("AUTUMN_HSHELL_REDRAW");
                _MsgID_Shell_Taskman = RegisterWindowMessage("AUTUMN_HSHELL_TASKMAN");
                _MsgID_Shell_WindowActivated = RegisterWindowMessage("AUTUMN_HSHELL_WINDOWACTIVATED");
                _MsgID_Shell_WindowCreated = RegisterWindowMessage("AUTUMN_HSHELL_WINDOWCREATED");
                _MsgID_Shell_WindowDestroyed = RegisterWindowMessage("AUTUMN_HSHELL_WINDOWDESTROYED");

                // Start the hook
                try
                {
                    InitializeShellHook(0, _Handle);
                }
                catch 
                {
                    System.Windows.MessageBox.Show("GlobalHooks.dll module can't be loaded, so the taskbar may not work correctly. \nIs recommended that you re-download this program.", "error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }

            protected override void OnStop() {
                try
                {
                    UninitializeShellHook();
                }
                catch { /* maybe module doesn't exist and wasn't even initialized? */ }
            }

            public override IntPtr ProcessWindowMessage(int msg, IntPtr wParam, IntPtr lParam) {
                if (msg == _MsgID_Shell_ActivateShellWindow && ActivateShellWindow != null) {
                        return ActivateShellWindow();
                }
                else if (msg == _MsgID_Shell_GetMinRect && GetMinRect != null) {
                        return GetMinRect(wParam);
                }
                else if (msg == _MsgID_Shell_Language && Language != null) {
                        return Language(wParam);
                }
                else if (msg == _MsgID_Shell_Redraw && Redraw != null) {
                        return Redraw(wParam);
                }
                else if (msg == _MsgID_Shell_Taskman && Taskman != null) {
                        return Taskman();
                }
                else if (msg == _MsgID_Shell_WindowActivated && WindowActivated != null) {
                        return WindowActivated(wParam);
                }
                else if (msg == _MsgID_Shell_WindowCreated && WindowCreated != null) {
                        return WindowCreated(wParam);
                }
                else if (msg == _MsgID_Shell_WindowDestroyed && WindowDestroyed != null) {
                        return WindowDestroyed(wParam);
                }
                else return IntPtr.Zero;
            }
        }

    }
}
