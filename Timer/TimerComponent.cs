using Discord;

namespace OkawariBot.Timer;
internal class TimerComponent
{
	internal static MessageComponent Get(bool isPause)
	{
		var builder = new ComponentBuilder();
		if (isPause)
		{
			builder.WithButton("再開", "restart", emote: Emoji.Parse(":arrow_forward:"), style: ButtonStyle.Success);
		}
		else
		{
			builder.WithButton("一時停止", "pause", emote: Emoji.Parse(":pause_button:"), style: ButtonStyle.Success);
		}
		return builder.WithButton("停止", "stop", emote: Emoji.Parse(":stop_button:"), style: ButtonStyle.Danger).Build();
	}
}
