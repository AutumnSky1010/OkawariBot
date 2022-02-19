using System;
using Discord;
using Discord.WebSocket;
using Discord.Interactions;

using OkawariBot.Channels;
using OkawariBot.Settings;

namespace OkawariBot.Timer;
public class OkawariTimerModule : InteractionModuleBase
{
	internal static Dictionary<ulong, OkawariTimer> _authorIdTimerPairs = new Dictionary<ulong, OkawariTimer>();
	[SlashCommand("start_timer", "タイマーをセットし、開始します。")]
	public async Task CreateTimer(
		[Summary(description: "タイマーが鳴るまでのトピック")] string topic = "未設定", 
		[Summary(description: "タイマーが鳴るまでの分数(分と秒を同時に指定可能)")] int minute = 0,
		[Summary(description: "タイマーが鳴るまでの秒数(分と秒を同時に指定可能)")] int second = 0)
	{
		if (second + minute * 60 <= 0)
		{
			await this.RespondAsync("タイマーの時間は1秒以上に設定してください。", ephemeral: true);
			return;
		}
		// タイマーの作者
		var author = this.Context.User as SocketGuildUser;
		// タイマーの作者がボイスチャンネルに参加していない場合
		if (!await this.IsJoinedVoiceChannel(author)) { return; }
		// 既にタイマーを作成している場合
		if (await this.IsRegistered()) { return; }
		// タイマーをスタートする
		IUserMessage timerMessage = await this.ReplyAsync(
			$"{Time.GetTimeString((second + minute * 60)*1000)}のタイマーを開始します。", 
			components:TimerComponent.Get(false));
		var timer = new OkawariTimer(author, this.Context.Channel, timerMessage);
		await timer.MeetingChannel.TrySetTopic(topic);
		timer.StartTimer(this.Context.User.Id, (second + minute * 60) * 1000);
		this.AddAuthorIdTimerPairs(timer);
		await this.RespondAsync("ボタンでタイマーを操作できます。");
	}
	[SlashCommand("control_panel", "タイマーの操作ボタンを送信します。")]
	public async Task SendControlPanel()
	{
		if (!_authorIdTimerPairs.ContainsKey(this.Context.User.Id))
		{
			await RespondAsync("タイマーを作成していません。", ephemeral: true);
			return;
		}
		OkawariTimer timer = _authorIdTimerPairs[this.Context.User.Id];
		try
		{
			await timer.MeetingChannel.MessageChannel.DeleteMessageAsync(timer.TimerMessage);
		}
		catch { }
		timer.TimerMessage = await timer.MeetingChannel.MessageChannel.SendMessageAsync("タイマーの操作パネル", components: TimerComponent.Get(timer.IsPause));
		await this.RespondAsync("ボタンでタイマーを操作できます。");
	}
	[ComponentInteraction("stop")]
	public async Task OnStopButtonClick()
	{
		if (!_authorIdTimerPairs.ContainsKey(this.Context.User.Id))
		{
			await RespondAsync("タイマーを作成していないため、停止出来ませんでした。", ephemeral: true);
			return;
		}
		await this.RespondAsync("タイマーを終了しました。");
		OkawariTimer timer = _authorIdTimerPairs[this.Context.User.Id];
		await timer.OnTimeOut(this.Context.User.Id);
	}
	[ComponentInteraction("pause")]
	public async Task OnPauseButtonClick()
	{
		if (!_authorIdTimerPairs.ContainsKey(this.Context.User.Id))
		{
			await RespondAsync("タイマーを作成していないため、一時停止出来ませんでした。", ephemeral:true);
			return;
		}
		OkawariTimer timer = _authorIdTimerPairs[this.Context.User.Id];
		timer.Timer.Stop();
		timer.IsPause = true;
		await timer.TimerMessage.ModifyAsync((msg) => msg.Components = TimerComponent.Get(true));
		await this.RespondAsync($"タイマーを一時停止しました。");
	}
	[ComponentInteraction("restart")]
	public async Task OnRestartButtonClick()
	{
		if (!_authorIdTimerPairs.ContainsKey(this.Context.User.Id))
		{
			await RespondAsync("タイマーを作成していないため、一時停止出来ませんでした。", ephemeral: true);
			return;
		}
		OkawariTimer timer = _authorIdTimerPairs[this.Context.User.Id];
		timer.IsPause = false;
		timer.Timer.Start();
		await timer.TimerMessage.ModifyAsync((msg) => msg.Components = TimerComponent.Get(false));
		await this.RespondAsync($"タイマーを再開しました。");
	}
	[ComponentInteraction("ExtentionTimeMenu")]
	public async Task OnSelectExtentionTimeMenu(string[] selectedMenu)
	{
		var component = this.Context.Interaction as SocketMessageComponent;
		ulong timerAuthorId = Mention.Parse(component.Message.Content);
		if (this.Context.User.Id != timerAuthorId)
		{
			await this.RespondAsync("タイマーの作成者のみ延長できます。", ephemeral:true);
			return;
		}
		OkawariTimer timer = _authorIdTimerPairs[timerAuthorId];
		await timer.MeetingChannel.MessageChannel.DeleteMessageAsync(timer.ExtentionTimerMessage);
		if (selectedMenu[0] == "none")
		{
			await this.RespondAsync("延長をしません。");
			_authorIdTimerPairs.Remove(timerAuthorId);
			return;
		}
		await timer.Extend(int.Parse(selectedMenu[0]) * 60 * 1000, timerAuthorId, MeetingState.MeetingStateType.ExtendedByAuthor);
		await this.RespondAsync();
	}
	private async Task<bool> IsJoinedVoiceChannel(SocketGuildUser author)
	{
		if (author.VoiceChannel is null)
		{
			await RespondAsync("ボイスチャットに参加してからコマンドを打ってください。");
			return false;
		}
		return true;
	}
	private async Task<bool> IsRegistered()
	{
		if (_authorIdTimerPairs.ContainsKey(this.Context.User.Id))
		{
			await this.RespondAsync("既にタイマーを作っているので作ることができませんでした。");
			return true;
		}
		return false;
	}
	private void AddAuthorIdTimerPairs(OkawariTimer timer) => _authorIdTimerPairs.Add(this.Context.User.Id, timer);
}
