namespace HSR_MUTE_HELPER;

using System;
using System.Runtime.InteropServices;
using System.Text;

public static class NativeImports
{
  public const uint WINEVENT_OUTOFCONTEXT = 0;
  public const int WINEVENT_SKIPOWNPROCESS = 2;
  public const uint EVENT_SYSTEM_FOREGROUND = 0x0003;

  public delegate void WinEventDelegate(
    IntPtr hWinEventHook, 
    uint eventType,
    IntPtr hwnd, 
    int idObject, 
    int idChild, 
    uint dwEventThread, 
    uint dwmsEventTime);

  [DllImport("user32.dll")]
  public static extern IntPtr GetForegroundWindow();

  [DllImport("user32.dll")]
  public static extern IntPtr SetWinEventHook(
    uint eventMin,
    uint eventMax,
    IntPtr hmodWinEventProc,
    WinEventDelegate lpfnWinEventProc,
    uint idProcess,
    uint idThread,
    uint dwFlags);

  [DllImport("user32.dll")]
  public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

  [DllImport("user32.dll")]
  public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

  [DllImport("user32.dll")]
  public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
}
