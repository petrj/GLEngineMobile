using System;
using System.Text;
using System.IO;
using System.Drawing;
using System.Xml;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using TK = OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using GLEngineMobile;
using LoggerService;
using Android.Content;
using System.Threading.Tasks;

namespace Easy3DLabyrinth
{
	public class GLLabyrinthObj : GLObject
	{
        private bool _nearLockedDoors = false;
        private int _bonusItemsCount = 10;

        public int[,] LabMatrix { get; set; }        

        public Dictionary<Point,GLObject> BonusItems  { get; set; }
		
		public Point StartPos { get; set; }
		public Point EndPos { get; set; }

        public int BonusItemsCount
        {
            get
            {
                return _bonusItemsCount;
            }
        }
		
		public int LabyrinthWidth = 100;
		public int LabyrinthHeight = 100;
		
		public double FloorY = -3;		
		public double TileWidth = 25;		
		
		private Random Rnd = new Random();
		
		public int FinishCount { get; set; }
        public int Level { get; set; } = 1;

        public List<GLPolygon> LockedFinishPolygons { get; set; }
		public List<GLPolygon> UnLockedFinishPolygons { get; set; }

		public List<int> Path { get; set; }

		public AutoPilotStateEnum State { get; set; }
		
		public void Clear()
		{
			Polygons = new List<GLPolygon>();
			
			for (var i=0;i<LabyrinthWidth;i++)
			{
				for (var j=0;j<LabyrinthHeight;j++)
				{
					LabMatrix[i,j] = 0;
				}				
			}
			
			UnLockedFinishPolygons.Clear();
			LockedFinishPolygons.Clear();
			
			BonusItems.Clear();
			Path.Clear ();
		}
		
		public bool Locked
		{
			get
			{
				return BonusItems.Count>0;
			}
			set
			{
				foreach (var polygon in LockedFinishPolygons)
				{
					polygon.Visible = value;
				}
				foreach (var polygon in UnLockedFinishPolygons)
				{
					polygon.Visible = !value;
				}
			}
		}
		
		public GLPoint LabPointToScenePoint(Point P)
		{
			var glP = new GLPoint((TileWidth)*P.X, 10,(TileWidth)*(P.Y)+TileWidth/2);
			return glP;
		}

		 	// 0 .. right
		 	// 1 .. up
		 	// 2 .. left
		 	// 3 .. down
		private bool CanMove(int x, int y,int direction)
		{
				switch(direction)
			 		{
			 			case 0: x += 1; break; 				 			
			 			case 1: y += 1; break;
			 			case 2: x -= 1; break;
			 			case 3: y -= 1; break;
			 		}
			 		
			 	
			 	if (x<0) return false;			 	
			 	if (x>LabyrinthWidth-1) return false;
			 	if (y<0) return false;			 	
			 	if (y>LabyrinthHeight-1) return false;
			 	
			 	if ((x==StartPos.X) &&	(y==StartPos.Y)) return false;

			 	return true;
		}

