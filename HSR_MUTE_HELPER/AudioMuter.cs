namespace HSR_MUTE_HELPER;

using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public sealed class AudioMuter : IDisposable
{
  private readonly NativeImports.WinEventDelegate winEventDelegate;
  private readonly IntPtr hookHandle;
  private readonly object lockObject;
  private bool disposed;
  private uint lastActivatedProcessorId;
  private uint lastDwmsEventTime;

  public AudioMuter()
	{
    this.winEventDelegate = this.OnWinEvent;
    this.hookHandle = NativeImports.SetWinEventHook(
      NativeImports.EVENT_SYSTEM_FOREGROUND,
      NativeImports.EVENT_SYSTEM_FOREGROUND,
      IntPtr.Zero,
      this.winEventDelegate,
      0,
      0,
      NativeImports.WINEVENT_OUTOFCONTEXT | NativeImports.WINEVENT_SKIPOWNPROCESS);

    this.lockObject = new object();
    this.Refresh();
  }

  private void OnWinEvent(
    IntPtr hWinEventHook,
    uint eventType,
    IntPtr hwnd,
    int idObject,
    int idChild,
    uint dwEventThread,
    uint dwmsEventTime)
  {
    lock (this.lockObject)
		{
      if (dwmsEventTime <= this.lastDwmsEventTime)
      {
        return;
      }

      this.Refresh();
      this.lastDwmsEventTime = dwmsEventTime;
    }
  }

  private void Refresh()
	{
    var hwnd = NativeImports.GetForegroundWindow();
    var result = NativeImports.GetWindowThreadProcessId(hwnd, out var activatedProcessId);
    if (result == 0)
    {
      return;
    }

    if (this.lastActivatedProcessorId == activatedProcessId)
		{
      return;
		}
    
    this.OnActivated((int)activatedProcessId);
    this.lastActivatedProcessorId = activatedProcessId;
  }

  private void OnActivated(int activatedProcessId)
	{
    var audioSessions = this.GetLazyAudioSessions();
    var now = DateTime.Now;

    foreach (var targetName in Program.Settings.Program)
    {
      var targetProcesses = Process.GetProcessesByName(targetName);

      foreach (var targetProcess in targetProcesses)
      {
        if (audioSessions.Value.TryGetValue(targetProcess.Id, out var audioSession) == false)
        {
          continue;
        }

        var mute = targetProcess.Id != activatedProcessId;
        audioSession.SimpleAudioVolume.Mute = mute;
      }
    }
  }

  private Lazy<IReadOnlyDictionary<int, AudioSessionControl>> GetLazyAudioSessions()
  {
    return new Lazy<IReadOnlyDictionary<int, AudioSessionControl>>(
      () =>
      {
        var devices = new MMDeviceEnumerator();
        var defaultDevice = devices.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        var audioSessions = defaultDevice.AudioSessionManager.Sessions;
        var sessionMapByProcessorId = new Dictionary<int, AudioSessionControl>();

        for (var i = 0; i < audioSessions.Count; ++i)
        {
          var session = audioSessions[i];
          sessionMapByProcessorId.Add((int)session.GetProcessID, session);
        }

        return sessionMapByProcessorId;
      });
  }

	public void Dispose()
  {
    if (this.disposed)
		{
      return;
		}

    this.disposed = true;

    if (this.hookHandle != IntPtr.Zero)
		{
      NativeImports.UnhookWinEvent(this.hookHandle);
    }

    var audioSessions = this.GetLazyAudioSessions();

    foreach (var targetName in Program.Settings.Program)
    {
      var targetProcesses = Process.GetProcessesByName(targetName);

      foreach (var targetProcess in targetProcesses)
      {
        if (audioSessions.Value.TryGetValue(targetProcess.Id, out var audioSession) == false)
        {
          continue;
        }

        audioSession.SimpleAudioVolume.Mute = false;
      }
    }
  }
}
