using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using OkawariBot.Voting;
using OkawariBot.Settings;
namespace OkawariBot.Timer;
public class OkawariTimer
{
	private SettingJson _settingJson = new SettingJson("settings.json");
	/// <summary>
	/// 初期化
	/// </summary>
	/// <param name="author">タイマーを開始した人</param>
	/// <param name="timerMessageChannel">タイマーを開始したテキストチャンネル</param>
	/// <param name="minute">分</param>
	/// <param name="second">秒</param>
	public OkawariTimer(SocketGuildUser author, IMessageChannel timerMessageChannel, IUserMessage timerMessage, int minute = 0, int second = 0)
	{
		minute = minute < 0 ? 0 : minute;
		second = second < 0 ? 0 : second;
		var timer = new System.Timers.Timer(minute*60*1000 + second*1000);
		this.Timer = timer;
		this.Author = author;
		this.JoinedVoiceChannel = author.VoiceChannel;
		this.TimerMessageChannel = timerMessageChannel;
		this.TimerMessage = timerMessage;
	}
	/// <summary>
	/// タイマーを開始した人
	/// </summary>
	public SocketGuildUser Author { get; set; }
	/// <summary>
	/// タイマーを開始した人が参加しているボイスチャンネル
	/// </summary>
	public IVoiceChannel JoinedVoiceChannel { get; }
	/// <summary>
	/// タイマーを開始したテキストチャンネル
	/// </summary>
	public IMessageChannel TimerMessageChannel { get; }
	/// <summary>
	/// タイマーのメッセージ
	/// </summary>
	public IUserMessage TimerMessage { get; set; }
	/// <summary>
	/// 自動延長時間を尋ねるメッセージ
	/// </summary>
	public IUserMessage ExtentionTimerMessage { get; set; }
	/// <summary>
	/// タイマーの本体
	/// </summary>
	public System.Timers.Timer Timer { get; set; }
	/// <summary>
	/// 参加者全員をメンションしたメッセージの文字列を返す
	/// </summary>
	/// <returns>タイマー開始した人が参加しているボイスチャンネルに参加している人を全員メンションしたメッセージの文字列</returns>
	private async Task<string> GetVoiceChannelUsersMentionMessage()
	{
		string mentionMessage = "";
		List<ulong> ids = await this.GetVoiceChannelUserIds();
		foreach (var id in ids)
		{
			mentionMessage += $"<@!{id}>";
		}
		return mentionMessage;
	}
	public async Task<List<ulong>> GetVoiceChannelUserIds()
	{
		var userList = new List<ulong>();
		var usersCollection = this.JoinedVoiceChannel.GetUsersAsync();
		await foreach (var users in usersCollection)
		{
			foreach (var user in users)
			{
				userList.Add(user.Id);
			}
		}
		return userList;
	}
	/// <summary>
	/// タイマーがとまった時の処理
	/// </summary>
	/// <param name="authorId">タイマーを開始した人のId</param>
	public async Task OnTimeOut(ulong authorId)
	{
		OkawariTimer timer = OkawariTimerModule._authorIdTimerPairs[authorId];
		BotSetting botSetting = this._settingJson.Deserialize();
		timer.Timer.Stop();
		timer.Timer.Dispose();
		await timer.TimerMessageChannel.DeleteMessageAsync(timer.TimerMessage);
		string mentionMessage = await timer.GetVoiceChannelUsersMentionMessage();
		await timer.TimerMessageChannel.SendMessageAsync(botSetting.TimeOutMessage, isTTS: true);
		Voting.Voting voting = await this.CreateVoting(authorId);
		voting.VotingMessage = await timer.TimerMessageChannel.SendMessageAsync(
			$"<@!{timer.Author.Id}>タイマーが終了しました。\n\n" +
			$"【投票できる人】\n{mentionMessage}\n\n " +
			$"{Emote.Parse(botSetting.okawariEmojiId)} or {Emote.Parse(botSetting.gotiEmojiId)}\n\n" +
			$"{Time.GetTimeString(botSetting.VotingTimeLimitSecond * 1000)}以内に投票してください。", components: voting.GetVotingComponent());
		voting.VotingUsersMessage = await timer.TimerMessageChannel.SendMessageAsync($"投票者一覧になる予定のメッセージ");
		
	}
	/// <summary>
	/// 投票を作成する
	/// </summary>
	/// <param name="timerAuthorId">タイマーを開始した人のId</param>
	/// <returns></returns>
	private async Task<Voting.Voting> CreateVoting(ulong timerAuthorId)
	{
		var voting = new Voting.Voting(this._settingJson.Deserialize().VotingTimeLimitSecond);
		voting.VotingChannel = OkawariTimerModule._authorIdTimerPairs[timerAuthorId].TimerMessageChannel;
		await voting.SetVoterCount(OkawariTimerModule._authorIdTimerPairs[timerAuthorId].JoinedVoiceChannel);
		voting.Timer.Elapsed += async (sender, args) => await voting.TimeOut(timerAuthorId);
		voting.Timer.Start();
		VotingModule._authorIdVotingPairs.Add(timerAuthorId, voting);
		return VotingModule._authorIdVotingPairs[timerAuthorId];
	}
	/// <summary>
	/// タイマーを開始する。
	/// </summary>
	/// <param name="authorId">タイマーを開始した人のId</param>
	public void StartTimer(ulong authorId)
	{
		this.Timer.Elapsed += async (sender, e) => await this.OnTimeOut(authorId);
		this.Timer.Start();
	}
}
