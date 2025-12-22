using Sandbox;

public sealed class PlayerChat : Component
{
	private PunchyPlayer _punchyPlayer;
	private ChatManager _chatManager;
	
	private bool _chatboxIsOpen, _openChatLocal;

	protected override void OnStart()
	{
		_punchyPlayer = GameObject.GetComponent<PunchyPlayer>();
		_chatManager = Scene.GetComponentInChildren<ChatManager>();
	}

	protected override void OnUpdate()
	{
		
		UpdateChat();

	}
	
	#region Chat methods for player script
	
	public void ChatInputPressed()
	{
		if ( _chatManager.TextEntryStyle != "textEntryVisible" ) OpenChat();
		if ( _chatManager.ChatTimer > 0 ) _chatManager.ChatTimer = 0;
	}

	private void OpenChat()
	{
		ShowChatbox(true);//the chatbox contains the chat log, the textEntryBox is where you type
		_openChatLocal = true;
		_punchyPlayer.Idle();	//enter idle stance while chatting
	}

	private void CloseChat()
	{
		if(_chatManager.CurrentTextEntry != "") SendChat( _chatManager.CurrentTextEntry );
		
		_chatManager.CurrentTextEntry = ""; 
		_openChatLocal = false;
		_chatboxIsOpen = false;
		_punchyPlayer.DeIdle(); //exit idle stance
		
		HideChatbox();
	}
	
	private void ShowChatbox(bool showTextEntryBox)
	{
		_chatManager.ChatString = _chatManager.ChatStringLog;
		_chatManager.ChatStyle = "chatboxVisible";
		_chatboxIsOpen = true;
		
		if(showTextEntryBox) _chatManager.TextEntryStyle = "TextEntryVisible";
	}
	
	private void HideChatbox()
	{
		_chatManager.ChatString = "";
		_chatManager.ChatStyle = "chatboxNotVisible"; //this doesnt actually correspond to any css. the string could really be anything besides "chatboxVisible"
		_chatManager.TextEntryStyle = "TextEntryNotVisible"; //same
	}

	private void SendChat( string message )
	{
		_chatManager.SendChatMessage( GameObject.Name.Remove( 0 , 9 ),  message );
	}

	private void UpdateChat()
	{//things to check every frame, probably not very resource friendly
		if ( IsProxy ) return;
		
		if(_chatManager.MessageSubmitted)
		{
			CloseChat();
			_chatManager.MessageSubmitted = false;
		}
			
		if ( _openChatLocal )																							//this is for when you open the chat to send a message
		{
			_chatManager.ShouldFocus = true;
			ShowChatbox( true );
		}
		else if ( _chatManager.ChatTimer > 0 )																					//this is for when chat stays open after a message is sent (by anyone)
		{
			_chatManager.ShouldFocus = false;
			ShowChatbox( false );
		}
		else if(_chatboxIsOpen) HideChatbox();
	}

	#endregion
}
