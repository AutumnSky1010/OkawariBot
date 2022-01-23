namespace OkawariBot;
internal class Time
{
	public static string GetTimeString(int milliSecond)
	{
		int second = milliSecond / 1000;
		if (second < 60)
		{
			return $"{second}秒";
		}
		int minute = second / 60;
		second = second % 60;
		return $"{minute}分{second}秒";
	}
}
