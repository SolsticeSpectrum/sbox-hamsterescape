using Sandbox;

public sealed class GoalComponent : Component, Component.ITriggerListener
{
	[Property] public bool FinalGoal { get; set; } = false;
	[Property] public bool IsActive { get; set; } = false;
	[Property] public GoalComponent NextGoal { get; set; }
	[Property] public bool StartActive { get; set; } = false;

	void Component.ITriggerListener.OnTriggerEnter( Collider other )
	{
		var hamster = other.GameObject.Components.Get<HamsterController>();
		if ( hamster is null ) return;
		hamster.ReachGoal();
	}
}
