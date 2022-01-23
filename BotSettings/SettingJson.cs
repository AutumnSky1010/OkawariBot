
using System.Text.Unicode;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace OkawariBot.Settings;
internal class SettingJson
{
	public SettingJson(string jsonPath)
	{
		this.InitializeSerializerOptions();
		if (!File.Exists(jsonPath))
		{
			var jsonFile = File.Create(jsonPath);
			jsonFile.Close();
			jsonFile.Dispose();
		}
		this.JsonPath = jsonPath;
	}
	public string JsonPath { get; set; }
	private JsonSerializerOptions _jsonSerializerOptions { get; set; } = new JsonSerializerOptions();
	public void Serialize(BotSetting botSetting)
	{
		using (var sw = new StreamWriter(this.JsonPath))
		{
			string jsonString = JsonSerializer.Serialize(botSetting, this._jsonSerializerOptions);
			sw.Write(jsonString);
		}
	}
	public BotSetting Deserialize()
	{
		using (var sr = new StreamReader(this.JsonPath))
		{
			string jsonString = sr.ReadToEnd();
			BotSetting? botSetting = JsonSerializer.Deserialize<BotSetting>(jsonString, this._jsonSerializerOptions);
			return botSetting is null ? new BotSetting() : botSetting;
		}
	}
	private void InitializeSerializerOptions()
	{
		var option = new JsonSerializerOptions()
		{
			WriteIndented = true,
			Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
		};
		this._jsonSerializerOptions = option;
	}
}
