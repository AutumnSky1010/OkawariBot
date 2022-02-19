
namespace OkawariBot;
internal static class Mention
{
	/// <summary>
	/// メッセージの一番最初にメンションしているメッセージからメンション先のユーザのIDを取得する。
	/// </summary>
	/// <param name="mentionMessageBeginning">メッセージの文章</param>
	/// <returns>成功：ユーザID、失敗：0</returns>
	public static ulong Parse(string mentionMessageBeginning)
	{
		int startIndex = mentionMessageBeginning.IndexOf('<') + 3;
		int endIndex = mentionMessageBeginning.IndexOf('>');
		var range = new Range(startIndex, endIndex);
		ulong id = 0;
		if (ulong.TryParse(mentionMessageBeginning[startIndex..endIndex], out id)) { }
		return id;
	}
}