		public void Generate(Context context, int moves, int itemsCount)
		{		
		 	Clear();

            State = AutoPilotStateEnum.Stoppped;
		 	
		 	StartPos = new Point(Rnd.Next(20,80),Rnd.Next(20,80));

            _bonusItemsCount = itemsCount;

            var actPos = new Point(StartPos.X,StartPos.Y);
		 	EndPos = new Point(actPos.X,actPos.Y);
		 	
		 	LabMatrix[actPos.X,actPos.Y] = 1;

		 	var direction = 1;
		 	for (var move=0;move<=moves;move++)
		 	{
		 		var steps = Rnd.Next( 1, 4);

                if (move == 0) steps = 4; // 4 steps on beginning

                for (var step=0;step<steps;step++)
		 		{		 		
		 			if (CanMove(actPos.X,actPos.Y,direction))
		 			{
			 			switch(direction)
				 		{
				 			case 0: actPos.X += 1; break; 				 			
				 			case 1: actPos.Y += 1; break;
				 			case 2: actPos.X -= 1; break;
				 			case 3: actPos.Y -= 1; break;
				 		}

						Path.Add (direction);
				 		
				 		EndPos = new Point(actPos.X,actPos.Y);
				 		LabMatrix[actPos.X,actPos.Y] = 1;
				 	}
			 	}
			 	
			 	direction = Rnd.Next( 0, 3);
		 	}
	 	
		 	var generatedPositions = new List<Point>();		 	
		 	for (var j=0;j<LabyrinthHeight;j++)
			{
				for (var i=0;i<LabyrinthWidth;i++)				
				{				
					if (LabMatrix[i,j] == 1)					
						generatedPositions.Add(new Point(i,j));					
				
					if ((i==StartPos.X) && (j==StartPos.Y))
					{		
							GeneratePosition(i,j,"labWallS","labBottomS", "labTopS");
					} else
					if ((i==EndPos.X) && (j==EndPos.Y)) 
					{
							// locked:
							LockedFinishPolygons.AddRange( GeneratePosition(i,j,"labWallL","labBottomL", "labTopL") );
							
							// unlocked:
							UnLockedFinishPolygons.AddRange( GeneratePosition(i,j,"labWallF","labBottomF", "labTopF") );
					} else
					{	
						GeneratePosition(i,j);
					}	
				}			

			}

            // bonus items

            if (generatedPositions.Count > BonusItemsCount + 4)
            {
                while (BonusItems.Count < BonusItemsCount)
                {
                    var r = Rnd.Next(2, generatedPositions.Count - 2);
                    if (BonusItems.Count == 0)
                    {
                        // first bonus item always 2 steps before observer
                        r = 2;
                    }

                    var p = new Point(generatedPositions[r].X, generatedPositions[r].Y);

                    if (BonusItems.ContainsKey(p) ||
                        ((StartPos.X == p.X && StartPos.Y == p.Y)) ||
                        ((EndPos.X == p.X && EndPos.Y == p.Y))
                        )
                    {
                        continue;
                    }

                    var item = new GLObject();
                    item.LoadFromAndroidAsset(context, "diamond.xml");
                    item.Position = new GLPoint((p.X + 0) * TileWidth, -2, (p.Y + 0) * TileWidth + TileWidth / 2);                    

                    BonusItems.Add(new Point(p.X, p.Y), item);
                }
            }
		
			// generating map
			var mapLines = new StringBuilder();
			for (var j=0;j<LabyrinthHeight;j++)
			{
				string line = null;
				for (var i=0;i<LabyrinthWidth;i++)				
				{					
					if (LabMatrix[i,j] == 1)
					{
						if ((i==StartPos.X) && (j==StartPos.Y)) 
						{
							line+='S';
						} else
						if ((i==EndPos.X) && (j==EndPos.Y))
						{
							line+='F';
						} else
						{
							var bonusAtThisPos =  false;
							foreach (var kvp in BonusItems)
							{
								if ((i==kvp.Key.X) && (j== kvp.Key.Y))
								{
									bonusAtThisPos = true;
									break;
								}
							}
							
							line+= bonusAtThisPos ? 'o' :'#';							
						}
					} else
					{
						line+=' ';
					}
				}		
				
				if (line != null && line.Trim()!=String.Empty)
				{	
					mapLines.Append(line);			
					mapLines.Append(Environment.NewLine);
				}
			}
			
			Logger.Info("Map:"+Environment.NewLine+mapLines.ToString());  
				 				 			 			
			Locked = true;
            FinishCount = 0;
					 			
			Move(-TileWidth/2,0,0);			
		}
		
		public List<GLPolygon> GeneratePosition(int labX,int labY, 
										string specialTexture = null, string specialBottomTexture = null, string specialTopTexture = null)
		{	
			if (LabMatrix[labX,labY] == 1)
					{
						var left = true;
						var right = true;
						var front = true;
						var back = true;
						
						if ((labX<LabyrinthWidth-1) && (LabMatrix[labX+1,labY]==1)) left = false;
						if ((labX>0) && (LabMatrix[labX-1,labY]==1)) right = false;
						
						if ((labY<LabyrinthHeight-1) && (LabMatrix[labX,labY+1]==1)) front = false;
						if ((labY>0) && (LabMatrix[labX,labY-1]==1)) back = false;
						
						return GeneratePosition(labX*TileWidth,FloorY,labY*TileWidth,left,right,front, back, specialTexture, specialBottomTexture, specialTopTexture);
					}
			
			return null;
		}
		
