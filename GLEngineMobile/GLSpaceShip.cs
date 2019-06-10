
namespace GLEngineMobile
{
	public class GLSpaceShip : GLObject
	{
		double _orbitAngle = 90;
		public GLEllipse OrbitEllipse { get; set; }
	
		public GLSpaceShip (string name="SapceShip")
		{
			Name = name;
			
			OrbitEllipse = new GLEllipse();
            OrbitEllipse.RadiusMajor = 15;
            OrbitEllipse.RadiusMinor = 10;                        
            OrbitEllipse.U = new GLVector(1,0,0);
            OrbitEllipse.V = new GLVector(0,-0.2,-1);
		}				
	
		public double OrbitAngle
		{
			set
			{
				_orbitAngle = value;
				
				Rotation.Y =  180 - value;
				Position = (OrbitEllipse as GLEllipse).GetPositionOfAngle(-value);
			}
			get
			{
				return _orbitAngle;
			}
		}
	}
}

