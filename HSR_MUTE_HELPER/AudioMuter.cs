namespace HSR_MUTE_HELPER;

using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public sealed class AudioMuter : IDisposable
{
  private readonly NativeImports.WinEventDelegate winEventDelegate;
  private readonly IntPtr hookHandle;

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
    var result = NativeImports.GetWindowThreadProcessId(hwnd, out var activatedProcessId);
    if (result == 0)
		{
      return;
		}

    this.OnActivated((int)activatedProcessId);
  }

  private void Refresh()
	{
    var hwnd = NativeImports.GetForegroundWindow();
    var result = NativeImports.GetWindowThreadProcessId(hwnd, out var activatedProcessId);
    if (result == 0)
    {
      return;
    }

    this.OnActivated((int)activatedProcessId);
  }

  private void OnActivated(int activatedProcessId)
	{
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
    if (this.hookHandle == IntPtr.Zero)
		{
      return;
		}

    NativeImports.UnhookWinEvent(this.hookHandle);
  }
}
