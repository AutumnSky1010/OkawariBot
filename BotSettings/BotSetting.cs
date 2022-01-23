
using System.Text.Json.Serialization;
namespace OkawariBot.Settings;
internal class BotSetting
{
	/// <summary>
	/// botのトークン
	/// </summary>
	[JsonPropertyName("token")]
	public string Token { get; set; } = "未設定";
	/// <summary>
	/// 投票待ち時間のタイムリミット(秒)
	/// </summary>
	[JsonPropertyName("votingTimeLimitSecond")]
	public int VotingTimeLimitSecond { get; set; } = 30;
	/// <summary>
	/// 時間切れの時に送信するメッセージの文字列
	/// </summary>
	[JsonPropertyName("timeOutMessage")]
	public string TimeOutMessage { get; set; } = "おかわりおあごち";
	/// <summary>
	/// botを使用するディスコードサーバ(ギルド)のId
	/// </summary>
	[JsonPropertyName("guildId")]
	public ulong GuildId { get; set; } = 0;
	/// <summary>
	/// おかわりの絵文字Id
	/// </summary>
	[JsonPropertyName("okawariEmojiId")]
	public string okawariEmojiId { get; set; } = ":no_entry_sign:";
	/// <summary>
	/// ごちの絵文字Id
	/// </summary>
	[JsonPropertyName("gotiEmojiId")]
	public string gotiEmojiId { get; set; } = ":no_entry_sign:";
	/// <summary>
	/// 自動延長するか
	/// </summary>
	[JsonPropertyName("isAutomaticExtension")]
	public bool IsAutomaticExtension { get; set; } = false;
	[JsonPropertyName("automaticExtensionSecond")]
	public int AutomaticExtensionSecond { get; set; } = 180;
}
