namespace IgnoreMessages
{
	public sealed class PluginConfig
	{
		public bool PrintKeyNames { get; set; } = false;
		public HashSet<string> IgnoredMessages { get; set; } = [];
	}
}
