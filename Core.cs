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
		await this.LoadCommandsAsync();
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
///-----------------------------ライセンス---------------------------------------------------------------------------------------------------------------------------------------------------
/// 
/// 【MITライセンスの使用ライブラリ】
/// [ライブラリ名：著作者]
/// ・Microsoft.Extensions.DependencyInjection　　　　　　　：Microsoft
/// ・Microsoft.Extensions.DependencyInjection.Abstractions：Microsoft
/// ・Newtonsoft.Json　　　　　　　　　　　　　　　　　　　　　：James Newton-King
/// ・System.Interactive.Async　　　　　　　　　　　　　　　　：.NET Foundation and Contributors
/// ・System.Linq.Async　　　　　　　　　　　　　　　　　　　　：.NET Foundation and Contributors
/// ・System.Reactive										：.NET Foundation and Contributors
/// ・Discord.Net.WebSocket　　　　　　　　　　　　　　　　　　：Discord.Net Contributors
/// ・Discord.Net.Webhook　　　　　　　　　　　　　　　　　　　：Discord.Net Contributors
/// ・Discord.Net.Rest　　　　　　　　　　　　　　　　　　　　 ：Discord.Net Contributors
/// ・Discord.Net.Interactions　　　　　　　　　　　　　　　　：Discord.Net Contributors
/// ・Discord.Net.Core　　　　　　　　　　　　　　　　　　　　 ：Discord.Net Contributors
/// ・Discord.Net.Commands　　　　　　　　　　　　　　　　　　 ：Discord.Net Contributors
/// 
/// 【MITライセンス】
///Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without 
///restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of 
///the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
///
///The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
///
///THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
///MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
///LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR 
///OTHER DEALINGS IN THE SOFTWARE.
///-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------