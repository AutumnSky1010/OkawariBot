using System;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using OkawariBot.Timer;
using OkawariBot.Settings;

namespace OkawariBot.Voting;
public class VotingModule : InteractionModuleBase
{
	internal static Dictionary<ulong, Voting> _authorIdVotingPairs = new Dictionary<ulong, Voting>();
	private SettingJson _settingJson = new SettingJson("settings.json");
	[ComponentInteraction("okawari")]
	public async Task Okawari()
	{
		if (!await this.CanVote()) { return; }
		await this.Vote();
		await this.RespondAsync("おかわりしました。", ephemeral: true);
	}
	[ComponentInteraction("goti")]
	public async Task Goti()
	{
		if (!await this.CanVote()) { return; }
		await this.Vote(true);
		await this.RespondAsync("ごちそうさましました。", ephemeral: true);
	}
	private async Task<bool> CanVote()
	{
		var component = this.Context.Interaction as SocketMessageComponent;
		ulong timerAuthorId = MentionId.Parse(component.Message.Content);
		OkawariTimer timer = OkawariTimerModule._authorIdTimerPairs[timerAuthorId];
		List<ulong> userIds = await timer.MeetingChannel.GetVoiceChannelUserIds();
		if (userIds.Contains(this.Context.User.Id))
		{
			return true;
		}
		await this.RespondAsync("参加していないので投票できませんでした。", ephemeral:true);
		return false;
	}
	private async Task Vote(bool isGoti = false)
	{
		var component = this.Context.Interaction as SocketMessageComponent;
		ulong timerAuthorId = MentionId.Parse(component.Message.Content);
		Voting voting = _authorIdVotingPairs[timerAuthorId];
		OkawariTimer timer = OkawariTimerModule._authorIdTimerPairs[timerAuthorId];
		BotSetting setting = this._settingJson.Deserialize();
		voting.TryRemoveId(this.Context.User.Id);
		if (isGoti)
		{
			voting.Gotis.Add(this.Context.User.Id);
		}
		else
		{
			voting.Okawaris.Add(this.Context.User.Id);
		}
		await voting.UpdateVotingEmbed(timer, setting);
	}
}
