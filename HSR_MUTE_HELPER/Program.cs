namespace HSR_MUTE_HELPER;

using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public static class Program
{
  public static readonly Settings Settings;

  static Program()
  {
    try
		{
      var jsonSerializerSettings = new JsonSerializerSettings();
      jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Error;

      var rawJson = File.ReadAllText(Application.StartupPath + "/Properties" + "/setting.json");
      Settings = JsonConvert.DeserializeObject<Settings>(rawJson, jsonSerializerSettings);
    }
    catch (Exception)
		{
      MessageBox.Show(
        "실행에 필요한 파일이 일부 누락되었습니다.",
        "HSR_MUTE_HELPER WARNING",
        MessageBoxButtons.OK,
        MessageBoxIcon.Warning);
      Environment.Exit(-1);
    }
  }

  /// <summary>
  /// 해당 애플리케이션의 주 진입점입니다.
  /// </summary>
  [STAThread]
  public static void Main()
  {
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);

    var currentProcess = Process.GetCurrentProcess();
    var processName = currentProcess.ProcessName;
    var processes = Process.GetProcessesByName(processName);

    if (processes.Length != 0)
    {
      foreach (var process in processes)
			{
        if (process.Id == currentProcess.Id)
				{
          continue;
				}

        process.Kill();
      }
    }

    try
    {
      Icon icon = new Icon(Application.StartupPath + "/Properties/" + Settings.Icon);
      MessageBox.Show("프로그램이 정상적으로 실행되었습니다.", "HSR_MUTE_HELPER INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);
      Application.Run(new HSR_MUTE_HELPER());
    }
    catch
    {
      MessageBox.Show("실행에 필요한 파일이 일부 누락되었습니다.", "HSR_MUTE_HELPER WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
  }
}
