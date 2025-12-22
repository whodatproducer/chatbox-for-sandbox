using Sandbox;

public sealed class PunchyPlayer : Component
{
	private const int PunchForce = 800, PunchRange = 75, SlideSpeed = 800;
	private const double SlideAnimationTime = .3, SlideCooldownTime = 1.25;
	private double _slideAnimationTimer, _slideCooldownTimer;
	
	private const double PunchTime = .25, PunchStanceCooldownTime = 1.5;
	private double _punchTimer, _punchStanceCooldownTimer;

	private readonly SoundEvent _punchSound = ResourceLibrary.Get<SoundEvent>("sounds/Punchy Player Sounds/punch.sound"),
		_missSound =  ResourceLibrary.Get<SoundEvent>("sounds/Punchy Player Sounds/miss.sound");
	
	private SkinnedModelRenderer _modelRenderer;
	private PlayerController _player;

	public PlayerChat _playerChat;
	

	protected override void OnStart()
	{
		SetVars();
		LoadSkin( GameObject );
	}

	protected override void OnUpdate()
	{
		UpdateTimers();
		
		CheckForInputs();
		
		if(WorldPosition.z < -3000) Teleport( new Vector3( 0,0,0 ) );
	}

	#region player controls
	
	[Rpc.Owner]
	private void Slide()
	{
		if ( _slideCooldownTimer > 0 ) return;

		Vector3 slideVector = Vector3.Forward * Scene.Camera.LocalRotation * SlideSpeed;
		
		_player.Jump(new Vector3( slideVector.x, slideVector.y, 0 ));
		AnimateSlide();
		
		_slideAnimationTimer = SlideAnimationTime;
		_slideCooldownTimer = SlideCooldownTime;
	}

	[Rpc.Owner]
	private void Punch()
	{
		if ( _punchTimer > 0 ) return;

		var startPos = _player.EyePosition;
		var endPos = startPos + (Vector3.Forward * Scene.Camera.WorldRotation * PunchRange);
		var traceCheckPlayer = Scene.Trace.Ray( startPos, endPos ).WithTag( "player" ).IgnoreGameObject( GameObject ).Run();
		var traceCheckPunchable = Scene.Trace.Ray( startPos, endPos ).WithTag( "punchable" ).IgnoreGameObject( GameObject ).Run();
		
		if ( traceCheckPlayer.Hit )
		{
			var playerThatWasPunched = traceCheckPlayer.GameObject.GetComponent<PunchyPlayer>(  );
			playerThatWasPunched.Push( Vector3.Forward * _player.EyeAngles * PunchForce);
			Sound.Play(  _punchSound , WorldPosition );
		}
		else if ( traceCheckPunchable.Hit )
		{
			Sound.Play(  _punchSound , WorldPosition );
		}
		else
		{
			Sound.Play( _missSound , WorldPosition );
		}
		
		AnimatePunch();
		_punchTimer = PunchTime;
		_punchStanceCooldownTimer = PunchStanceCooldownTime;
	}
	
	#endregion

	private void CheckForInputs()
	{
		if ( IsProxy ) return;
		
		//if ( Input.Pressed( "Slide" ) ) Slide();//add a slide control in project settings and uncomment this line if you want a slide ability

		if ( Input.Pressed( "Attack1" ) ) Punch();

		if ( Input.Pressed( "chat" ) ) _playerChat.ChatInputPressed();
	}
	
	[Rpc.Owner]
	private void UpdateTimers()
	{
		if ( _slideAnimationTimer > 0 ) _slideAnimationTimer -= Time.Delta;
		else ResetSlideStance();
		if ( _slideCooldownTimer > 0 ) _slideCooldownTimer -= Time.Delta;
		if( _punchTimer > 0 ) _punchTimer -= Time.Delta;
		if( _punchStanceCooldownTimer > 0 ) _punchStanceCooldownTimer -= Time.Delta;
		else ResetPunchStance();
	}
	
	[Rpc.Owner]
	private void SetVars()
	{
		_modelRenderer = GameObject.GetComponentInChildren<SkinnedModelRenderer>();
		_playerChat = GameObject.GetComponent<PlayerChat>();
		_player = GameObject.GetComponent<PlayerController>();
	}
	
	#region player modifiers
	
	[Rpc.Owner]
	private void Push( Vector3 pushVector )
	{
		GameObject.GetComponent<PlayerController>().Jump(pushVector);
	}

	[Rpc.Owner]
	private void Teleport( Vector3 coordsToTeleTo )
	{
		WorldPosition = coordsToTeleTo;
	}
	
	#endregion
	
	#region Animation methods
	
	[Rpc.Broadcast]
	public void Idle()
	{
		_modelRenderer?.Set( "special_idle_states", 1 );
	}
	
	[Rpc.Broadcast]
	public void DeIdle()
	{
		_modelRenderer?.Set( "special_idle_states", 0 );
	}
	
	[Rpc.Broadcast]
	private void AnimatePunch()
	{
		_modelRenderer?.Set( "holdtype", 5 );
		_modelRenderer?.Set( "b_attack", true );
	}

	[Rpc.Broadcast]
	private void ResetPunchStance()
	{
		_modelRenderer?.Set( "holdtype", 0 );
	}
	
	[Rpc.Broadcast]
	private void AnimateSlide()
	{
		_modelRenderer?.Set( "special_movement_states", 3 );
	}

	[Rpc.Broadcast]
	private void ResetSlideStance()
	{
		_modelRenderer?.Set( "special_movement_states", 0 );
	}
	
	[Rpc.Broadcast]
	private void LoadSkin(GameObject player)
	{
		player.GetComponent<Dresser>().Apply();
	}
	
	#endregion
	
}

