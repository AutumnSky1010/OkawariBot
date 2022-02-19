
using Discord;

namespace OkawariBot;
public static class EmoteParser
{
	/// <summary>
	/// エモート(カスタム絵文字)ないし絵文字を表すIdの場合、そのエモートないし絵文字を返す。
	/// 失敗時は:no_entry_sign:の絵文字が返る
	/// </summary>
	/// <param name="emoteId">エモート(カスタム絵文字)または絵文字を表すId</param>
	/// <param name="emote">エモート(カスタム絵文字)または絵文字を格納する変数</param>
	/// <returns>成功:true</returns>
	public static bool TryParse(string emoteId, out IEmote emote)
	{
		bool isEmote = Emote.TryParse(emoteId, out Emote tryEmote);
		bool isNotEmoji = !(Emoji.TryParse(emoteId, out Emoji tryEmoji) || isEmote);
		emote = Emoji.Parse(":no_entry_sign:");
		if (isNotEmoji) { return false; }
		emote = tryEmoji is null ? tryEmote : tryEmoji;
		return true;
	}
	/// <summary>
	/// エモート(カスタム絵文字)ないし絵文字を表すIdを元に、そのエモートないし絵文字を返す。
	/// 失敗時は:no_entry_sign:の絵文字が返る
	/// </summary>
	/// <param name="emoteId">エモート(カスタム絵文字)または絵文字を表すId</param>
	/// <returns>エモート(カスタム絵文字)または絵文字を格納する変数</returns>
	public static IEmote Parse(string emoteId)
	{
		TryParse(emoteId, out IEmote tryEmote);
		return tryEmote;
	}
}
