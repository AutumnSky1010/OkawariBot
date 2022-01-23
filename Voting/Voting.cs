
using Discord;
using OkawariBot.Timer;
using OkawariBot.Settings;
namespace OkawariBot.Voting;
public class Voting
{
	/// <summary>
	/// 初期化
	/// </summary>
	/// <param name="timeLimitSecond">投票可能時間(秒)</param>
	public Voting(int timeLimitSecond)
	{
		timeLimitSecond = timeLimitSecond < 0 ? 0 : timeLimitSecond;
		this.Timer = new System.Timers.Timer(timeLimitSecond * 1000);
	}
	/// <summary>
	/// 設定情報の入っているjsonファイル
	/// </summary>
	private SettingJson _settingJson = new SettingJson("settings.json");
	/// <summary>
	/// 投票可能時間計測用タイマー
	/// </summary>
	public System.Timers.Timer Timer { get; set; }
	/// <summary>
	/// おかわリスト(投票者のId)
	/// </summary>
	public List<ulong> Okawaris { get; set; } = new List<ulong>();
	/// <summary>
	/// ごちリスト(投票者のId)
	/// </summary>
	public List<ulong> Gotis { get; set; } = new List<ulong>();
	/// <summary>
	/// 有権者の人数
	/// </summary>
	public int VoterCount { get; set; }
	/// <summary>
	/// 投票を行っているテキストチャンネル
	/// </summary>
	public IMessageChannel VotingChannel { get; set; }
	/// <summary>
	/// 投票用メッセージ
	/// </summary>
	public IUserMessage VotingMessage { get; set; }
	/// <summary>
	/// 投票者リストのメッセージ
	/// </summary>
	public IUserMessage VotingUsersMessage { get; set; }
	/// <summary>
	/// 有権者(ボイスチャンネルに参加している人)の人数をセットする
	/// </summary>
	/// <param name="voiceChannel">ボイスチャンネル</param>
	public async Task SetVoterCount(IVoiceChannel voiceChannel)
	{
		int count = 0;
		await foreach (var users in voiceChannel.GetUsersAsync())
		{
			foreach (var user in users)
			{
				count++;
			}
		}
		this.VoterCount = count;
	}
	/// <summary>
	/// 投票者一覧メッセージを更新する。
	/// </summary>
	public async Task UpdateVotingMessage()
	{
		string gotisString = "【ごちリスト】\n";
		string okawariString = "【おかわリスト】\n";
		foreach(ulong userId in this.Okawaris)
		{
			okawariString += $"<@!{userId}>\n";
		}
		foreach(ulong userId in this.Gotis)
		{
			gotisString += $"<@!{userId}>\n"; 
		}
		okawariString += "と投票していない人全員\n";
		await this.VotingUsersMessage.ModifyAsync((message) => message.Content = gotisString + okawariString);
	}
	/// <summary>
	/// idがおかわリスト、ごちリストにある場合は削除し、成功したかどうかを返す。
	/// </summary>
	/// <param name="id">削除したいId</param>
	/// <returns>成功：true、失敗：false</returns>
	public bool TryRemoveId(ulong id)
	{
		if (this.Gotis.Contains(id))
		{
			this.Gotis.Remove(id);
			return true;
		}
		if (this.Okawaris.Contains(id))
		{
			this.Okawaris.Remove(id);
			return true;
		}
		return false;
	}
	/// <summary>
	/// 投票用のコンポーネントを作成し、取得する。
	/// </summary>
	/// <returns>投票用のコンポーネント(ボタン二つ)</returns>
	public MessageComponent GetVotingComponent()
	{
		BotSetting botSetting = this._settingJson.Deserialize();
		var builder = new ComponentBuilder()
			.WithButton("おかわり", "okawari", emote: Emote.Parse(botSetting.okawariEmojiId))
			.WithButton("ごち", "goti", emote: Emote.Parse(botSetting.gotiEmojiId));
		return builder.Build();
	}
	/// <summary>
	/// 投票時間が終了した時の処理
	/// </summary>
	/// <param name="timerAuthorId">タイマーを開始した人のId</param>
	/// <returns></returns>
	public async Task TimeOut(ulong timerAuthorId)
	{
		BotSetting botSetting = this._settingJson.Deserialize();
		Voting voting = VotingModule._authorIdVotingPairs[timerAuthorId];
		OkawariTimer timer = OkawariTimerModule._authorIdTimerPairs[timerAuthorId];
		voting.Timer.Stop();
		voting.Timer.Dispose();
		await voting.VotingChannel.DeleteMessageAsync(voting.VotingMessage);
		await voting.VotingChannel.DeleteMessageAsync(voting.VotingUsersMessage);
		VotingModule._authorIdVotingPairs.Remove(timerAuthorId);
		if (voting.Gotis.Count == voting.VoterCount)
		{
			await timer.TimerMessageChannel.SendMessageAsync("全員お腹いっぱいなのでタイマーを解除しました。");
			OkawariTimerModule._authorIdTimerPairs.Remove(timerAuthorId);
			return;
		}

		if (!botSetting.IsAutomaticExtension)
		{
			timer.ExtentionTimerMessage = await timer.TimerMessageChannel.SendMessageAsync(
				$"<@!{timerAuthorId}>何分延長するかを選んでください。", components: this.GetExtentionTimeComponent());
			return;
		}
		int extentionMilliSecond = botSetting.AutomaticExtensionSecond * 1000;
		timer.Timer = new System.Timers.Timer(extentionMilliSecond);
		timer.StartTimer(timerAuthorId);
		timer.TimerMessage = await timer.TimerMessageChannel.SendMessageAsync($"追加の{Time.GetTimeString(extentionMilliSecond)}タイマーを開始しました。", components: TimerComponent.Get(false));

	}
	private MessageComponent GetExtentionTimeComponent()
	{
		var menuBuilder = new SelectMenuBuilder()
		{
			CustomId = "ExtentionTimeMenu"
		};
		for (int i = 1;i <= 15; i++)
		{
			menuBuilder.AddOption($"{i}分", $"{i}");
		}
		menuBuilder.AddOption("延長しない。", "none");
		var builder = new ComponentBuilder().WithSelectMenu(menuBuilder);
		return builder.Build();
	}
}
