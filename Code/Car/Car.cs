using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

public sealed class Car : Component
{
	[Property] public float TireRestDistance { get; set; } = 38;
	[Property] public float SpringStrength { get; set; } = 30;
	[Property] public float SpringDamping { get; set; } = 1500;
	[Property] public float TireGrip { get; set; } = 200;
	[Property] public float TireMass { get; set; } = 10;
	[Property] public float MaxSpeedMPH { get; set; } = 90;
	public float MaxSpeed { get { return MaxSpeedMPH * 17.6f; } }
	[Property] public float TurnSpeed { get; set; } = 10;
	[Property] public Curve TorqueCurve { get; set; }
	[Property] public List<GameObject> FrontTires { get; set; }
	[Property] public List<GameObject> BackTires { get; set; }

	[RequireComponent] public Rigidbody Rb { get; set; }
	[RequireComponent] public BaseChair Chair { get; set; }
	
	private SceneTraceResult TireTrace { get; set; }
	private bool WasInCar { get; set; }
	
	protected override void OnFixedUpdate()
	{
		if ( IsProxy ) return;
		DebugOverlay.ScreenText( new Vector2( 120, 20 ), (Rb.Velocity.Length / 17.6f).Floor().ToString() + " mph", 40,
			TextFlag.Absolute, Color.Orange );
		foreach ( var i in FrontTires )
		{
			ApplySuspension( i );
			ApplyTorque( i );
			if ( Chair.IsOccupied )
			{
				if ( !WasInCar )
				{
					/*var doorSnd = Sound.Play( "sounds/cars/door-close.sound" );
					var engineStartSnd = Sound.Play( "sounds/cars/engine-start.sound" );
					doorSnd.Parent = GameObject;
					engineStartSnd.Parent = GameObject;
					doorSnd.FollowParent = true;
					engineStartSnd.FollowParent = true;*/
				}
				
				//rotate wheel for steering
				i.LocalRotation *= Rotation.Identity.Angles().WithYaw( Input.AnalogMove.y * (TurnSpeed * 0.1f) );
				i.LocalRotation = i.LocalRotation.Clamp( Rotation.FromYaw( 0 ), 45 );
				DebugOverlay.ScreenText( new Vector2( 20, 20 ), (-i.LocalRotation.Yaw()).Floor().ToString(), 40,
					TextFlag.Absolute, Color.Orange );
				//slowly rotate the wheel back to neutral
				if ( Input.AnalogMove.y.AlmostEqual( 0 ) )
				{
					var dif = Rotation.Difference( i.LocalRotation, Rotation.Identity );
					i.LocalRotation *= dif / 20;
				}
				
				WasInCar = true;
			}
			else WasInCar = false;
		}
		foreach ( var i in BackTires )
		{
			ApplySuspension( i );
		}
	}

	public void ApplySuspension( GameObject tire )
	{
		TireTrace = Scene.Trace.Ray( tire.WorldPosition + tire.WorldRotation.Up * 20,
				tire.WorldPosition + tire.WorldRotation.Down * TireRestDistance )
			.IgnoreGameObjectHierarchy(GameObject)
			.Run();
		//DebugOverlay.Trace(TireTrace);

		if ( TireTrace.Hit )
		{
			var tireWorldVelocity = Rb.GetVelocityAtPoint( tire.WorldPosition );
			var offset = TireRestDistance - TireTrace.Distance;
			
			var springDir = tire.WorldRotation.Up;
			var springVel = Vector3.Dot( springDir, tireWorldVelocity );
			var springForce = (offset * (SpringStrength * Rb.Mass)) - (springVel * SpringDamping);
			
			var slipDir = tire.WorldRotation.Left;
			var slipVel = Vector3.Dot( slipDir, tireWorldVelocity );
			var slipForce = -slipVel * TireGrip;

			Rb.ApplyForceAt( tire.WorldPosition, springDir * springForce );
			Rb.ApplyForceAt( tire.WorldPosition, slipDir * TireMass * slipForce );

			var model = tire.Children.FirstOrDefault();
			if ( model.IsValid() && TireTrace.Distance < TireRestDistance * 0.75f ) model.WorldPosition = TireTrace.HitPosition;
			
			//DebugOverlay.Line(tire.WorldPosition, tire.WorldPosition + springDir * springForce, Color.Blue);
			//DebugOverlay.Line(tire.WorldPosition, tire.WorldPosition + slipDir * TireMass * slipForce, Color.Red);
		}
	}

	public void ApplyTorque( GameObject tire )
	{
		if ( TireTrace.Hit )
		{
			var tireWorldVelocity = Rb.GetVelocityAtPoint( tire.WorldPosition );
			var driveDir = tire.WorldRotation.Forward;
			var driveVel = Vector3.Dot( driveDir, tireWorldVelocity );
			
			var driveVelNormal = float.Clamp(MathF.Abs( driveVel ) / MaxSpeed, 0, 1);
			var driveForce = TorqueCurve.Evaluate( driveVelNormal ) * Input.AnalogMove.x * 200000;
			if ( !Chair.IsOccupied || Input.AnalogMove.x <= 0 )
			{
				driveForce = -driveVel * 1000;
			}

			if ( driveVel >= MaxSpeed ) driveForce = 0;
		
			Rb.ApplyForceAt( tire.WorldPosition, driveDir * driveForce );
			//DebugOverlay.Line(tire.WorldPosition, tire.WorldPosition + driveDir * driveForce, Color.Green);
		}
	}
}
