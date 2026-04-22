using Sandbox;
using Sandbox.UI;

#pragma warning disable CS0618 // ScenePanel.World/Camera obsolete, no scene based replacement yet

public class GimbalViewer : ScenePanel
{
	private ScenePointLight Light { get; set; }
	private SceneDirectionalLight Sun { get; set; }
	public static GimbalViewer Instance { get; set; }
	public SceneModel Gimbal { get; set; }

	public GimbalViewer()
	{
		Instance = this;
	}

	protected override void OnParametersSet()
	{
		World = new SceneWorld();

		Gimbal = new( World, "models/gimbal.vmdl", new( Vector3.Zero, Rotation.Identity ) );

		if ( Gimbal == null || Gimbal.Model.IsError ) return;

		Light = new ScenePointLight( World, Gimbal.Transform.Position + Vector3.Up * 100f, 300f, Color.White );
		Sun = new SceneDirectionalLight( World, Rotation.From( 0, 50f, 0 ), Color.White );

		Camera.Position = Gimbal.Bounds.Center + Camera.Angles.Forward * -Gimbal.Bounds.Size.Length;

		Camera.FieldOfView = 70f;
	}
}
