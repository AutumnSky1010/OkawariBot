
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
	private SettingJson _settingJson { get; set; } = new SettingJson("settings.json");
	/// <summary>
	/// 投票可能時間計測用タイマー
	/// </summary>
	public System.Timers.Timer Timer { get; set; } = new System.Timers.Timer();
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
	/// 投票者一覧埋め込みを更新する。
	/// </summary>
	public async Task UpdateVotingEmbed(OkawariTimer timer, BotSetting setting)
	{
		await this.VotingMessage.ModifyAsync(async (message) => message.Embed = await this.GetVotingEmbed(timer, setting));
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
			.WithButton("おかわり", "okawari", emote: EmotePuls.Parse(botSetting.okawariEmojiId))
			.WithButton("ごち", "goti", emote: EmotePuls.Parse(botSetting.gotiEmojiId));
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
		OkawariTimer timer = OkawariTimerModule._authorIdTimerPairs[timerAuthorId];
		this.Timer.Dispose();
		await this.VotingChannel.DeleteMessageAsync(this.VotingMessage);
		VotingModule._authorIdVotingPairs.Remove(timerAuthorId);
		if (this.Gotis.Count == this.VoterCount)
		{
			await timer.MeetingChannel.MessageChannel.SendMessageAsync("全員お腹いっぱいなのでタイマーを解除しました。");
			OkawariTimerModule._authorIdTimerPairs.Remove(timerAuthorId);
			return;
		}

		if (!botSetting.IsAutomaticExtension)
		{
			timer.ExtentionTimerMessage = await timer.MeetingChannel.MessageChannel.SendMessageAsync(
				$"<@!{timerAuthorId}>何分延長するかを選んでください。", components: this.GetExtentionTimeComponent());
			return;
		}
		int extentionMilliSecond = botSetting.AutomaticExtensionSecond * 1000;
		timer.Timer = new System.Timers.Timer(1000);
		timer.StartTimer(timerAuthorId, extentionMilliSecond);
		timer.TimerMessage = await timer.MeetingChannel.MessageChannel.SendMessageAsync($"追加の{Time.GetTimeString(extentionMilliSecond)}タイマーを開始しました。", components: TimerComponent.Get(false));
	}
	/// <summary>
	/// 延長時間を選べるメニューを返す。
	/// </summary>
	/// <returns>メニュー(メッセージコンポーネント)</returns>
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
	/// <summary>
	/// 投票用の埋め込みを作成し、返す。
	/// </summary>
	/// <param name="timer">時間が切れたタイマー</param>
	/// <param name="botSetting">botの設定</param>
	/// <returns>投票用の埋め込み</returns>
	public async Task<Embed> GetVotingEmbed(OkawariTimer timer, BotSetting botSetting)
	{
		string mentionMessage = await timer.MeetingChannel.GetVoiceChannelUsersMentionMessage();
		var builder = new EmbedBuilder()
		{
			Title = "タイマーが終了しました",
			Description = $"【投票できる人】\n{mentionMessage}\n\n",
			Color = Color.Red
		};
		builder.AddField(new EmbedFieldBuilder()
		{
			Name = $"【{EmotePuls.Parse(botSetting.okawariEmojiId)}に投票した人】",
			Value = this.GetOkawariString(),
			IsInline = true
		});
		builder.AddField(new EmbedFieldBuilder()
		{
			Name = $"【{EmotePuls.Parse(botSetting.gotiEmojiId)}に投票した人】",
			Value = this.GetGotiString(),
			IsInline = true
		});
		builder.Description +=
			$"{EmotePuls.Parse(botSetting.okawariEmojiId)} or {EmotePuls.Parse(botSetting.gotiEmojiId)}\n\n" +
			$"{Time.GetTimeString(botSetting.VotingTimeLimitSecond * 1000)}以内に投票してください。";
		return builder.Build();
	}
	/// <summary>
	/// ごち勢を列挙した文字列を返す。
	/// </summary>
	/// <returns>ゴチ勢を列挙した文字列</returns>
	private string GetGotiString()
	{
		string gotiString = this.GetUsersMentionString(this.Gotis);
		if (string.IsNullOrEmpty(gotiString))
		{
			gotiString = "まだ誰もごちそうさましていません。";
		}
		return gotiString;
	}
	/// <summary>
	/// おかわり勢を列挙した文字列を返す。
	/// </summary>
	/// <returns>おかわり勢を列挙した文字列</returns>
	private string GetOkawariString()
	{
		string okawariString = this.GetUsersMentionString(this.Okawaris);
		return okawariString += "・投票していない人全員\n";
	}
	/// <summary>
	/// ユーザIdのリスト内の人を全てメンションしたメッセージを返す。
	/// </summary>
	/// <param name="userIds">ユーザIdのあるリスト</param>
	/// <returns>ユーザIdのリスト内の人を全てメンションしたメッセージ</returns>
	private string GetUsersMentionString(IReadOnlyList<ulong> userIds)
	{
		string result = "";
		foreach (ulong userId in userIds)
		{
			result += $"・<@!{userId}>\n";
		}
		return result;
	}
}
