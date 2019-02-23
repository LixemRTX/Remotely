﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using Win32;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;

namespace Remotely_ScreenCast
{
    public class BitBltCapture : ICapturer
    {
        public Bitmap CurrentFrame { get; set; }
        public Bitmap PreviousFrame { get; set; }
        public bool IsCapturing { get; set; }
        public bool CaptureFullscreen { get; set; } = true;
        public int PauseForMilliseconds { get; set; }
        public int SelectedScreen
        {
            get
            {
                return selectedScreen;
            }
            set
            {
                if (Screen.AllScreens.Length >= value + 1)
                {
                    selectedScreen = value;
                }
                else
                {
                    selectedScreen = 0;
                }
                CurrentBounds = Screen.AllScreens[selectedScreen].Bounds;
            }
        }
        public Rectangle CurrentBounds { get; set; } = Screen.PrimaryScreen.Bounds;
        private int selectedScreen = 0;

        // Offsets are the left and top edge of the screen, in case multiple monitor setups
        // create a situation where the edge of a monitor is in the negative.  This must
        // be converted to a 0-based max left/top to render images on the canvas properly.
        private Graphics graphic;
        private string desktopName;


        public BitBltCapture()
        {
            CurrentFrame = new Bitmap(CurrentBounds.Width, CurrentBounds.Height, PixelFormat.Format32bppArgb);
            PreviousFrame = new Bitmap(CurrentBounds.Width, CurrentBounds.Height, PixelFormat.Format32bppArgb);
            graphic = Graphics.FromImage(CurrentFrame);
			desktopName = Win32Interop.GetCurrentDesktop();
        }

        public async void BeginCapturing(string participantID)
        {
            CursorIconWatcher.Current.OnChange += CursorIcon_OnChange;
            while (IsCapturing)
            {
                Capture();

        
                await Task.Delay(PauseForMilliseconds);
                PauseForMilliseconds = 1;
            }
            CursorIconWatcher.Current.OnChange -= CursorIcon_OnChange;
        }

        private void CursorIcon_OnChange(object sender, int e)
        {
            //AditClient.SocketMessageHandler.SendIconUpdate(e);
        }

        public void Capture()
        {
			Console.WriteLine($"Using Capturer.");
			var currentDesktop = Win32Interop.GetCurrentDesktop();
            Console.WriteLine($"Current Desktop: {currentDesktop}");
            if (currentDesktop != desktopName)
            {
                desktopName = currentDesktop;
                var inputDesktop = Win32Interop.OpenInputDesktop();
                var success = User32.SetThreadDesktop(inputDesktop);
                User32.CloseDesktop(inputDesktop);
                Console.WriteLine($"Set thread desktop: {success}");
            }


            PreviousFrame = (Bitmap)CurrentFrame.Clone();

            try
            {
                graphic.CopyFromScreen(0 + CurrentBounds.Left, 0 + CurrentBounds.Top, 0, 0, new Size(CurrentBounds.Width, CurrentBounds.Height));
            }
            catch (Exception ex)
            {
                //Utilities.WriteToLog(ex);
            }
        }

	}
}