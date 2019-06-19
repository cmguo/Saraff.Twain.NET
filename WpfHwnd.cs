//////////////////////////////////////////////////////////////////
//
// Copyright (c) 2011, IBN Labs, Ltd All rights reserved.
// Please Contact rnd@ibn-labs.com 
//
// This Code is released under Code Project Open License (CPOL) 1.02,
//
// To emphesize - # Representations, Warranties and Disclaimer. THIS WORK IS PROVIDED "AS IS", "WHERE IS" AND "AS AVAILABLE", WITHOUT ANY EXPRESS OR IMPLIED WARRANTIES OR CONDITIONS OR GUARANTEES. YOU, THE USER, ASSUME ALL RISK IN ITS USE, INCLUDING COPYRIGHT INFRINGEMENT, PATENT INFRINGEMENT, SUITABILITY, ETC. AUTHOR EXPRESSLY DISCLAIMS ALL EXPRESS, IMPLIED OR STATUTORY WARRANTIES OR CONDITIONS, INCLUDING WITHOUT LIMITATION, WARRANTIES OR CONDITIONS OF MERCHANTABILITY, MERCHANTABLE QUALITY OR FITNESS FOR A PARTICULAR PURPOSE, OR ANY WARRANTY OF TITLE OR NON-INFRINGEMENT, OR THAT THE WORK (OR ANY PORTION THEREOF) IS CORRECT, USEFUL, BUG-FREE OR FREE OF VIRUSES. YOU MUST PASS THIS DISCLAIMER ON WHENEVER YOU DISTRIBUTE THE WORK OR DERIVATIVE WORKS.
//

using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Forms;

namespace Saraff.Twain
{

    public class WpfHwnd
    {
        private System.IntPtr _handle = IntPtr.Zero;

        public class HwndReadyArgs
        {
            public HwndReadyArgs(System.IntPtr hwnd)
            {
                Hwnd = hwnd;
            }

            public System.IntPtr Hwnd { get; private set; }
        }

        public class PreFilterMessageArgs
        {
            public PreFilterMessageArgs(Message m)
            {
                Message = m;
                IsHandled = false;
            }

            public System.Windows.Forms.Message Message { get; private set; }
            public bool IsHandled { get; set; }
        }

        public event EventHandler<HwndReadyArgs> HwndReady;

        public event EventHandler<PreFilterMessageArgs> PreFilterMessage;

        public Window TheMainWindow { get; private set; }

        //public uint WM_App_Acquire = Win32.RegisterWindowMessage("IBN_WfpTwain_Acquire");
        //If you do not want so register a message, a simple const will do (just make sure it is unique)
        //public const uint WM_App_Aquire =  0x8123; // WM_App + 0x123 - should be uniqe within the application

        public System.IntPtr WindowHandle
        {
            get
            {
                if (_handle == IntPtr.Zero)
                    _handle = (new WindowInteropHelper(TheMainWindow)).Handle;
                return _handle;
            }
        }

        public WpfHwnd(Window window)
        {
            TheMainWindow = window;
            // hook to events of the main window
            if (WindowHandle != IntPtr.Zero)
            {
                // main windows is initialized and we can hook events and start woking with it
                HostWindow_Loaded(this, null);
            }
            else
            {
                // hook events etc later, when the main window is loaded.
                TheMainWindow.Loaded += HostWindow_Loaded;
            }
            TheMainWindow.Closing += HostWindow_Closing;
        }

        ~WpfHwnd()
        {
            // by now the interface should already be closed. we call terminate just in case.
        }

        static UInt32 wParam_buffer;

        private void HostWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AddMessageHook();
            HwndReady?.Invoke(this, new HwndReadyArgs(WindowHandle));
        }

        private void HostWindow_Closing(object sender, EventArgs e)
        {
            RemoveMessageHook();
        }

        private void AddMessageHook()
        {
            HwndSource src = HwndSource.FromHwnd(WindowHandle);
            src.AddHook(new HwndSourceHook(this.WndProc));
        }

        private void RemoveMessageHook()
        {
            HwndSource src = HwndSource.FromHwnd(WindowHandle);
            src.AddHook(new HwndSourceHook(this.WndProc));
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            System.Windows.Forms.Message m = new System.Windows.Forms.Message();
            m.HWnd = hwnd;
            m.Msg = msg;
            m.WParam = wParam;
            m.LParam = lParam;

            if (handled)
                return IntPtr.Zero;

            PreFilterMessageArgs e = new PreFilterMessageArgs(m);
            PreFilterMessage?.Invoke(this, e);
            handled = e.IsHandled;
            return IntPtr.Zero;
        }

    }
}
