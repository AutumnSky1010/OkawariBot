using System;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using OkawariBot.Settings;

namespace OkawariBot.Timer;
[Group("setting", "設定コマンド")]
public class SettingModule : InteractionModuleBase
{
	private SettingJson settingJson = new SettingJson(@"settings.json");
	[SlashCommand("emoji", "おかわりorごち絵文字を設定します。")]
	public async Task SetEmoji([Choice("おかわり", "okawari"),Choice("ごち", "goti")]string okawariOrGoti, string emojiId)
	{
		if (!Emote.TryParse(emojiId, out Emote emote))
		{
			await this.RespondAsync("これは絵文字ではありません。", ephemeral: true);
			return;
		}
		BotSetting botSetting = this.settingJson.Deserialize();
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
		this.settingJson.Serialize(botSetting);
	}
	[SlashCommand("voting_time", "投票の待ち時間を設定します。")]
	public async Task SetVotingTime(int minute = 0, int second = 0)
	{
		second = minute * 60 + second;
		BotSetting botSetting = this.settingJson.Deserialize();
		botSetting.VotingTimeLimitSecond = second;
		this.settingJson.Serialize(botSetting);
		await this.RespondAsync($"{Time.GetTimeString(second*1000)}に設定しました。", ephemeral: true);
	}
	[SlashCommand("automatic_extension_time", "自動延長の時間を設定します。")]
	public async Task SetAutomaticExtentionTime(int minute = 0, int second = 0)
	{
		second = minute * 60 + second;
		BotSetting botSetting = this.settingJson.Deserialize();
		botSetting.AutomaticExtensionSecond = second;
		this.settingJson.Serialize(botSetting);
		await this.RespondAsync($"{Time.GetTimeString(second * 1000)}に設定しました。", ephemeral: true);
	}
	[SlashCommand("automatic_extension", "自動延長をするかを設定します。")]
	public async Task SetIsAutomaticExtend([Choice("する", "true"), Choice("しない", "false")]string tOrF)
	{
		BotSetting botSetting = this.settingJson.Deserialize();
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
		this.settingJson.Serialize(botSetting);
	}
	[SlashCommand("time_out_message", "タイマー停止時のTTSメッセージを設定します。")]
	public async Task SetTimeOutMessage(string content)
	{
		BotSetting botSetting = this.settingJson.Deserialize();
		botSetting.TimeOutMessage = content;
		this.settingJson.Serialize(botSetting);
		await this.RespondAsync($"{content}に設定しました。", ephemeral: true);
	}
	[SlashCommand("show", "設定を確認できるメッセージを送信します。")]
	public async Task SendSettingMesage()
	{
		BotSetting botSetting = this.settingJson.Deserialize();
		string isAutomaticExtention = botSetting.IsAutomaticExtension ? "する" : "しない";
		string description =
			$"タイマー停止時のＴＴＳメッセージ：{botSetting.TimeOutMessage}\n\n" +
			$"投票の待ち時間　　　　　　　　　：{Time.GetTimeString(botSetting.VotingTimeLimitSecond * 1000)}\n\n" +
			$"おかわり絵文字　　　　　　　　　：{Emote.Parse(botSetting.okawariEmojiId)}\n\n" +
			$"ごち絵文字　　　　　　　　　　　：{Emote.Parse(botSetting.gotiEmojiId)}\n\n" +
			$"自動延長をするか　　　　　　　　：{isAutomaticExtention}\n\n" +
			$"自動延長した時の延長時間　　　　：{Time.GetTimeString(botSetting.AutomaticExtensionSecond * 1000)}";
		var embed = new EmbedBuilder()
		{
			Title = "現在の設定",
			Description = description,
			Color = Color.Orange
		};
		await RespondAsync(embed: embed.Build(), ephemeral: true);
	}
}
