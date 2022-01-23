using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

using OkawariBot.Settings;
namespace OkawariBot;
class Core
{
	private DiscordSocketClient _client;
	private Logger _logger;
	private IServiceProvider _services;
	private InteractionService _interactionService;
	private SettingJson _settingJson = new SettingJson("settings.json");
	private static Task Main() => new Core().MainAsync();
	private async Task MainAsync()
	{
		var socketConfig = new DiscordSocketConfig()
		{
			GatewayIntents = GatewayIntents.All
		};
		this._client = new DiscordSocketClient(socketConfig);
		this._services = new ServiceCollection().BuildServiceProvider();
		this._interactionService = new InteractionService(_client);
		this._logger = new Logger(_client);
		this._client.Log += _logger.ShowLog;
		this._client.MessageReceived += MessageReceivedAsync;
		this._client.InteractionCreated += ExecuteSlashCommand;
		this._client.Ready += RegisterCommands;
		await this._client.LoginAsync(TokenType.Bot, this._settingJson.Deserialize().Token);
		await this._client.StartAsync();
		await LoadCommandsAsync();
		await Task.Delay(-1);
	}
	private async Task RegisterCommands()
	{
		await this._interactionService.RegisterCommandsToGuildAsync(this._settingJson.Deserialize().GuildId);
		//await this._interactionService.RegisterCommandsGloballyAsync();
	}
	private async Task LoadCommandsAsync() => await this._interactionService.AddModulesAsync(assembly:Assembly.GetEntryAssembly(),services: this._services);
	
	private async Task ExecuteSlashCommand(SocketInteraction interaction)
	{
		var socketInteractionContext = new SocketInteractionContext(this._client, interaction);
		await this._interactionService.ExecuteCommandAsync(socketInteractionContext, this._services);
	}
	private async Task MessageReceivedAsync(SocketMessage message)
	{
		var userMessage = message as SocketUserMessage;
		if (userMessage == null) { return; }
		ICommandContext commandContext = new CommandContext(this._client,userMessage);
		await this._logger.ShowMessageLogAsync(commandContext);
	}
}