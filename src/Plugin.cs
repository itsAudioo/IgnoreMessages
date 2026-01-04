using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Plugins;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared.NetMessages;
using Microsoft.Extensions.Configuration;
using SwiftlyS2.Shared.ProtobufDefinitions;
using Microsoft.Extensions.DependencyInjection;

namespace IgnoreMessages;

[PluginMetadata(
	Id = "IgnoreMessages",
	Version = "1.0.0",
	Name = "IgnoreMessages",
	Author = "itsAudio",
	Description = "Blocks selected HUD and hint messages by localization key"
)]
public sealed class IgnoreMessages
	(ISwiftlyCore core) : BasePlugin(core)
{
	public required IOptionsMonitor<PluginConfig> Config { get; set; }

	public override void Load(bool hotReload)
	{
		Core.Configuration
			.InitializeJsonWithModel<PluginConfig>("IgnoreMessages.json", "IgnoreMessages")
				.Configure(builder =>
				{
					builder.AddJsonFile("IgnoreMessages.json", optional: false, reloadOnChange: true);
				});

		var services = new ServiceCollection();

		services.AddSwiftly(Core)
			.AddOptionsWithValidateOnStart<PluginConfig>()
				.BindConfiguration("IgnoreMessages");

		var serviceProvider =
			services.BuildServiceProvider();

		this.Config =
			serviceProvider.GetRequiredService<
				IOptionsMonitor<PluginConfig>>();
	}

	[ServerNetMessageHandler]
	private HookResult OnTextMsg(CUserMessageTextMsg msg)
	{
		var accessor = msg.Accessor;
		var count = accessor.GetRepeatedFieldSize("param");

		for (int i = 0; i < count; i++)
		{
			var value = accessor.GetRepeatedString("param", i);
			if (string.IsNullOrEmpty(value))
				continue;

			if (Config.CurrentValue.IgnoredMessages.Contains(value))
				return HookResult.Stop;
		}
		return HookResult.Continue;
	}

	[ServerNetMessageHandler]
	private HookResult OnHintText(CCSUsrMsg_HintText msg)
	{
		var text = msg.Accessor.GetString("message");
		return HandleKey(text);
	}

	private HookResult HandleKey(string? key)
	{
		if (string.IsNullOrEmpty(key) || !key.StartsWith("#"))
			return HookResult.Continue;

		if (Config.CurrentValue.IgnoredMessages.Contains(key))
			return HookResult.Stop;

		if (Config.CurrentValue.PrintKeyNames)
		{
			Core.ConsoleOutput.WriteToServerConsole
			(
				$" [green][IgnoreMessages][/] Current message key:[lightblue] \"{key.Replace(Environment.NewLine, "")}\" [/]\n"
			);
		}

		return HookResult.Continue;
	}

	public override void Unload()
	{

	}
}