		public GLTexture GetRandomTexture(string texName)
		{
			var texNameIndex = 0;
			if (Rnd.Next(0,3)==1) texNameIndex = 1; else			
			if (Rnd.Next(0,3)==1) texNameIndex = 3; else
			if (Rnd.Next(0,7)==1) texNameIndex = 2; 
           
            texName = texName + texNameIndex.ToString();
			var tex = GLTextureAdmin.GetTextureByName(texName);
			
			return tex;
		}

		public List<GLPolygon> GeneratePosition(double x,double y,double z, 
										bool left, bool right, bool front, bool back,
										string specialTexture = null, string specialBottomTexture = null, string specialTopTexture = null)
		{
			var polygons = new List<GLPolygon>();
		
			// bottom 
		
			var bottomPolygon = new GLPolygon();
			bottomPolygon.Points = new List<GLPoint>() 
				{
                    new GLPoint(x+TileWidth,y,z+TileWidth),
                    new GLPoint(x+TileWidth,y,z),
                    new GLPoint(x,y,z),
                
                    new GLPoint(x,y,z+TileWidth),
                };
				
			bottomPolygon.Texture = specialBottomTexture == null ? GLTextureAdmin.GetTextureByName("labBottom") 
																: GLTextureAdmin.GetTextureByName(specialBottomTexture);
			polygons.Add(bottomPolygon);
					
		
			// top		
			var topPolygon = new GLPolygon();
			topPolygon.Points = new List<GLPoint>() 
				{                    
					new GLPoint(x,y+TileWidth,z+TileWidth),
                    new GLPoint(x+TileWidth,y+TileWidth,z+TileWidth),
                    new GLPoint(x+TileWidth,y+TileWidth,z),
                    new GLPoint(x,y+TileWidth,z),
                };
				
			topPolygon.Texture = specialTopTexture == null ? GLTextureAdmin.GetTextureByName("labTop") 
																: GLTextureAdmin.GetTextureByName(specialTopTexture);
			polygons.Add(topPolygon);
		
		
			if (left)
			{			
				var leftPolygon = new GLPolygon();
				leftPolygon.Points = new List<GLPoint>() 			
					{
						new GLPoint(x+TileWidth,y,z),
                        new GLPoint(x+TileWidth,y+TileWidth,z),
                        new GLPoint(x+TileWidth,y+TileWidth,z+TileWidth),
                        new GLPoint(x+TileWidth,y,z+TileWidth),
                    };				
				
				leftPolygon.Texture = specialTexture == null ? GetRandomTexture("labWall") :  GLTextureAdmin.GetTextureByName(specialTexture);
				polygons.Add(leftPolygon);
			}
					

			if (right)
			{			
				var rightPolygon = new GLPolygon();
				rightPolygon.Points = new List<GLPoint>() 			
					{ 	
                        new GLPoint(x,y,z),
                        new GLPoint(x,y+TileWidth,z),
                        new GLPoint(x,y+TileWidth,z+TileWidth),
                        new GLPoint(x,y,z+TileWidth),
                    };				
				
				rightPolygon.Texture = specialTexture == null ? GetRandomTexture("labWall") :  GLTextureAdmin.GetTextureByName(specialTexture);											
				polygons.Add(rightPolygon);
			}
			
			if (front)
			{			
				var frontPolygon = new GLPolygon();
				frontPolygon.Points = new List<GLPoint>() 			
					{ 							
						new GLPoint(x+TileWidth,y,z+TileWidth),
						new GLPoint(x+TileWidth,y+TileWidth,z+TileWidth),
						new GLPoint(x,y+TileWidth,z+TileWidth),
						new GLPoint(x,y,z+TileWidth)
					};				
					
				frontPolygon.Texture = specialTexture == null ? GetRandomTexture("labWall") :  GLTextureAdmin.GetTextureByName(specialTexture);											
				polygons.Add(frontPolygon);
			}	
			
			if (back)
			{			
				var backPolygon = new GLPolygon();
				backPolygon.Points = new List<GLPoint>() 			
					{ 							
						new GLPoint(x+TileWidth,y,z),
						new GLPoint(x+TileWidth,y+TileWidth,z),
						new GLPoint(x,y+TileWidth,z),
						new GLPoint(x,y,z)
					};				
					
					
				backPolygon.Texture = specialTexture == null ? GetRandomTexture("labWall") :  GLTextureAdmin.GetTextureByName(specialTexture);															
				polygons.Add(backPolygon);
			}				
			
			
			foreach (var polygon in polygons)
			{
				Polygons.Add(polygon);
			}
			
			return polygons;
		}

