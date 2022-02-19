using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OkawariBot.Channels;
public class MeetingState
{
	public MeetingState(MeetingStateType state)
	{
		this.State = state;
	}
	private IReadOnlyDictionary<string, string> _meetingStates = new Dictionary<string, string>()
	{
		{ "AutomaticExtended", "おかわりした人が一人以上居たのでボットによって自動延長されました。" },
		{ "ExtendedByAuthor", "タイマー作成者によって延長されました。" },
		{ "StartedByAuthor", "タイマー作成者によって開始されました。"}
	};
	public enum MeetingStateType
	{
		AutomaticExtended,
		ExtendedByAuthor,
		StartedByAuthor
	}
	public MeetingStateType State { get; set; } = MeetingStateType.StartedByAuthor;
	public string GetString() => this._meetingStates[this.State.ToString()];
}
