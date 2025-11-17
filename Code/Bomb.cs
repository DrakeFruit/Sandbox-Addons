using Sandbox;

public sealed class Bomb : Component, Component.IDamageable
{
	[Property] public GameObject ExplosionPrefab { get; set; }
	[Property] public float Damage { get; set; }
	[Property] public float Radius { get; set; }

	public void OnDamage( in DamageInfo damage )
	{
		var expl = ExplosionPrefab.Clone();
		expl.WorldPosition = WorldPosition;
		
		foreach ( var i in Scene.Components.GetAll<IDamageable>() )
		{
			i.OnDamage(new DamageInfo( Damage, damage.Attacker, GameObject ));
		}
		
		GameObject.Destroy();
	}
}