        public override void Render()
        {
            base.Render();

            foreach (var kvp in BonusItems)
            {
                // rotation around in 5 secs
                var ms = DateTime.Now.Millisecond + DateTime.Now.Second*1000;
                ms = ms % 5000;

                var angle = ms * (360.0 / (5.0 * 1000.0)); // around in 5 sec;                

                kvp.Value.Rotation.Y = angle;

                kvp.Value.Render();
            }
        }

        public GLObject GetNearestBonusItem(GLPoint p)
        {
            var minDist = double.MaxValue;
            GLObject res = null;
            foreach (var item in BonusItems)
            {
                var dist = p.DistanceToPoint(item.Value.Position);
                if (dist < minDist)
                {
                    minDist = dist;
                    res = item.Value;
                }
            }

            return res;
        }

        public void PickUpBonusItem(GLObject item)
        {
            var found = false;
            Point indexToRemove = new Point();
            foreach (var kvp in BonusItems)
            {
                if (kvp.Value == item)
                {
                    indexToRemove = kvp.Key;
                    found = true;
                    break;
                }
            }

            if (found)
            {
                BonusItems.Remove(indexToRemove);
                FinishCount++;

                string sound;
                if (FinishCount >= BonusItemsCount)
                {
                    sound = "lastdiamond.mp3";
                    Locked = false;
                } else
                {
                    sound = "diamond.mp3";
                }

                Task.Run(() =>
                {
                    var player = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.Current;
                    player.Load(sound);
                    player.Play();
                });
            }
        }

        public bool CheckPosition(GLPoint position)
        {
            var nearestBonusItem = GetNearestBonusItem(position);
            if (nearestBonusItem != null)
            {
                var dist = position.DistanceToPoint(nearestBonusItem.Position);
                if (dist < TileWidth)
                {
                    PickUpBonusItem(nearestBonusItem);
                }
            }

            var finishPosition = LabPointToScenePoint(EndPos);
            var distToFinish = position.DistanceToPoint(finishPosition);
            if (distToFinish < TileWidth)
            {
                if (!Locked)
                {
                    Level++;

                    Task.Run(() =>
                    {
                        var player = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.Current;
                        player.Load("levelcomplete.mp3");
                        player.Play();
                    });

                    return true;
                }
                else
                {
                    if (!_nearLockedDoors)
                    {
                        Task.Run(() =>
                        {
                            var player = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.Current;
                            player.Load("doorclosed.mp3");
                            player.Play();
                        });

                        _nearLockedDoors = true;
                    }
                }
            }
            else
            {
                _nearLockedDoors = false;
            }

            return false;
        }

        public GLLabyrinthObj()
		{
			BonusItems = new Dictionary<Point, GLObject>();
			
			UnLockedFinishPolygons = new List<GLPolygon>();
			LockedFinishPolygons = new List<GLPolygon>();
			Path = new List<int> ();
		
			LabMatrix = new int[LabyrinthWidth,LabyrinthHeight];			
			
			FinishCount = 0;
            Level = 1;
			
			Clear();
		}	
	
	}
}

