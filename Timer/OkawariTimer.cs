using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using OkawariBot.Voting;
using OkawariBot.Settings;
using OkawariBot.Channels;
namespace OkawariBot.Timer;
public class OkawariTimer
{
	private SettingJson _settingJson { get; set; } = new SettingJson("settings.json");
	/// <summary>
	/// 初期化
	/// </summary>
	/// <param name="author">タイマーを開始した人</param>
	/// <param name="timerMessageChannel">タイマーを開始したテキストチャンネル</param>
	/// <param name="minute">分</param>
	/// <param name="second">秒</param>
	public OkawariTimer(SocketGuildUser author, IMessageChannel timerMessageChannel, IUserMessage timerMessage)
	{
		var timer = new System.Timers.Timer(1000);
		this.Timer = timer;
		this.Author = author;
		this.MeetingChannel = new MeetingChannel(author.VoiceChannel, timerMessageChannel);
		this.TimerMessage = timerMessage;
	}
	/// <summary>
	/// タイマーが一時停止しているか
	/// </summary>
	public bool IsPause { get; set; } = false;
	/// <summary>
	/// タイマーの停止時間
	/// </summary>
	private int _timeOutMillisecond { get; set; } = 0;
	/// <summary>
	/// タイマーの経過時間
	/// </summary>
	private int _elapseMillisecond { get; set; } = 0;
	/// <summary>
	/// タイマーを開始した人
	/// </summary>
	public SocketGuildUser Author { get; set; }
	/// <summary>
	/// タイマーを開始した人が参加しているボイスチャンネル
	/// </summary>
	public MeetingChannel MeetingChannel { get; set; }
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
	/// タイマーがとまった時の処理
	/// </summary>
	/// <param name="authorId">タイマーを開始した人のId</param>
	public async Task OnTimeOut(ulong authorId)
	{
		BotSetting botSetting = this._settingJson.Deserialize();
		this.MeetingChannel.Stop();
		this.Timer.Dispose();
		await this.MeetingChannel.MessageChannel.DeleteMessageAsync(this.TimerMessage);
		Voting.Voting voting = await this.CreateVoting(authorId);
		await this.SendTimeOutMessage(voting, botSetting);
	}
	/// <summary>
	/// 時間切れ時にメッセージを送信する。
	/// </summary>
	private async Task SendTimeOutMessage(Voting.Voting voting, BotSetting botSetting)
	{
		await this.MeetingChannel.MessageChannel.SendMessageAsync(botSetting.TimeOutMessage, isTTS: true);
		voting.VotingMessage = await this.MeetingChannel.MessageChannel.SendMessageAsync
			($"<@!{this.Author.Id}>", 
			embed:await voting.GetVotingEmbed(this, botSetting), 
			components: voting.GetVotingComponent());
	}
	/// <summary>
	/// 投票を作成する
	/// </summary>
	/// <param name="timerAuthorId">タイマーを開始した人のId</param>
	/// <returns></returns>
	private async Task<Voting.Voting> CreateVoting(ulong timerAuthorId)
	{
		int votingTimeLimitSecond = this._settingJson.Deserialize().VotingTimeLimitSecond;
		if (votingTimeLimitSecond <= 0)
		{
			votingTimeLimitSecond = 1;
		}
		var voting = new Voting.Voting(votingTimeLimitSecond);
		voting.VotingChannel = OkawariTimerModule._authorIdTimerPairs[timerAuthorId].MeetingChannel.MessageChannel;
		await voting.SetVoterCount(OkawariTimerModule._authorIdTimerPairs[timerAuthorId].MeetingChannel.VoiceChannel);
		voting.Timer.Elapsed += async (sender, args) => await voting.Finish(timerAuthorId);
		VotingModule._authorIdVotingPairs.Add(timerAuthorId, voting);
		voting.Timer.Start();
		return VotingModule._authorIdVotingPairs[timerAuthorId];
	}
	/// <summary>
	/// タイマーを開始する。
	/// </summary>
	/// <param name="authorId">タイマーを開始した人のId</param>
	public async void StartTimer(ulong authorId, int timeOutMillisecond)
	{
		this.MeetingChannel.Start();
		this._timeOutMillisecond = timeOutMillisecond;
		this._elapseMillisecond = 0;
		this.Timer.Elapsed += async (sender, e) => await this.Elapsed(authorId);
		this.Timer.Start();
		await this.MeetingChannel.SendInformation();
		if (timeOutMillisecond == 0)
		{
			await this.OnTimeOut(authorId);
		}
	}
	public async Task Extend(int extentionMilliSecond, ulong authorId, MeetingState.MeetingStateType state)
	{
		this.Timer = new System.Timers.Timer(1000);
		this.StartTimer(authorId, extentionMilliSecond);
		this.MeetingChannel.State.State = state;
		this.TimerMessage = await this.MeetingChannel.MessageChannel.SendMessageAsync($"追加の{Time.GetTimeString(extentionMilliSecond)}タイマーを開始しました。", components: TimerComponent.Get(false));
	}
	private async Task Elapsed(ulong authorId)
	{
		this._elapseMillisecond += 1000;
		BotSetting botSetting = this._settingJson.Deserialize();
		if (botSetting.NotificationTimes.Contains((this._timeOutMillisecond - this._elapseMillisecond) / 1000) && botSetting.NotificationTimes.Count != 0)
		{
			await this.MeetingChannel.SendInformation(new EmbedFieldBuilder()
			{
				Name = "【残り時間】",
				Value = $"{ Time.GetTimeString(this._timeOutMillisecond - this._elapseMillisecond) }",
			});
			return;
		}
		if (this._elapseMillisecond == this._timeOutMillisecond)
		{
			await this.OnTimeOut(authorId);
		}
	}
}
