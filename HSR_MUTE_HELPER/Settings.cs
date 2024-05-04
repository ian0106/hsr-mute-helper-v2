namespace HSR_MUTE_HELPER;

using Newtonsoft.Json;

public sealed class Settings
{
	public Settings(string icon, string[] program)
	{
		this.Icon = icon;
		this.Program = program;
	}

	[JsonProperty("icon")]
	public string Icon { get; }

	[JsonProperty("program")]
	public string[] Program { get; }
}
