using System;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;

public sealed class Seal : Component
{
	[RequireComponent] private Rigidbody Rb { get; set; }
	PlayerController ClosestPlayer { get; set; }
	private bool IsGalumphing { get; set; }
	
	protected override void OnFixedUpdate()
	{
		var players = Scene.GetAllComponents<PlayerController>();
		ClosestPlayer = players.FirstOrDefault();
		if ( ClosestPlayer != null )
		{
			foreach ( var p in players )
			{
				if ( (p.WorldPosition - WorldPosition).Length < (ClosestPlayer.WorldPosition - WorldPosition).Length )
				{
					ClosestPlayer = p;
				}
			}
			Rb.SmoothRotate(Rotation.LookAt(ClosestPlayer.WorldPosition - WorldPosition).Angles().WithPitch(0), .025f, Time.Delta);
			var tr = Scene.Trace.Ray( WorldPosition + Vector3.Up * 5, WorldPosition + WorldRotation.Down * 10 ).IgnoreGameObjectHierarchy(GameObject).Run();
			if ( !IsGalumphing && tr.Hit ) _ = Galumph();
		}
	}

	private async Task Galumph()
	{
		IsGalumphing = true;
		await Task.DelayRealtimeSeconds( .5f + Random.Shared.Float(-.15f, .15f) );
		Rb.ApplyImpulse(((ClosestPlayer.WorldPosition - WorldPosition).Normal + Vector3.Up * 2) * 15000);
		if ( Random.Shared.Int( 0, 10 ) == 2 )
		{
			var snd = Sound.Play( "sounds/gyu.sound" );
			snd.Position = WorldPosition;
		}
		IsGalumphing = false;
	}
}
