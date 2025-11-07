using System.Linq;
using Sandbox;
using Sandbox.Internal;
using Sandbox.UI;
using System;
using NativeEngine;

public class PPEditor : Component
{
	public Panel Tab;
	public Panel Body;
	public TypeDescription Comp;
	
	protected override void OnStart()
	{
		var type = GlobalGameNamespace.TypeLibrary.GetType( "SpawnMenuHost" );
		var comp = Scene.GetAllComponents( type.TargetType ).FirstOrDefault();
		if ( type.IsValid && Comp != null && comp is PanelComponent panelComp )
		{
			var tabs = panelComp.Panel.Descendants.FirstOrDefault(x => x.HasClass("tabs") && x.Parent.HasClass("spawnmenuleft"));
			var body = panelComp.Panel.Descendants.FirstOrDefault(x => x.HasClass("body") && x.Parent.HasClass("spawnmenuleft"));
			if ( tabs.IsValid() )
			{
				tabs.AddChild( out Tab );
				Tab.SetContent( "PP Editor" );
			}

			if ( body.IsValid() )
			{
				body.AddChild( out Body );
			}
		}
	}

	protected override void OnUpdate()
	{
		if ( Tab.IsValid() && Tab.HasActive )
		{
			Tab.AddClass( "active" );
		}

		if ( Body.IsValid() )
		{
			Comp.SetValue( Comp, "activeTab", Body );
		}
	}
}
