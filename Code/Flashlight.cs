using Sandbox;

public sealed class Flashlight : Component
{
	[Property] public SpotLight Light { get; set; }
	protected override void OnUpdate()
	{
		WorldPosition = Scene.Camera.WorldPosition + Scene.Camera.WorldRotation.Forward * 40;
		WorldRotation = Scene.Camera.WorldRotation;

		if ( Input.Pressed( "flashlight" ) )
		{
			Light.Enabled = !Light.Enabled;
			var snd = Sound.Play("sounds/flashlight-click.sound");
			snd.Position = Scene.Camera.WorldPosition;
		}
	}
}
