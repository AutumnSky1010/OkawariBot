using Discord;
using Discord.Commands;
namespace OkawariBot;
class Logger
{
	private IDiscordClient _client;
	public Logger(IDiscordClient client)
	{
		this._client = client;
	}
	/// <summary>
	/// ボットのステータスをコンソールウインドウに出力する。クライアントのイベントに登録して使う
	/// </summary>
	/// <param name="message">ボットクライアントの情報(起動、接続完了などの情報)</param>
	/// <returns>Task</returns>
	public Task ShowLog(LogMessage message)
	{
		Console.WriteLine(message.ToString());
		return Task.CompletedTask;
	}
	/// <summary>
	/// メッセージのログをコンソールウインドウに出力する
	/// </summary>
	/// <param name="message">ICommandContextを実装するログで出力したいメッセージのオブジェクト</param>
	/// <returns>Task</returns>
	public async Task ShowMessageLogAsync(ICommandContext message)
	{
		string time = new LogMessage().ToString();
		//メッセージが送信されたサーバーを取得する
		IGuild guild = await _client.GetGuildAsync(message.Guild.Id);
		//送信者のギルドユーザープロパティを取得
		IGuildUser user = await guild.GetUserAsync(message.Message.Author.Id);

		Console.ForegroundColor = ConsoleColor.Green;
		Console.Write($"【<{message.Guild.Name}>{message.Channel.Name}】");
		Console.ForegroundColor = ConsoleColor.Gray;
		Console.WriteLine($"{time}");
		Console.ForegroundColor = ConsoleColor.Yellow;
		string nickName = user.Nickname ?? "ニックネーム無し";
		Console.Write($"<{user.Username}({nickName})>");
		Console.ForegroundColor = ConsoleColor.White;
		Console.WriteLine($"{message.Message}");
	}
}