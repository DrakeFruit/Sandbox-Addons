using System.Collections.Generic;
using System.Linq;

namespace Sandbox.SCP;

public class Observer : Component
{
	public bool IsEyesClosed { get; set; }
	public bool IsBlinking { get; set; }
	public bool NeedsToBlink { get; set; }

	protected override void OnFixedUpdate()
	{
		UpdateObservation();
	}
	
	public void UpdateObservation()
    {
        // When player is blinking or has eyes closed, they cannot observe SCPs
        if ( IsEyesClosed || IsBlinking )
        {
            // Inform all SCPs that this player is no longer observing them
            var scpsInScene = Game.ActiveScene.GetAllComponents<SCP173>();
            foreach ( var scp in scpsInScene.Where( s => s.Observers.Contains( this ) ) )
            {
                scp.RemoveObserver( this );
            }
            NeedsToBlink = false;
            return;
        }

        if ( !IsBlinking )
        {
            var visibleSCPs = new List<SCP173>();
            var eyePosition = Game.ActiveScene.Camera.WorldPosition;
            var eyeAngles = Game.ActiveScene.Camera.WorldRotation.Angles();
            var eyeForward = eyeAngles.ToRotation().Forward;

            var scpsInScene = Game.ActiveScene.GetAllComponents<SCP173>();

            foreach ( var scp in scpsInScene )
            {
                var scpBounds = scp.GameObject.GetBounds();
                var scpCenter = scpBounds.Center;
                
                // Check if SCP is within observation radius
                var distanceToSCP = Vector3.DistanceBetween( eyePosition, scpCenter );
                if ( distanceToSCP > 1000f )
                    continue;

                // Check if SCP is within viewing angle
                var directionToSCP = (scpCenter - eyePosition).Normal;
                var dotProduct = Vector3.Dot( eyeForward, directionToSCP );
                if ( dotProduct < 0.25f )
                    continue;

                // Trace to multiple points on the SCP's bounding box to check visibility
                bool isVisible = false;
                var corners = scpBounds.Corners.ToArray();
                
                // Check center first (most likely to be visible)
                var centerTrace = Game.ActiveScene.Trace.FromTo( eyePosition, scpCenter )
                    .IgnoreGameObject( GameObject )
                    .Run();
                
                if ( (!centerTrace.Hit) || (centerTrace.GameObject == scp.GameObject) )
                {
                    isVisible = true;
                }
                else
                {
                    // If center is blocked, check corners
                    foreach ( var corner in corners )
                    {
                        var cornerTrace = Scene.Trace.FromTo( eyePosition, corner )
                            .IgnoreGameObject( GameObject )
                            .Run();
                        
                        if ( (!cornerTrace.Hit) || (cornerTrace.GameObject == scp.GameObject) )
                        {
                            isVisible = true;
                            break;
                        }
                    }
                }

                if ( isVisible )
                {
                    NeedsToBlink = true;
                    scp.AddObserver( this );
                    visibleSCPs.Add( scp );
                }
            }

            // Remove observers from SCPs that are no longer visible
            foreach ( var scp in Game.ActiveScene.GetAllComponents<SCP173>().Where( s => s.Observers.Contains( this ) && !visibleSCPs.Contains( s ) ) )
            {
                scp.RemoveObserver( this );
            }

            if ( !visibleSCPs.Any() ) NeedsToBlink = false;
        }
    }
}
