using System;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using OkawariBot.Settings;

namespace OkawariBot.Settings;
[Group("setting", "設定コマンド")]
public class SettingModule : InteractionModuleBase
{
	private SettingJson _settingJson = new SettingJson(@"settings.json");
	[SlashCommand("emoji", "おかわりorごち絵文字を設定します。")]
	public async Task SetEmoji(
		[Summary(description: "変更する絵文字を選択。")][Choice("おかわり", "okawari"),Choice("ごち", "goti")]string okawariOrGoti,
		[Summary(description: "絵文字、カスタム絵文字のID")] string emojiId)
	{
		if (!EmoteParser.TryParse(emojiId, out IEmote emote))
		{
			await this.RespondAsync("これは絵文字ではありません。", ephemeral: true);
			return;
		}

		BotSetting botSetting = this._settingJson.Deserialize();
		if (okawariOrGoti == "okawari")
		{
			botSetting.okawariEmojiId = emojiId;
			await this.RespondAsync($"おかわりの絵文字を{emote}にしました。", ephemeral: true);
		}
		else
		{
			botSetting.gotiEmojiId = emojiId;
			await this.RespondAsync($"ごちの絵文字を{emote}にしました。", ephemeral: true);
		}
		this._settingJson.Serialize(botSetting);
	}
	[SlashCommand("voting_time", "投票の待ち時間を設定します。")]
	public async Task SetVotingTime(
		[Summary(description: "投票の待ち時間(分と秒を同時に指定可能)")] int minute = 0,
		[Summary(description: "投票の待ち時間(分と秒を同時に指定可能)")] int second = 0)
	{
		second = minute * 60 + second;
		if (second <= 0)
		{
			await this.RespondAsync("投票の待ち時間は1秒以上で指定してください。", ephemeral: true);
			return;
		}
		BotSetting botSetting = this._settingJson.Deserialize();
		botSetting.VotingTimeLimitSecond = second;
		this._settingJson.Serialize(botSetting);
		await this.RespondAsync($"{Time.GetTimeString(second*1000)}に設定しました。", ephemeral: true);
	}
	[SlashCommand("automatic_extension_time", "自動延長の時間を設定します。")]
	public async Task SetAutomaticExtentionTime(
		[Summary(description: "自動延長時間(分と秒を同時に指定可能)")] int minute = 0,
		[Summary(description: "自動延長時間(分と秒を同時に指定可能)")] int second = 0)
	{
		second = minute * 60 + second;
		if (second <= 0)
		{
			await this.RespondAsync("自動延長の時間は1秒以上で指定してください。", ephemeral: true);
			return;
		}
		BotSetting botSetting = this._settingJson.Deserialize();
		botSetting.AutomaticExtensionSecond = second;
		this._settingJson.Serialize(botSetting);
		await this.RespondAsync($"{Time.GetTimeString(second * 1000)}に設定しました。", ephemeral: true);
	}
	[SlashCommand("automatic_extension", "自動延長をするかを設定します。")]
	public async Task SetIsAutomaticExtend(
		[Summary(description: "自動延長をするかしないか")][Choice("する", "true"), Choice("しない", "false")]string tOrF)
	{
		BotSetting botSetting = this._settingJson.Deserialize();
		if (tOrF == "true")
		{
			botSetting.IsAutomaticExtension = true;
			await this.RespondAsync("自動延長をするようにしました。", ephemeral: true);
		}
		else
		{
			botSetting.IsAutomaticExtension = false;
			await this.RespondAsync("自動延長をしないようにしました。", ephemeral: true);
		}
		this._settingJson.Serialize(botSetting);
	}
	[SlashCommand("time_out_message", "タイマー停止時のTTSメッセージを設定します。")]
	public async Task SetTimeOutMessage(
		[Summary(description: "送信するTTSメッセージの内容")] string content)
	{
		BotSetting botSetting = this._settingJson.Deserialize();
		botSetting.TimeOutMessage = content;
		this._settingJson.Serialize(botSetting);
		await this.RespondAsync($"{content}に設定しました。", ephemeral: true);
	}
	[SlashCommand("show", "設定を確認できるメッセージを送信します。")]
	public async Task SendSettingMesage()
	{
		BotSetting botSetting = this._settingJson.Deserialize();
		string notificationTimes = botSetting.NotificationTimes.Count == 0 ? "通知しない" : $"({string.Join(',', botSetting.NotificationTimes)})秒";
		string isAutomaticExtention = botSetting.IsAutomaticExtension ? "する" : "しない";
		string description =
			$"タイマー停止時のＴＴＳメッセージ：{botSetting.TimeOutMessage}\n\n" +
			$"投票の待ち時間　　　　　　　　　：{Time.GetTimeString(botSetting.VotingTimeLimitSecond * 1000)}\n\n" +
			$"おかわり絵文字　　　　　　　　　：{EmoteParser.Parse(botSetting.okawariEmojiId)}\n\n" +
			$"ごち絵文字　　　　　　　　　　　：{EmoteParser.Parse(botSetting.gotiEmojiId)}\n\n" +
			$"自動延長をするか　　　　　　　　：{isAutomaticExtention}\n\n" +
			$"自動延長した時の延長時間　　　　：{Time.GetTimeString(botSetting.AutomaticExtensionSecond * 1000)}\n\n" +
			$"残り時間を通知する時間　　　　　：{notificationTimes}";
		var embed = new EmbedBuilder()
		{
			Title = "現在の設定",
			Description = description,
			Color = Color.Orange
		};
		await this.RespondAsync(embed: embed.Build(), ephemeral: true);
	}
	[SlashCommand("set_notification_time", "残り時間を通知する時間を設定します。")]
	public async Task SetNotificationTime(
		[Summary(description: "「秒，秒，・・・，秒」のようにカンマ(半角)区切りで入力してください")] string commaSeparatedSecond)
	{
		if (!CSVSecond.IsAllInNumber(commaSeparatedSecond))
		{
			await this.RespondAsync("数字以外が含まれている、または「,」区切りでない可能性があります。", ephemeral: true);
			return;
		}
		BotSetting botSetting = this._settingJson.Deserialize();
		botSetting.NotificationTimes = CSVSecond.Parse(commaSeparatedSecond);
		this._settingJson.Serialize(botSetting);
		await this.RespondAsync("通知をする時間を設定しました。", ephemeral: true);
	}
	[SlashCommand("no_notification", "残り時間を通知しなくなります。")]
	public async Task RemoveNotificationTime()
	{
		BotSetting botSetting = this._settingJson.Deserialize();
		botSetting.NotificationTimes = new List<int>();
		this._settingJson.Serialize(botSetting);
		await this.RespondAsync("通知をする時間を削除しました。", ephemeral: true);
	}
}
