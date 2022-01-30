using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OkawariBot.Settings;
static class CSVSecond
{
	public static bool IsAllInNumber(string CSVSecond)
	{
		string seconds = CSVSecond.Replace(",", "");
		return seconds.All(char.IsDigit);
	}
	public static List<int> Parse(string CSVSecond)
	{
		var seconds = new List<int>(CSVSecond.Length / 2 + 1);
		string[] secondChars = CSVSecond.Split(',');
		foreach (string number in secondChars)
		{
			seconds.Add(int.Parse(number));
		}
		return seconds;
	}
}
