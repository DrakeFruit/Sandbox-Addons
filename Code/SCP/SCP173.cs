using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Internal;

namespace Sandbox.SCP;

public class SCP173 : Component
{
	public NetList<Observer> Observers = [];
	
	[RequireComponent] public NavMeshAgent Agent { get; set; }
	public PlayerController NearestPlayer { get; set; }
	public SoundHandle ScrapeSnd;

	public bool IsObserved
	{
		get
		{
			if ( Observers.Count > 0 ) return true;
			return false;
		}
	}
	
	protected override void OnFixedUpdate()
	{
		foreach ( var i in Scene.GetAll<PlayerController>() )
		{
			i.GetOrAddComponent<Observer>();
		}

		var players = Scene.GetAll<PlayerController>();
		NearestPlayer = players.FirstOrDefault();
		foreach ( var i in players )
		{
			if ( NearestPlayer.IsValid() && (i.WorldPosition - WorldPosition).Length < (NearestPlayer.WorldPosition - WorldPosition).Length )
			{
				NearestPlayer = i;
			}
		}
		if ( !IsObserved && NearestPlayer.IsValid() )
		{
			Agent.MoveTo(NearestPlayer.WorldPosition);
			WorldRotation = Rotation.LookAt( NearestPlayer.WorldPosition - WorldPosition ).Angles().WithPitch( 0 );
			
			if ( (NearestPlayer.WorldPosition - WorldPosition).Length <= 75 )
			{
				var component = NearestPlayer.Components.Get( GlobalGameNamespace.TypeLibrary.GetType( "Player" ).TargetType );
				if ( component is IDamageable player )
				{
					player.OnDamage( new DamageInfo
					{
						Damage = 999999, Attacker = GameObject, Position = component.WorldPosition,
					} );
					/*var neckSnapSnd = Sound.Play("sounds/neck-snap.sound");
					neckSnapSnd.Position = component.WorldPosition;*/
				}
			}
		}
		else
		{
			Agent.Velocity = 0;
			Agent.Stop();
		}

		/*if ( ScrapeSnd.IsValid() )
		{
			ScrapeSnd.Position = WorldPosition;
		}
		if ( !ScrapeSnd.IsValid() && Agent.Velocity.Length > 0 )
		{
			ScrapeSnd = Sound.Play( "sounds/concrete-scrape.sound" );
		}
		if ( ScrapeSnd.IsValid() && Agent.Velocity.Length.AlmostEqual( 0 ) )
		{
			ScrapeSnd.Stop();
		}*/
	}
	
	[Rpc.Broadcast]
	public void AddObserver(Observer observer)
	{
		if (!Observers.Contains(observer))
		{
			Observers.Add(observer);
		}
	}

	[Rpc.Broadcast]
	public void RemoveObserver(Observer observer)
	{
		if (Observers.Contains(observer))
		{
			Observers.Remove(observer);
		}
	}
}
