using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RunAsAdmin.Helper
{
    public static class WM
    {
        #region Window to front
        [DllImport("User32.dll")]
        public static extern int SetForegroundWindow(int hWnd);
        #endregion

        #region Placeholder
        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        [DllImport("user32.dll")]
        private static extern bool GetComboBoxInfo(IntPtr hwnd, ref COMBOBOXINFO pcbi);
        [StructLayout(LayoutKind.Sequential)]

        private struct COMBOBOXINFO
        {
            public int cbSize;
            public RECT rcItem;
            public RECT rcButton;
            public UInt32 stateButton;
            public IntPtr hwndCombo;
            public IntPtr hwndItem;
            public IntPtr hwndList;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        public static void Placeholder(Control control, string placeholder)
        {
            try
            {
                if (control is ComboBox)
                {
                    COMBOBOXINFO info = GetComboBoxInfo(control);
                    SendMessage(info.hwndItem, EM_SETCUEBANNER, 0, placeholder);
                }
                else
                {
                    SendMessage(control.Handle, EM_SETCUEBANNER, 0, placeholder);
                }
            }
            catch (Exception)
            {

            }
        }

        private static COMBOBOXINFO GetComboBoxInfo(Control control)
        {
            COMBOBOXINFO info = new COMBOBOXINFO();
            //a combobox is made up of three controls, a button, a list and textbox;
            //we want the textbox
            info.cbSize = Marshal.SizeOf(info);
            GetComboBoxInfo(control.Handle, ref info);
            return info;
        }
        #endregion


    }
}
