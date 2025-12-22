using Sandbox;
using Sandbox.UI;

public sealed class ChatManager : Component
{
	public string TextEntryStyle, ChatStyle, ChatString, ChatStringLog = "", CurrentTextEntry;
	public List<string[]> ChatList = new();
	public bool ShouldFocus = true, MessageSubmitted;																			//chat log (capped at 2000 chars)
	public float ChatTimer;

	private int _maxChatMessages = 50;

	protected override void OnStart()
	{
		InitChat();
	}

	protected override void OnUpdate()
	{
		UpdateTimers();
	}

	private void UpdateTimers()
	{
		if(ChatTimer > 0 )ChatTimer -= Time.Delta;
	}

	[Rpc.Owner]
	public void SendChatMessage( string name, string message )
	{
		ChatList.Add( [name, message] );
		
		if(ChatList.Count >= _maxChatMessages) ChatList.RemoveAt(0);
		
		SetChatLog(CreateStringFromList( ChatList ));
		ShowChatForEveryone( 4f );
	}

	[Rpc.Host]
	private void InitChat()
	{
		SetChatLog(CreateStringFromList( ChatList ));
	}

	private string CreateStringFromList( List<string[]> list )
	{
		string[] temparray = new string[list.Count];
		string tempstring = "";
		for ( int i = 0; i < list.Count; i++ )
		{
			temparray[i] = list[i][0] + "█" +  list[i][1];
		}

		for ( int i = 0; i < temparray.Length; i++ )
		{
			tempstring += temparray[i];
			if(i != temparray.Length - 1) tempstring += "§";
		}
		
		return tempstring;
	}

	[Rpc.Broadcast]
	private void SetChatLog( string log )
	{
		ChatStringLog = log;
		if( log == "" ) return;
		
		ChatList.Clear();
		var tempArray = log.Split( "§" );
		for ( int i = 0; i < tempArray.Length; i++ )
		{
			ChatList.Add( tempArray[i].Split( "█" ) );
		}
	}

	[Rpc.Broadcast]
	private void ShowChatForEveryone( float time )
	{
		ShouldFocus = false;
		ChatTimer = time;
	}
}
