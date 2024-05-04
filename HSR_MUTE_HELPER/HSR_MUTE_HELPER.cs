namespace HSR_MUTE_HELPER;

using System;
using System.Drawing;
using System.Windows.Forms;

public partial class HSR_MUTE_HELPER : Form
{
  private readonly ContextMenu contextMenu;
  private readonly NotifyIcon notifyIcon;
  private readonly AudioMuter audioMuter;

  public HSR_MUTE_HELPER()
  {
    this.contextMenu = new ContextMenu();
    this.contextMenu.MenuItems.Add(
      new MenuItem(
        "Info",
        new EventHandler((sender, e) =>
        {
          MessageBox.Show("setting.json 지정된 이름의 프로그램을 알텝하면 음소거해줘요", "HSR_MUTE_HELPER INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);
        })));
    new MenuItem(
        "Exit",
        new EventHandler((sender, e) =>
        {
          notifyIcon.Visible = false;
          Application.Exit();
        }));

    this.notifyIcon = new NotifyIcon
    {
      Icon = new Icon(Application.StartupPath + "/Properties/" + Program.Settings.Icon),
      ContextMenu = contextMenu,
      Visible = true
    };

    this.BackColor = Color.Magenta;
    this.TransparencyKey = Color.Magenta;
    this.FormBorderStyle = FormBorderStyle.None;
    this.TopMost = true;
    this.ShowInTaskbar = false;

    this.audioMuter = new AudioMuter();
  }

  protected override CreateParams CreateParams
  {
    get
    {
      var cp = base.CreateParams;
      cp.ExStyle |= 0x80;
      return cp;
    }
  }
}
