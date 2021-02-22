﻿// Decompiled with JetBrains decompiler
// Type: ZuneUI.WindowSnapSimulator
// Assembly: ZuneShell, Version=4.7.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: FC8028F3-A47B-4FB4-B35B-11D1752D8264
// Assembly location: C:\Program Files\Zune\ZuneShell.dll

using Microsoft.Iris;
using Microsoft.Zune.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ZuneUI
{
    public class WindowSnapSimulator : SingletonModelItem<WindowSnapSimulator>
    {
        private const float c_hotspotPercentage = 0.015f;
        private Window _window;
        private bool _isInitialized;
        private List<WindowSnapSimulator.Monitor> _monitors;
        private WindowSnapSimulator.IHotspot _currentHotspot;
        private bool _isSnapping;

        public void Phase3Init() => this.Initialize();

        private void Initialize()
        {
            if (this.IsInitialized)
                return;
            this.InitializeMonitors();
            this._window = Application.Window;
            this._window.PropertyChanged += new PropertyChangedEventHandler(this.OnWindowPropertyChanged);
            Win7ShellManager.Instance.OnMonitorChange += new MonitorChangeHandler(this.OnMonitorChangeDetected);
            if (OSVersion.IsWin7())
                Win7ShellManager.Instance.OnWindowPositionKeyPress += new WindowPositionKeyPressHandler(this.OnWin7KeypressDetected);
            Shell defaultInstance = (Shell)ZuneShell.DefaultInstance;
            if (defaultInstance.NormalWindowSize == null)
                defaultInstance.NormalWindowSize = new Size(this._window.ClientSize.Width, this._window.ClientSize.Height);
            if (defaultInstance.NormalWindowPosition == null)
                defaultInstance.NormalWindowPosition = new Point(this._window.Position.X, this._window.Position.Y);
            this.IsInitialized = true;
            this.UpdateIsSnapping();
        }

        private void InitializeMonitors()
        {
            this._monitors = new List<WindowSnapSimulator.Monitor>();
            foreach (MonitorSize detectMonitor in new MonitorDetector().DetectMonitors())
            {
                WindowSnapSimulator.Monitor monitor = new WindowSnapSimulator.Monitor(detectMonitor);
                int width = Math.Max((monitor.Right - monitor.Left + 1) / 2, Shell.MinimumWindowWidth);
                int height = Math.Max(monitor.Bottom - monitor.Top + 1, Shell.MinimumWindowHeight);
                monitor.LeftSnap.Position = new WindowPosition(monitor.Left, monitor.Top);
                monitor.LeftSnap.Size = new WindowSize(width, height);
                monitor.RightSnap.Position = new WindowPosition(monitor.Right - width, monitor.Top);
                monitor.RightSnap.Size = new WindowSize(width, height);
                this._monitors.Add(monitor);
            }
            this._monitors.Sort();
            foreach (WindowSnapSimulator.Monitor monitor in this._monitors)
                monitor.InitializeHotspots(this._monitors, 0.015f);
        }

        public bool IsSnapping
        {
            get => this._isSnapping;
            private set
            {
                if (this._isSnapping == value)
                    return;
                this._isSnapping = value;
                this.FirePropertyChanged(nameof(IsSnapping));
            }
        }

        private bool IsInitialized
        {
            get => this._isInitialized;
            set => this._isInitialized = value;
        }

        public void ShiftUp()
        {
            if (!this.IsInitialized || this._window.WindowState != WindowState.Normal)
                return;
            if (this.IsSnapping)
                this.Unsnap(this.GetWindowMonitor());
            this._window.WindowState = WindowState.Maximized;
        }

        public void ShiftDown()
        {
            if (!this.IsInitialized)
                return;
            switch (this._window.WindowState)
            {
                case WindowState.Normal:
                    if (this.IsSnapping)
                    {
                        this.Unsnap();
                        break;
                    }
                    this._window.WindowState = WindowState.Minimized;
                    break;
                case WindowState.Maximized:
                    this._window.WindowState = WindowState.Normal;
                    break;
            }
        }

        public void ShiftLeft()
        {
            if (!this.IsInitialized)
                return;
            if (this._window.WindowState == WindowState.Maximized)
                this._window.WindowState = WindowState.Normal;
            WindowSnapSimulator.Monitor windowMonitor = this.GetWindowMonitor();
            switch (this.GetWindowSnappedness(windowMonitor))
            {
                case WindowSnapSimulator.WindowSnappedness.Unsnapped:
                    this.Snap(windowMonitor.LeftSnap);
                    break;
                case WindowSnapSimulator.WindowSnappedness.Left:
                    this.Snap(this.FindPreviousMonitor(windowMonitor).RightSnap);
                    break;
                case WindowSnapSimulator.WindowSnappedness.Right:
                    this.Unsnap(windowMonitor);
                    break;
            }
        }

        public void ShiftRight()
        {
            if (!this.IsInitialized)
                return;
            if (this._window.WindowState == WindowState.Maximized)
                this._window.WindowState = WindowState.Normal;
            WindowSnapSimulator.Monitor windowMonitor = this.GetWindowMonitor();
            switch (this.GetWindowSnappedness(windowMonitor))
            {
                case WindowSnapSimulator.WindowSnappedness.Unsnapped:
                    this.Snap(windowMonitor.RightSnap);
                    break;
                case WindowSnapSimulator.WindowSnappedness.Left:
                    this.Unsnap(windowMonitor);
                    break;
                case WindowSnapSimulator.WindowSnappedness.Right:
                    this.Snap(this.FindNextMonitor(windowMonitor).LeftSnap);
                    break;
            }
        }

        public void ShiftMonitorLeft()
        {
            if (!this.IsInitialized)
                return;
            WindowSnapSimulator.Monitor windowMonitor = this.GetWindowMonitor();
            WindowSnapSimulator.WindowSnappedness windowSnappedness = this.GetWindowSnappedness(windowMonitor);
            WindowSnapSimulator.Monitor destinationMonitor = (WindowSnapSimulator.Monitor)null;
            if (!OSVersion.IsWin7())
                destinationMonitor = this.FindPreviousMonitor(windowMonitor);
            switch (windowSnappedness)
            {
                case WindowSnapSimulator.WindowSnappedness.Unsnapped:
                    if (OSVersion.IsWin7())
                        break;
                    this.MoveWindowToMonitor(windowMonitor, this._window.Position, destinationMonitor);
                    break;
                case WindowSnapSimulator.WindowSnappedness.Left:
                    if (OSVersion.IsWin7())
                    {
                        this.Snap(windowMonitor.LeftSnap);
                        break;
                    }
                    this.Snap(destinationMonitor.LeftSnap);
                    break;
                case WindowSnapSimulator.WindowSnappedness.Right:
                    if (OSVersion.IsWin7())
                    {
                        this.Snap(windowMonitor.RightSnap);
                        break;
                    }
                    this.Snap(destinationMonitor.RightSnap);
                    break;
            }
        }

        public void ShiftMonitorRight()
        {
            if (!this.IsInitialized)
                return;
            WindowSnapSimulator.Monitor windowMonitor = this.GetWindowMonitor();
            WindowSnapSimulator.WindowSnappedness windowSnappedness = this.GetWindowSnappedness(windowMonitor);
            WindowSnapSimulator.Monitor destinationMonitor = (WindowSnapSimulator.Monitor)null;
            if (!OSVersion.IsWin7())
                destinationMonitor = this.FindNextMonitor(windowMonitor);
            switch (windowSnappedness)
            {
                case WindowSnapSimulator.WindowSnappedness.Unsnapped:
                    if (OSVersion.IsWin7())
                        break;
                    this.MoveWindowToMonitor(windowMonitor, this._window.Position, destinationMonitor);
                    break;
                case WindowSnapSimulator.WindowSnappedness.Left:
                    if (OSVersion.IsWin7())
                    {
                        this.Snap(windowMonitor.LeftSnap);
                        break;
                    }
                    this.Snap(destinationMonitor.LeftSnap);
                    break;
                case WindowSnapSimulator.WindowSnappedness.Right:
                    if (OSVersion.IsWin7())
                    {
                        this.Snap(windowMonitor.RightSnap);
                        break;
                    }
                    this.Snap(destinationMonitor.RightSnap);
                    break;
            }
        }

        public void Unsnap()
        {
            if (!this.IsSnapping || !this.IsInitialized)
                return;
            this.Unsnap(this.GetWindowMonitor());
        }

        public void DragBegun()
        {
            if (this.IsInitialized)
            {
                int x = 0;
                int y = 0;
                MousePosition.GetCursorScreenPosition(out x, out y);
                this._currentHotspot = this.GetWindowMonitor(x, y).GetHotspotForCursor(x, y);
            }
            else
                this._currentHotspot = (WindowSnapSimulator.IHotspot)null;
        }

        public bool ReactToDrag()
        {
            WindowSnapSimulator.IHotspot hotspot = (WindowSnapSimulator.IHotspot)null;
            if (this.IsInitialized)
            {
                int x = 0;
                int y = 0;
                MousePosition.GetCursorScreenPosition(out x, out y);
                hotspot = this.GetWindowMonitor(x, y).GetHotspotForCursor(x, y);
                if (hotspot != this._currentHotspot)
                {
                    if (this.IsSnapping)
                        this.Unsnap();
                    if (this._window.WindowState == WindowState.Maximized)
                        this._window.WindowState = WindowState.Normal;
                    this._currentHotspot = hotspot;
                    hotspot?.Snap();
                }
            }
            return hotspot != null;
        }

        public void DragEnded() => this._currentHotspot = (WindowSnapSimulator.IHotspot)null;

        public void SetWindowPosition(int left, int top, int width, int height)
        {
            this.Initialize();
            WindowSnapSimulator.Monitor windowMonitor = this.GetWindowMonitor(left + width / 2, top + height / 2);
            if (windowMonitor != null)
            {
                if (width > windowMonitor.Right - windowMonitor.Left)
                    width = windowMonitor.Right - windowMonitor.Left;
                if (height > windowMonitor.Bottom - windowMonitor.Top)
                    height = windowMonitor.Bottom - windowMonitor.Top;
                if (left < windowMonitor.Left)
                    left = windowMonitor.Left;
                else if (left + width > windowMonitor.Right)
                    left = windowMonitor.Right - width;
                if (top < windowMonitor.Top)
                    top = windowMonitor.Top;
                else if (top + height > windowMonitor.Bottom)
                    top = windowMonitor.Bottom - height;
            }
            this._window.Position = new WindowPosition(left, top);
            this._window.ClientSize = new WindowSize(width, height);
        }

        private void Snap(WindowSnapSimulator.WindowPlacement placement)
        {
            if (!this.IsInitialized)
                return;
            this._window.Position = placement.Position;
            this._window.ClientSize = placement.Size;
        }

        private void Unsnap(WindowSnapSimulator.Monitor monitor)
        {
            if (!this.IsSnapping || !this.IsInitialized)
                return;
            Shell defaultInstance = (Shell)ZuneShell.DefaultInstance;
            WindowPosition windowPosition = new WindowPosition(defaultInstance.NormalWindowPosition.X, defaultInstance.NormalWindowPosition.Y);
            WindowSize size = new WindowSize(defaultInstance.NormalWindowSize.Width, defaultInstance.NormalWindowSize.Height);
            WindowSnapSimulator.Monitor windowMonitor = this.GetWindowMonitor(windowPosition, size);
            if (monitor == windowMonitor)
                this._window.Position = windowPosition;
            else
                this.MoveWindowToMonitor(windowMonitor, windowPosition, monitor);
            this._window.ClientSize = size;
        }

        private void MoveWindowToMonitor(
          WindowSnapSimulator.Monitor currentMonitor,
          WindowPosition currentPosition,
          WindowSnapSimulator.Monitor destinationMonitor)
        {
            int num1 = destinationMonitor.Left - currentMonitor.Left;
            int num2 = destinationMonitor.Top - currentMonitor.Top;
            this._window.Position = new WindowPosition(currentPosition.X + num1, currentPosition.Y + num2);
        }

        private WindowSnapSimulator.Monitor FindNextMonitor(
          WindowSnapSimulator.Monitor monitor)
        {
            int index = this._monitors.IndexOf(monitor) + 1;
            if (index >= this._monitors.Count)
                index = 0;
            return this._monitors[index];
        }

        private WindowSnapSimulator.Monitor FindPreviousMonitor(
          WindowSnapSimulator.Monitor monitor)
        {
            int index = this._monitors.IndexOf(monitor) - 1;
            if (index < 0)
                index = this._monitors.Count - 1;
            return this._monitors[index];
        }

        private WindowSnapSimulator.WindowSnappedness GetWindowSnappedness(
          WindowSnapSimulator.Monitor monitor)
        {
            WindowSnapSimulator.WindowSnappedness windowSnappedness = WindowSnapSimulator.WindowSnappedness.Ineligible;
            if (this.IsInitialized && this._window.WindowState != WindowState.Maximized && this._window.WindowState != WindowState.Minimized)
                windowSnappedness = !(this._window.Position == monitor.LeftSnap.Position) || !(this._window.ClientSize == monitor.LeftSnap.Size) ? (!(this._window.Position == monitor.RightSnap.Position) || !(this._window.ClientSize == monitor.RightSnap.Size) ? WindowSnapSimulator.WindowSnappedness.Unsnapped : WindowSnapSimulator.WindowSnappedness.Right) : WindowSnapSimulator.WindowSnappedness.Left;
            return windowSnappedness;
        }

        private WindowSnapSimulator.Monitor GetWindowMonitor() => this.GetWindowMonitor(this._window.Position, this._window.ClientSize);

        private WindowSnapSimulator.Monitor GetWindowMonitor(
          WindowSnapSimulator.WindowPlacement placement)
        {
            return this.GetWindowMonitor(placement.Position, placement.Size);
        }

        private WindowSnapSimulator.Monitor GetWindowMonitor(
          WindowPosition position,
          WindowSize size)
        {
            return this.GetWindowMonitor(position.X + size.Width / 2, position.Y + size.Height / 2);
        }

        private WindowSnapSimulator.Monitor GetWindowMonitor(int x, int y)
        {
            WindowSnapSimulator.Monitor monitor1 = (WindowSnapSimulator.Monitor)null;
            if (this.IsInitialized)
            {
                if (this._monitors.Count > 1)
                {
                    int num1 = int.MaxValue;
                    foreach (WindowSnapSimulator.Monitor monitor2 in this._monitors)
                    {
                        int num2 = 0;
                        int num3 = 0;
                        if (x < monitor2.Left)
                            num2 = monitor2.Left - x;
                        else if (x > monitor2.Right)
                            num2 = x - monitor2.Right;
                        if (y < monitor2.Top)
                            num3 = monitor2.Top - y;
                        else if (y > monitor2.Bottom)
                            num3 = y - monitor2.Bottom;
                        int num4 = num2 + num3;
                        if (num4 < num1)
                        {
                            monitor1 = monitor2;
                            num1 = num4;
                        }
                    }
                }
                else
                    monitor1 = this._monitors[0];
            }
            return monitor1;
        }

        private void UpdateIsSnapping()
        {
            bool flag = false;
            if (this._window.WindowState == WindowState.Normal)
            {
                switch (this.GetWindowSnappedness(this.GetWindowMonitor()))
                {
                    case WindowSnapSimulator.WindowSnappedness.Left:
                    case WindowSnapSimulator.WindowSnappedness.Right:
                        flag = true;
                        break;
                }
            }
            this.IsSnapping = flag;
        }

        private void OnWindowPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (!(args.PropertyName == "Position") && !(args.PropertyName == "ClientSize") && !(args.PropertyName == "WindowState"))
                return;
            this.UpdateIsSnapping();
        }

        private void OnWin7KeypressDetected(WindowPositionKeys key) => Application.DeferredInvoke((DeferredInvokeHandler)delegate
       {
           switch (key)
           {
               case WindowPositionKeys.eWindowPositionKeyLeft:
                   this.ShiftLeft();
                   break;
               case WindowPositionKeys.eWindowPositionKeyRight:
                   this.ShiftRight();
                   break;
               case WindowPositionKeys.eWindowPositionKeyDown:
                   if (this._window.WindowState != WindowState.Maximized)
                       break;
                   this.ShiftDown();
                   break;
               case WindowPositionKeys.eWindowPositionKeyShiftLeft:
                   this.ShiftMonitorLeft();
                   break;
               case WindowPositionKeys.eWindowPositionKeyShiftRight:
                   this.ShiftMonitorRight();
                   break;
           }
       }, (object)null);

        private void OnMonitorChangeDetected() => Application.DeferredInvoke((DeferredInvokeHandler)delegate
       {
           this.Unsnap();
           this.InitializeMonitors();
       }, (object)null);

        private enum WindowSnappedness
        {
            Unsnapped,
            Left,
            Right,
            Ineligible,
        }

        private class WindowPlacement
        {
            private WindowPosition _position;
            private WindowSize _size;

            public WindowPosition Position
            {
                get => this._position;
                set => this._position = value;
            }

            public WindowSize Size
            {
                get => this._size;
                set => this._size = value;
            }
        }

        private class Monitor : IComparable<WindowSnapSimulator.Monitor>
        {
            private MonitorSize _dimensions;
            private WindowSnapSimulator.WindowPlacement _leftSnap;
            private WindowSnapSimulator.WindowPlacement _rightSnap;
            private int _hotspotSize;
            private WindowSnapSimulator.IHotspot _topHotspot;
            private WindowSnapSimulator.IHotspot _leftHotspot;
            private WindowSnapSimulator.IHotspot _rightHotspot;

            public Monitor(MonitorSize dimensions)
            {
                this._dimensions = dimensions;
                this.LeftSnap = new WindowSnapSimulator.WindowPlacement();
                this.RightSnap = new WindowSnapSimulator.WindowPlacement();
            }

            public int Left => this._dimensions.WorkArea.Left;

            public int Right => this._dimensions.WorkArea.Right;

            public int Top => this._dimensions.WorkArea.Top;

            public int Bottom => this._dimensions.WorkArea.Bottom;

            public WindowSnapSimulator.WindowPlacement LeftSnap
            {
                get => this._leftSnap;
                private set => this._leftSnap = value;
            }

            public WindowSnapSimulator.WindowPlacement RightSnap
            {
                get => this._rightSnap;
                private set => this._rightSnap = value;
            }

            public void InitializeHotspots(
              List<WindowSnapSimulator.Monitor> monitorList,
              float hotspotSizePercentage)
            {
                this._hotspotSize = (int)((double)((this.Right - this.Left + 1 + (this.Bottom - this.Top + 1)) / 2) * (double)hotspotSizePercentage);
                bool flag1 = true;
                bool flag2 = true;
                bool flag3 = true;
                foreach (WindowSnapSimulator.Monitor monitor in monitorList)
                {
                    RECT totalArea = monitor._dimensions.TotalArea;
                    if (totalArea.Top + 1 == this.Top)
                        flag1 = false;
                    if (totalArea.Right + 1 == this.Left)
                        flag2 = false;
                    if (totalArea.Left - 1 == this.Right)
                        flag3 = false;
                }
                if (flag1)
                    this._topHotspot = (WindowSnapSimulator.IHotspot)new WindowSnapSimulator.MaximizeHotspot();
                if (flag2)
                    this._leftHotspot = (WindowSnapSimulator.IHotspot)new WindowSnapSimulator.PlacementHotspot(this.LeftSnap);
                if (!flag3)
                    return;
                this._rightHotspot = (WindowSnapSimulator.IHotspot)new WindowSnapSimulator.PlacementHotspot(this.RightSnap);
            }

            public WindowSnapSimulator.IHotspot GetHotspotForCursor(
              int cursorX,
              int cursorY)
            {
                WindowSnapSimulator.IHotspot hotspot = (WindowSnapSimulator.IHotspot)null;
                if (cursorY <= this.Top + this._hotspotSize && this._topHotspot != null)
                    hotspot = this._topHotspot;
                else if (cursorX <= this.Left + this._hotspotSize)
                    hotspot = this._leftHotspot;
                else if (cursorX >= this.Right - this._hotspotSize)
                    hotspot = this._rightHotspot;
                return hotspot;
            }

            public int CompareTo(WindowSnapSimulator.Monitor other)
            {
                int num = this.Top.CompareTo(other.Top);
                return num == 0 ? this.Left.CompareTo(other.Left) : num;
            }
        }

        private interface IHotspot
        {
            void Snap();
        }

        private class MaximizeHotspot : WindowSnapSimulator.IHotspot
        {
            public void Snap() => SingletonModelItem<WindowSnapSimulator>.Instance.ShiftUp();
        }

        private class PlacementHotspot : WindowSnapSimulator.IHotspot
        {
            private WindowSnapSimulator.WindowPlacement _placement;

            public PlacementHotspot(WindowSnapSimulator.WindowPlacement placement) => this._placement = placement;

            public void Snap() => SingletonModelItem<WindowSnapSimulator>.Instance.Snap(this._placement);
        }
    }
}
