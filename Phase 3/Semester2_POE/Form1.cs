using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;


namespace Semester2_POE
{
    public partial class Form1 : Form, INotifyPropertyChanged
    {
        public static Map _map;
        public static GameEngine _gameEngine { get; set; }
        // Timer
        public Timer _timer = new Timer();
        private int _tickCounter;
        public int TickCounter { get { return _tickCounter; } set { _tickCounter = value; OnPropertyChanged("TickCounter"); } }
        private string _winnerText;
        public string WinnerText { get { return _winnerText; } set { _winnerText = value; OnPropertyChanged("WinnerText"); } }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string property)
        {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        public Form1()
        {
            _timer.Tick += new EventHandler(TimerElapsed);
            _timer.Interval = 1000;
            _timer.Enabled = true;
            _timer.Stop();
            _tickCounter = 0;
            InitializeComponent();
            lblTick.DataBindings.Add(new Binding("Text", this, "TickCounter"));
            lblWinner.DataBindings.Add(new Binding("Text", this, "WinnerText"));
            _gameEngine = new GameEngine();            
        }
        /// <summary>
        /// Update UI with new map layout
        /// </summary>
        public void UpdateMap()
        {
            //tableLayoutPanel1.Controls.Clear();
            for (int i = 0; i < _map.Size; i++)
            {
                for (int j = 0; j < _map.Size; j++)
                {
                    Label label1 = (Label)this.tableLayoutPanel1.GetControlFromPosition(i, j);                 
                    if (_map.MapArray.GetValue(i, j).ToString() != ".")
                    {
                        label1.Text = _map.MapArray.GetValue(i, j).ToString()[0].ToString();
                        label1.Font = new Font(label1.Font.Name, 10, FontStyle.Bold);
                        label1.ForeColor = _map.MapArray.GetValue(i, j).ToString()[1] == '1' ? Color.Red : Color.Blue;                      
                    }
                    else
                    {
                        label1.Text = ".";
                        label1.ForeColor = Color.Black;                      
                    }
                }
            }          
        }
        // Label Click event to display the Attributes of a unit to the textbox
        private void label_Click(object sender, EventArgs e)
        {
            
        }
        /// <summary>
        /// This method will be called everytime the timer elapses
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TimerElapsed(object sender, EventArgs e)
        {
            //_timer.Enabled = false;
            TickCounter++;
            if (!_gameEngine.UpdateMap(ref _map, out string winner))
            {
                WinnerText = "Game over! " + winner;
                _timer.Stop();
            }
            _map.UpdateMapArray();
            // Update UI
            UpdateMap();
        }    
        /// <summary>
        /// Executed when the start button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            int mapSize;
            if (numMapSize.Text.Length > 0)
            {
                mapSize = Int32.Parse(numMapSize.Text);
            }
            else
                mapSize = 20;
            _map = new Map(mapSize);
            _map.GenerateMap();
            // Initilise labels to be displayed on table layout panel
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    if (_map.MapArray.GetValue(i, j).ToString() != ".")
                    {
                        Label label = new Label()
                        {
                            Text = _map.MapArray.GetValue(i, j).ToString()[0].ToString(),
                            Font = new Font(label1.Font.Name, 10, FontStyle.Bold),
                            ForeColor = _map.MapArray.GetValue(i, j).ToString()[1] == '1' ? Color.Red : Color.Blue,
                        };

                        //label.Click += NameOfMethod;

                        tableLayoutPanel1.Controls.Add(label, i, j);
                    }
                    else
                    {
                        Label label = new Label
                        {
                            Text = _map.MapArray.GetValue(i, j).ToString()
                        };
                        tableLayoutPanel1.Controls.Add(label, i, j);
                    }
                }
            }
            _timer.Start();
        }
        /// <summary>
        /// Executed when the pause button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPause_Click(object sender, EventArgs e)
        {
            _timer.Stop();
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            
        }
        private void btnRead_Click(object sender, EventArgs e)
        {
            
        }
        
    }
    /// <summary>
    /// Class to contain coordinates of a unit
    /// </summary>
    public class Coordinates
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
    /// <summary>
    /// Public building class to store the properties of Resource, Factory and Barracks buildings
    /// </summary>
    public class Building
    {
        public int X_Position { get; set; }
        public int Y_Position { get; set; }
        public int Health { get; set; }
        public int Team { get; set; }
        public string Symbol { get; set; }
        public Coordinates CoordinatesOfClosestEnemy { get; set; }
        private Random _random = new Random(Guid.NewGuid().GetHashCode());
        public string Name { get; set; }
        public virtual void Save() { }
        public virtual void Read() { }
        public Building(string name)
        {
            X_Position = 0;
            Y_Position = 0;
            Health = 500;
            Team = _random.Next(1, 3);
            Name = name;
        }
        public virtual void HandleDeathOfBuilding()
        {

        }
        public override string ToString()
        {
            return "X:" + X_Position + Environment.NewLine +
                    "Y:" + Y_Position + Environment.NewLine +
                    "Health:" + Health + Environment.NewLine +
                    "Team:" + Team.ToString() + Environment.NewLine +
                    "Symbol:" + Symbol + Environment.NewLine +
                    "Name:" + Name + Environment.NewLine;
        }
    }
    /// <summary>
    /// Resource building calling from the building class
    /// </summary>
    public class ResourceBuilding : Building
    {
        public int ResourceType { get; set; }
        public int ResourcePerGameTick { get; set; }
        public int ResourceRemaining { get; set; }
        public ResourceBuilding(string name) : base(name)
        {
            base.Symbol = "#" + Team.ToString();
        }
        public override void Save()
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Public\TestFolder\" + this.Name + ".txt", true))
            {
                file.WriteLine(this.ToString());
            }
        }
        public override void Read()
        {
            {
                StreamReader reader = File.OpenText(@"C: \Users\Public\TestFolder\" + this.Name + ".txt");
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] items = line.Split(':');
                    int myInteger = int.Parse(items[1]);
                    string path = null;
                    foreach (string item in items)
                    {
                        if (item.StartsWith("item\\") && item.EndsWith(".ddj"))
                            path = item;
                    }
                }
            }
        }
        public override string ToString()
        {
            return base.ToString() +
                "ResourceType:" + ResourceType + Environment.NewLine +
                "ResourcePerGameTick:" + ResourcePerGameTick + Environment.NewLine +
                "ResourceRemaining:" + ResourceRemaining + Environment.NewLine;
        }
        public void GenerateResources() { }
    }
    /// <summary>
    /// Factory Building calling from the building class
    /// </summary>
    public class FactoryBuilding : Building
    {
        public int UnitProduction { get; set; }
        public int GameTickPerProduction { get; set; }
        public int SpawnPoint { get; set; }
        public void FactoryBuildingSpawnUnits()
        {

        }
        public FactoryBuilding(string name) : base(name)
        {
            base.Symbol = "*" + Team.ToString();
        }
        public override string ToString()
        {
            return base.ToString() +
                "UnitProduction:" + UnitProduction + Environment.NewLine +
                "GameTickPerProduction:" + GameTickPerProduction + Environment.NewLine +
                "SpawnPoint:" + SpawnPoint + Environment.NewLine;
        }
        public override void Save()
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Public\TestFolder\" + this.Name + ".txt", true))
            {
                file.WriteLine(this.ToString());
            }
        }
        public override void Read()
        {
            {
                StreamReader reader = File.OpenText(@"C: \Users\Public\TestFolder\" + this.Name + ".txt");
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] items = line.Split(':');
                    int myInteger = int.Parse(items[1]);
                    string path = null;
                    foreach (string item in items)
                    {
                        if (item.StartsWith("item\\") && item.EndsWith(".ddj"))
                            path = item;
                    }
                }
            }
        }
    }
    /// <summary>
    /// Barracks Building calling from the building class
    /// </summary>
    public class BarracksBuilding : Building
    {
        public int WeaponProduction { get; set; }
        public int GameTickPerProduction { get; set; }
        public int SpawnPoint { get; set; }
        public void BarracksBuildingRepairUnitsWeapons() { }
        public BarracksBuilding(string name) : base(name)
        {
            base.Symbol = "B" + Team.ToString();
        }
        public override string ToString()
        {
            return base.ToString() +
                "WeaponProduction:" + WeaponProduction + Environment.NewLine +
                "GameTickPerProduction:" + GameTickPerProduction + Environment.NewLine +
                "SpawnPoint:" + SpawnPoint + Environment.NewLine;
        }
        public override void Save()
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Public\TestFolder\" + this.Name + ".txt", true))
            {
                file.WriteLine(this.ToString());
            }
        }
        public override void Read()
        {
            {
                StreamReader reader = File.OpenText(@"C: \Users\Public\TestFolder\" + this.Name + ".txt");
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] items = line.Split(':');
                    int myInteger = int.Parse(items[1]);
                    string path = null;
                    foreach (string item in items)
                    {
                        if (item.StartsWith("item\\") && item.EndsWith(".ddj"))
                            path = item;
                    }
                }
            }
        }
    }
    /// <summary>
    /// Unit base class
    /// </summary>
    public abstract class Unit
    {
        public int X_Position { get; set; }
        public int Y_Position { get; set; }
        public bool IsInCombat { get; set; }
        public bool IsDead { get; set; }
        public Coordinates CoordinatesOfClosestEnemy { get; set; }
        public int Health { get; set; }
        protected int Speed;
        protected int Attack;
        protected int AttackRange;
        public int Team { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        private Random Random = new Random(Guid.NewGuid().GetHashCode());
        public virtual void UnitClick(object sender, EventArgs e)
        { }
        public virtual void Save()
        {}
        public virtual void Read()
        {}
        /// <summary>
        /// /constructor
        /// </summary>
        public Unit()
        {
            X_Position = 0;
            Y_Position = 0;
            Health = 0;
            Speed = 0;
            Attack = 0;
            AttackRange = 0;
            Team = Random.Next(1,3);
            Symbol = String.Empty;
            Name = String.Empty;            
        }
        public virtual void MoveToNewPosition()
        {

        }
        public virtual void HandleCombat(ref Unit[] units)
        {

        }
        public virtual bool IsUnitInAttackRange(Unit[] units)
        {
            return false;
        }
        public virtual Coordinates ReturnPositionOfClosestEnemy(Unit[] units)
        {
            return null;
        }
        public virtual void HandleDeathOfUnit()
        {

        }
        public override string ToString()
        {
            return  "X:" + X_Position + Environment.NewLine +
                    "Y:" + Y_Position + Environment.NewLine +
                    "_Speed:" + Speed + Environment.NewLine +
                    "Health:" + Health + Environment.NewLine +
                    "Attack:" + Attack + Environment.NewLine +
                    "Attack_Range" + AttackRange + Environment.NewLine +
                    "Team:" + Team.ToString() + Environment.NewLine +
                    "Symbol:" + Symbol + Environment.NewLine +
                    "Name:" + Name + Environment.NewLine;
        }
    }
    /// <summary>
    /// This is a melee unit 
    /// </summary>
    public class MeleeUnit : Unit
    {
        int maxSize;
        public MeleeUnit(int size = 20)
        {
            Attack = 10;
            AttackRange = 1;
            Health = 100;
            Speed = 5;
            Symbol = "M" + Team.ToString();
            maxSize = size - 1;
        }
        public override void HandleCombat(ref Unit[] units)
        {
            foreach (var unit in units)
            {
                if (unit.X_Position == this.CoordinatesOfClosestEnemy.X && unit.Y_Position == this.CoordinatesOfClosestEnemy.Y)
                {
                    unit.Health -= this.Attack;
                    if (unit.Health < 0)
                    {
                        unit.HandleDeathOfUnit();
                    }
                }
            }
        }
        public override void HandleDeathOfUnit()
        {
            this.Health = 0;
        }
        public override bool IsUnitInAttackRange(Unit[] units)
        {
            if (CoordinatesOfClosestEnemy != null)
            {
                var length = Math.Abs(CoordinatesOfClosestEnemy.X - this.X_Position) + Math.Abs(CoordinatesOfClosestEnemy.Y - this.Y_Position);
                if (length <= this.AttackRange)
                {
                    IsInCombat = true;
                    return true;
                }
            }

            IsInCombat = false;
            return false;
        }
        public override void MoveToNewPosition()
        {
            if (CoordinatesOfClosestEnemy != null)
            {
                if (this.Health > 25)
                {
                    if (this.X_Position > CoordinatesOfClosestEnemy.X)
                    {
                        this.X_Position--;
                    }
                    else if (this.X_Position < CoordinatesOfClosestEnemy.X)
                    {
                        this.X_Position++;
                    }
                    if (this.Y_Position > CoordinatesOfClosestEnemy.Y)
                    {
                        this.Y_Position--;
                    }
                    else if (this.Y_Position < CoordinatesOfClosestEnemy.Y)
                    {
                        this.Y_Position++;
                    }
                }
                else
                {
                    if (this.X_Position > CoordinatesOfClosestEnemy.X)
                    {
                        this.X_Position++;
                    }
                    else if (this.X_Position < CoordinatesOfClosestEnemy.X)
                    {
                        this.X_Position--;
                    }
                    else if (this.X_Position == CoordinatesOfClosestEnemy.X)
                    {
                        if (this.X_Position + 1 > maxSize)
                        {
                            this.X_Position--;
                        }
                        else if (this.X_Position - 1 < 0)
                        {
                            this.X_Position++;
                        }
                    }
                    if (this.Y_Position > CoordinatesOfClosestEnemy.Y)
                    {
                        this.Y_Position++;
                    }
                    else if (this.Y_Position < CoordinatesOfClosestEnemy.Y)
                    {
                        this.Y_Position--;
                    }
                    else if (this.Y_Position == CoordinatesOfClosestEnemy.Y)
                    {
                        if (this.Y_Position + 1 > maxSize)
                        {
                            this.Y_Position--;
                        }
                        else if (this.Y_Position - 1 < 0)
                        {
                            this.Y_Position++;
                        }
                    }
                }
                    if (this.X_Position < 0) this.X_Position = 0;
                    if (this.X_Position > maxSize) this.X_Position = maxSize;
                    if (this.Y_Position < 0) this.Y_Position = 0;
                    if (this.Y_Position > maxSize) this.Y_Position = maxSize;
            }
        }
        public override Coordinates ReturnPositionOfClosestEnemy(Unit[] units)
        {
            int shortestLength = 0;
            int index = 0;
            int i = 0;
            foreach (var unit in units)
            {
                if (unit.Team != this.Team && unit.Team != 0)
                {
                    var length = Math.Abs(unit.X_Position - this.X_Position) + Math.Abs(unit.Y_Position - this.Y_Position);
                    if (length < shortestLength)
                    {
                        shortestLength = length;
                        index = i;
                    }
                }
                i++;
            }
            CoordinatesOfClosestEnemy = new Coordinates
            {
                X = units[index].X_Position,
                Y = units[index].Y_Position
            };            
            return CoordinatesOfClosestEnemy;
        }  
        public override string ToString()
        {
            return "Attack:" + Attack + Environment.NewLine +
                    "Attack_Range" + AttackRange + Environment.NewLine+ 
                    "Health:" + Health + Environment.NewLine +
                    "Speed:" + Speed + Environment.NewLine +
                    "Symbol:" + Symbol.ToString() + Environment.NewLine;           
        }
        public override void Save()
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Public\TestFolder\" + this.Name + ".txt", true))
            {
                file.WriteLine(this.ToString());
            }
        }
        public override void Read()
        {            
            {
                StreamReader reader = File.OpenText(@"C: \Users\Public\TestFolder\" + this.Name + ".txt");
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] items = line.Split(':');
                    int myInteger = int.Parse(items[1]);   
                    string path = null;
                    foreach (string item in items)
                    {
                        if (item.StartsWith("item\\") && item.EndsWith(".ddj"))
                            path = item;
                    }
                }
            }
        }
    }
    /// <summary>
    /// This is a ranged unit 
    /// </summary>
    public class RangedUnit : Unit
    {
        int maxSize;
        public RangedUnit(int size = 20)
        {
            Attack = 5;
            AttackRange = 5;
            Health = 100;
            Speed = 5;
            Symbol = "R" + Team.ToString();
            maxSize = size - 1;
        }
        public override void HandleCombat(ref Unit[] units)
        {
            foreach (var unit in units)
            {
                if (unit.X_Position == this.CoordinatesOfClosestEnemy.X && unit.Y_Position == this.CoordinatesOfClosestEnemy.Y)
                {
                    unit.Health -= this.Attack;
                    if (unit.Health < 0)
                    {
                        unit.HandleDeathOfUnit();
                    }
                }
            }
        }
        public override void HandleDeathOfUnit()
        {
            this.Health = 0;
        }
        public override bool IsUnitInAttackRange(Unit[] unit)
        {
            if (CoordinatesOfClosestEnemy != null)
            {
                var length = Math.Abs(CoordinatesOfClosestEnemy.X - this.X_Position) + Math.Abs(CoordinatesOfClosestEnemy.Y - this.Y_Position);
                if (length <= this.AttackRange)
                {
                    IsInCombat = true;
                    return true;
                }
            }
            IsInCombat = false;
            return false;
        }
        public override void MoveToNewPosition()
        {
            if (CoordinatesOfClosestEnemy != null)
            {
                if (this.Health > 25)
                {
                    if (this.X_Position > CoordinatesOfClosestEnemy.X)
                    {
                        this.X_Position--;
                    }
                    else if (this.X_Position < CoordinatesOfClosestEnemy.X)
                    {
                        this.X_Position++;
                    }
                    if (this.Y_Position > CoordinatesOfClosestEnemy.Y)
                    {
                        this.Y_Position--;
                    }
                    else if (this.Y_Position < CoordinatesOfClosestEnemy.Y)
                    {
                        this.Y_Position++;
                    }
                }
                else
                {
                    if (this.X_Position > CoordinatesOfClosestEnemy.X)
                    {
                        this.X_Position++;
                    }
                    else if (this.X_Position < CoordinatesOfClosestEnemy.X)
                    {
                        this.X_Position--;
                    }
                    else if (this.X_Position == CoordinatesOfClosestEnemy.X)
                    {
                        if (this.X_Position + 1 > maxSize)
                        {
                            this.X_Position--;
                        }
                        else if (this.X_Position - 1 < 0)
                        {
                            this.X_Position++;
                        }
                    }
                    if (this.Y_Position > CoordinatesOfClosestEnemy.Y)
                    {
                        this.Y_Position++;
                    }
                    else if (this.Y_Position < CoordinatesOfClosestEnemy.Y)
                    {
                        this.Y_Position--;
                    }
                    else if (this.Y_Position == CoordinatesOfClosestEnemy.Y)
                    {
                        if (this.Y_Position + 1 > maxSize)
                        {
                            this.Y_Position--;
                        }
                        else if (this.Y_Position - 1 < 0)
                        {
                            this.Y_Position++;
                        }
                    }
                }
                if (this.X_Position < 0) this.X_Position = 0;
                if (this.X_Position > maxSize) this.X_Position = maxSize;
                if (this.Y_Position < 0) this.Y_Position = 0;
                if (this.Y_Position > maxSize) this.Y_Position = maxSize;
            }
        }
        public override Coordinates ReturnPositionOfClosestEnemy(Unit[] units)
        {
            int shortestLength = 41;
            int index = 0;
            int i = 0;
            foreach (var unit in units)
            {
                if (unit.Team != this.Team && unit.Team != 0)
                {
                    var length = Math.Abs(unit.X_Position - this.X_Position) + Math.Abs(unit.Y_Position - this.Y_Position);
                    if (length < shortestLength)
                    {
                        shortestLength = length;
                        index = i;
                    }
                }
                i++;
            }
            CoordinatesOfClosestEnemy = new Coordinates
            {
                X = units[index].X_Position,
                Y = units[index].Y_Position
            };
            return CoordinatesOfClosestEnemy;
        }
        public override string ToString()
        {
            return "Attack:" + Attack + Environment.NewLine +
                    "Attack_Range" + AttackRange + Environment.NewLine +
                    "Health:" + Health + Environment.NewLine +
                    "Speed:" + Speed + Environment.NewLine +
                    "Symbol:" + Symbol.ToString() + Environment.NewLine;
        }
        public override void Save()
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Public\TestFolder\" + this.Name + ".txt", true))
            {
                file.WriteLine(this.ToString());
            }
        }
        public override void Read()
        {
            StreamReader reader = File.OpenText(@"C: \Users\Public\TestFolder\" + this.Name + ".txt");
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] items = line.Split(':');
                int myInteger = int.Parse(items[1]);  
                string path = null;
                foreach (string item in items)
                {
                    if (item.StartsWith("item\\") && item.EndsWith(".ddj"))
                        path = item;
                }
            }
        }
    }
    /// <summary>
    /// This is a siege unit
    /// </summary>
    public class SiegeUnit : Unit
    {
        int maxSize;
        public SiegeUnit(int size = 20)
        {
            Attack = 10;
            AttackRange = 8;
            Health = 125;
            Speed = 2;
            Symbol = "S" + Team.ToString();
            maxSize = size - 1;
        }
        public override void HandleCombat(ref Unit[] units)
        {
            foreach (var unit in units)
            {
                if (unit.X_Position == this.CoordinatesOfClosestEnemy.X && unit.Y_Position == this.CoordinatesOfClosestEnemy.Y)
                {
                    unit.Health -= this.Attack;
                    if (unit.Health < 0)
                    {
                        unit.HandleDeathOfUnit();
                    }
                }
            }
        }
        public override void HandleDeathOfUnit()
        {
            this.Health = 0;
        }
        public override bool IsUnitInAttackRange(Unit[] unit)
        {
            if (CoordinatesOfClosestEnemy != null)
            {
                var length = Math.Abs(CoordinatesOfClosestEnemy.X - this.X_Position) + Math.Abs(CoordinatesOfClosestEnemy.Y - this.Y_Position);
                if (length <= this.AttackRange)
                {
                    IsInCombat = true;
                    return true;
                }
            }
            IsInCombat = false;
            return false;
        }
        public override void MoveToNewPosition()
        {
            if (CoordinatesOfClosestEnemy != null)
            {
                if (this.Health > 25)
                {
                    if (this.X_Position > CoordinatesOfClosestEnemy.X)
                    {
                        this.X_Position--;
                    }
                    else if (this.X_Position < CoordinatesOfClosestEnemy.X)
                    {
                        this.X_Position++;
                    }
                    if (this.Y_Position > CoordinatesOfClosestEnemy.Y)
                    {
                        this.Y_Position--;
                    }
                    else if (this.Y_Position < CoordinatesOfClosestEnemy.Y)
                    {
                        this.Y_Position++;
                    }
                }
                else
                {
                    if (this.X_Position > CoordinatesOfClosestEnemy.X)
                    {
                        this.X_Position++;
                    }
                    else if (this.X_Position < CoordinatesOfClosestEnemy.X)
                    {
                        this.X_Position--;
                    }
                    else if (this.X_Position == CoordinatesOfClosestEnemy.X)
                    {
                        if (this.X_Position + 1 > maxSize)
                        {
                            this.X_Position--;
                        }
                        else if (this.X_Position - 1 < 0)
                        {
                            this.X_Position++;
                        }
                    }
                    if (this.Y_Position > CoordinatesOfClosestEnemy.Y)
                    {
                        this.Y_Position++;
                    }
                    else if (this.Y_Position < CoordinatesOfClosestEnemy.Y)
                    {
                        this.Y_Position--;
                    }
                    else if (this.Y_Position == CoordinatesOfClosestEnemy.Y)
                    {
                        if (this.Y_Position + 1 > maxSize)
                        {
                            this.Y_Position--;
                        }
                        else if (this.Y_Position - 1 < 0)
                        {
                            this.Y_Position++;
                        }
                    }
                }
                if (this.X_Position < 0) this.X_Position = 0;
                if (this.X_Position > maxSize) this.X_Position = maxSize;
                if (this.Y_Position < 0) this.Y_Position = 0;
                if (this.Y_Position > maxSize) this.Y_Position = maxSize;
            }
        }
        public override Coordinates ReturnPositionOfClosestEnemy(Unit[] units)
        {
            int shortestLength = 41;
            int index = 0;
            int i = 0;
            foreach (var unit in units)
            {
                if (unit.Team != this.Team && unit.Team != 0)
                {
                    var length = Math.Abs(unit.X_Position - this.X_Position) + Math.Abs(unit.Y_Position - this.Y_Position);
                    if (length < shortestLength)
                    {
                        shortestLength = length;
                        index = i;
                    }
                }
                i++;
            }
            CoordinatesOfClosestEnemy = new Coordinates
            {
                X = units[index].X_Position,
                Y = units[index].Y_Position
            };
            return CoordinatesOfClosestEnemy;
        }
        public override string ToString()
        {
            return "Attack:" + Attack + Environment.NewLine +
                    "Attack_Range" + AttackRange + Environment.NewLine +
                    "Health:" + Health + Environment.NewLine +
                    "Speed:" + Speed + Environment.NewLine +
                    "Symbol:" + Symbol.ToString() + Environment.NewLine;
        }
        public override void Save()
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Public\TestFolder\" + this.Name + ".txt", true))
            {
                file.WriteLine(this.ToString());
            }
        }
        public override void Read()
        {
            StreamReader reader = File.OpenText(@"C: \Users\Public\TestFolder\" + this.Name + ".txt");
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] items = line.Split(':');
                int myInteger = int.Parse(items[1]);
                string path = null;
                foreach (string item in items)
                {
                    if (item.StartsWith("item\\") && item.EndsWith(".ddj"))
                        path = item;
                }
            }
        }
    }
    /// <summary>
    /// This is a peasant unit
    /// </summary>
    public class PeasantFighter : Unit
    {
        int maxSize;
        public PeasantFighter(int size = 20)
        {
            Attack = 2;
            AttackRange = 2;
            Health = 50;
            Speed = 2;
            Symbol = "P" + Team.ToString();
            maxSize = size - 1;
        }
        public override void HandleCombat(ref Unit[] units)
        {
            foreach (var unit in units)
            {
                if (unit.X_Position == this.CoordinatesOfClosestEnemy.X && unit.Y_Position == this.CoordinatesOfClosestEnemy.Y)
                {
                    unit.Health -= this.Attack;
                    if (unit.Health < 0)
                    {
                        unit.HandleDeathOfUnit();
                    }
                }
            }
        }
        public override void HandleDeathOfUnit()
        {
            this.Health = 0;
        }
        public override bool IsUnitInAttackRange(Unit[] unit)
        {
            if (CoordinatesOfClosestEnemy != null)
            {
                var length = Math.Abs(CoordinatesOfClosestEnemy.X - this.X_Position) + Math.Abs(CoordinatesOfClosestEnemy.Y - this.Y_Position);
                if (length <= this.AttackRange)
                {
                    IsInCombat = true;
                    return true;
                }
            }
            IsInCombat = false;
            return false;
        }
        public override void MoveToNewPosition()
        {
            if (CoordinatesOfClosestEnemy != null)
            {
                if (this.Health > 25)
                {
                    if (this.X_Position > CoordinatesOfClosestEnemy.X)
                    {
                        this.X_Position--;
                    }
                    else if (this.X_Position < CoordinatesOfClosestEnemy.X)
                    {
                        this.X_Position++;
                    }
                    if (this.Y_Position > CoordinatesOfClosestEnemy.Y)
                    {
                        this.Y_Position--;
                    }
                    else if (this.Y_Position < CoordinatesOfClosestEnemy.Y)
                    {
                        this.Y_Position++;
                    }
                }
                else
                {
                    if (this.X_Position > CoordinatesOfClosestEnemy.X)
                    {
                        this.X_Position++;
                    }
                    else if (this.X_Position < CoordinatesOfClosestEnemy.X)
                    {
                        this.X_Position--;
                    }
                    else if (this.X_Position == CoordinatesOfClosestEnemy.X)
                    {
                        if (this.X_Position + 1 > maxSize)
                        {
                            this.X_Position--;
                        }
                        else if (this.X_Position - 1 < 0)
                        {
                            this.X_Position++;
                        }
                    }
                    if (this.Y_Position > CoordinatesOfClosestEnemy.Y)
                    {
                        this.Y_Position++;
                    }
                    else if (this.Y_Position < CoordinatesOfClosestEnemy.Y)
                    {
                        this.Y_Position--;
                    }
                    else if (this.Y_Position == CoordinatesOfClosestEnemy.Y)
                    {
                        if (this.Y_Position + 1 > maxSize)
                        {
                            this.Y_Position--;
                        }
                        else if (this.Y_Position - 1 < 0)
                        {
                            this.Y_Position++;
                        }
                    }
                }
                if (this.X_Position < 0) this.X_Position = 0;
                if (this.X_Position > maxSize) this.X_Position = maxSize;
                if (this.Y_Position < 0) this.Y_Position = 0;
                if (this.Y_Position > maxSize) this.Y_Position = maxSize;
            }
        }
        public override Coordinates ReturnPositionOfClosestEnemy(Unit[] units)
        {
            int shortestLength = 41;
            int index = 0;
            int i = 0;
            foreach (var unit in units)
            {
                if (unit.Team != this.Team && unit.Team != 0)
                {
                    var length = Math.Abs(unit.X_Position - this.X_Position) + Math.Abs(unit.Y_Position - this.Y_Position);
                    if (length < shortestLength)
                    {
                        shortestLength = length;
                        index = i;
                    }
                }
                i++;
            }
            CoordinatesOfClosestEnemy = new Coordinates
            {
                X = units[index].X_Position,
                Y = units[index].Y_Position
            };
            return CoordinatesOfClosestEnemy;
        }
        public override string ToString()
        {
            return "Attack:" + Attack + Environment.NewLine +
                    "Attack_Range" + AttackRange + Environment.NewLine +
                    "Health:" + Health + Environment.NewLine +
                    "Speed:" + Speed + Environment.NewLine +
                    "Symbol:" + Symbol.ToString() + Environment.NewLine;
        }
        public override void Save()
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Public\TestFolder\" + this.Name + ".txt", true))
            {
                file.WriteLine(this.ToString());
            }
        }
        public override void Read()
        {
            StreamReader reader = File.OpenText(@"C: \Users\Public\TestFolder\" + this.Name + ".txt");
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] items = line.Split(':');
                int myInteger = int.Parse(items[1]);
                string path = null;
                foreach (string item in items)
                {
                    if (item.StartsWith("item\\") && item.EndsWith(".ddj"))
                        path = item;
                }
            }
        }
    }
    /// <summary>
    /// This is a mercenary
    /// </summary>
    public class Mercenary : Unit
    {
        int maxSize;
        public Mercenary(int size = 20)
        {
            Attack = 12;
            AttackRange = 1;
            Health = 125;
            Speed = 6;
            Symbol = "O" + Team.ToString();
            maxSize = size - 1;
        }
        public override void HandleCombat(ref Unit[] units)
        {
            foreach (var unit in units)
            {
                if (unit.X_Position == this.CoordinatesOfClosestEnemy.X && unit.Y_Position == this.CoordinatesOfClosestEnemy.Y)
                {
                    unit.Health -= this.Attack;
                    if (unit.Health < 0)
                    {
                        unit.HandleDeathOfUnit();
                    }
                }
            }
        }
        public override void HandleDeathOfUnit()
        {
            this.Health = 0;
        }
        public override bool IsUnitInAttackRange(Unit[] unit)
        {
            if (CoordinatesOfClosestEnemy != null)
            {
                var length = Math.Abs(CoordinatesOfClosestEnemy.X - this.X_Position) + Math.Abs(CoordinatesOfClosestEnemy.Y - this.Y_Position);
                if (length <= this.AttackRange)
                {
                    IsInCombat = true;
                    return true;
                }
            }
            IsInCombat = false;
            return false;
        }
        public override void MoveToNewPosition()
        {
            if (CoordinatesOfClosestEnemy != null)
            {
                if (this.Health > 25)
                {
                    if (this.X_Position > CoordinatesOfClosestEnemy.X)
                    {
                        this.X_Position--;
                    }
                    else if (this.X_Position < CoordinatesOfClosestEnemy.X)
                    {
                        this.X_Position++;
                    }
                    if (this.Y_Position > CoordinatesOfClosestEnemy.Y)
                    {
                        this.Y_Position--;
                    }
                    else if (this.Y_Position < CoordinatesOfClosestEnemy.Y)
                    {
                        this.Y_Position++;
                    }
                }
                else
                {
                    if (this.X_Position > CoordinatesOfClosestEnemy.X)
                    {
                        this.X_Position++;
                    }
                    else if (this.X_Position < CoordinatesOfClosestEnemy.X)
                    {
                        this.X_Position--;
                    }
                    else if (this.X_Position == CoordinatesOfClosestEnemy.X)
                    {
                        if (this.X_Position + 1 > maxSize)
                        {
                            this.X_Position--;
                        }
                        else if (this.X_Position - 1 < 0)
                        {
                            this.X_Position++;
                        }
                    }
                    if (this.Y_Position > CoordinatesOfClosestEnemy.Y)
                    {
                        this.Y_Position++;
                    }
                    else if (this.Y_Position < CoordinatesOfClosestEnemy.Y)
                    {
                        this.Y_Position--;
                    }
                    else if (this.Y_Position == CoordinatesOfClosestEnemy.Y)
                    {
                        if (this.Y_Position + 1 > maxSize)
                        {
                            this.Y_Position--;
                        }
                        else if (this.Y_Position - 1 < 0)
                        {
                            this.Y_Position++;
                        }
                    }
                }
                if (this.X_Position < 0) this.X_Position = 0;
                if (this.X_Position > maxSize) this.X_Position = maxSize;
                if (this.Y_Position < 0) this.Y_Position = 0;
                if (this.Y_Position > maxSize) this.Y_Position = maxSize;
            }
        }
        public override Coordinates ReturnPositionOfClosestEnemy(Unit[] units)
        {
            int shortestLength = 41;
            int index = 0;
            int i = 0;
            foreach (var unit in units)
            {
                if (unit.Team != this.Team && unit.Team != 0)
                {
                    var length = Math.Abs(unit.X_Position - this.X_Position) + Math.Abs(unit.Y_Position - this.Y_Position);
                    if (length < shortestLength)
                    {
                        shortestLength = length;
                        index = i;
                    }
                }
                i++;
            }
            CoordinatesOfClosestEnemy = new Coordinates
            {
                X = units[index].X_Position,
                Y = units[index].Y_Position
            };
            return CoordinatesOfClosestEnemy;
        }
        public override string ToString()
        {
            return "Attack:" + Attack + Environment.NewLine +
                    "Attack_Range" + AttackRange + Environment.NewLine +
                    "Health:" + Health + Environment.NewLine +
                    "Speed:" + Speed + Environment.NewLine +
                    "Symbol:" + Symbol.ToString() + Environment.NewLine;
        }
        public override void Save()
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Public\TestFolder\" + this.Name + ".txt", true))
            {
                file.WriteLine(this.ToString());
            }
        }
        public override void Read()
        {
            StreamReader reader = File.OpenText(@"C: \Users\Public\TestFolder\" + this.Name + ".txt");
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] items = line.Split(':');
                int myInteger = int.Parse(items[1]);
                string path = null;
                foreach (string item in items)
                {
                    if (item.StartsWith("item\\") && item.EndsWith(".ddj"))
                        path = item;
                }
            }
        }
    }
    /// <summary>
    /// The map containing the battlefield
    /// </summary>
    public class Map
    {
        public int Size { get; set; }
        private int BuildingCount { get; set; }
        public int unitSize { get; set; }
        private Random _random = new Random(Guid.NewGuid().GetHashCode());
        private string[,] _mapArray;
        private Unit[] units;
        private Building[] buildings;
        public Unit[] Units { get { return units; } set { units = value; } }
        public Array MapArray { get { return _mapArray; } }
        public Map(int size = 20)
        {
            Size = size;
            unitSize = size;
            BuildingCount = 4;
            _mapArray = new string[Size, Size];
            units = new Unit[Size];
            buildings = new Building[BuildingCount];
        }
        /// <summary>
        /// Randomly generate map with random units. The total number of units
        /// will be equal to the dimention of the map (20 x 20 = 20 units)
        /// </summary>
        public void GenerateMap()
        {
            // Fill positions with default '.'
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    _mapArray[i, j] = ".";
                }
            }
            for (int i = 0; i < BuildingCount; i++)
            {
                // 50 - 50 pe cent change to generate either factory or resource building
                var number = _random.Next(3);
                Building building;
                    switch(number)
                {
                    case 1:
                        building = new ResourceBuilding("#");
                        break;
                    case 2:
                        building = new FactoryBuilding("*");
                        break;
                    default:
                        building = new BarracksBuilding("B");
                        break;
                }
                var newX = _random.Next(0, Size - 1);
                var newY = _random.Next(0, Size - 1);
                //Set building position = Incapable of moving
                SetBuildingPosition(ref building, newX, newY);
                // Add buildings to list of buildings
                buildings[i] = building;
            }
                // Random unit generation
                for (int i = 0; i < Size; i++)
            {               
                var number = _random.Next(5);
                Unit unit;
                    switch (number)
                    {
                        case 1:
                            unit = new MeleeUnit(Size);
                            break;
                        case 2:
                            unit = new RangedUnit(Size);
                            break;
                        case 3:
                            unit = new PeasantFighter(Size);
                            break;
                        case 4:
                            unit = new Mercenary(Size);
                            break;
                        default:
                            unit = new SiegeUnit(Size);
                            break;
                }                
                var newX = _random.Next(0, Size - 1);
                var newY = _random.Next(0, Size - 1);
                // Move unit to position and update the unit Properties
                MoveUnitToPosition(ref unit, newX, newY);
                // Add unit to list of units
                units[i] = unit;
            }
        }
        public void UpdateMapArray()
        {
            // Fill positions with default '.'
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    _mapArray[i, j] = ".";
                }
            }
            for (int i = 0; i < BuildingCount; i++)
            {
                var building = buildings[i];
                if (building.Health > 0)
                    SetBuildingPosition(ref building, building.X_Position, building.Y_Position);
            }
            for (int i = 0; i < Size; i++)
            {
                var unit = units[i];
                if (unit.Team != 0)
                {
                    MoveUnitToPosition(ref unit, unit.X_Position, unit.Y_Position);
                }
            }
        }
        public void SetBuildingPosition(ref Building building, int x, int y)
        {
            _mapArray[x, y] = building.Symbol;
            UpdateBuildingPosition(ref building, x, y);
        }
        public void MoveUnitToPosition(ref Unit unit, int x, int y)
        {
            _mapArray[x, y] = unit.Symbol;
            UpdateUnitPosition(ref unit, x, y);
        }
        public void UpdateUnitPosition(ref Unit unit, int x, int y)
        {
            unit.X_Position = x;
            unit.Y_Position = y;
        }
        public void UpdateBuildingPosition(ref Building building, int x, int y)
        {
            building.X_Position = x;
            building.Y_Position = y;
        }
    }
    /// <summary>
    /// Game engine containing the rules of the game
    /// </summary>
    public class GameEngine
    {
        public GameEngine(){}
        public bool UpdateMap(ref Map map, out string winner)
        {
            winner = String.Empty;
            var units = map.Units;
            foreach (var unit in units)
            {
                // Get closest enemy coordinates for all units
                unit.ReturnPositionOfClosestEnemy(units);
                // Check if combat is possible, if yes then attack, else move closer
                // Check if in range
                if (!unit.IsUnitInAttackRange(units))
                {
                    // Move closer to closest enemy
                    unit.MoveToNewPosition();
                }
                else
                {
                    // Attack
                    if (unit.Health > 25)
                    {
                        unit.HandleCombat(ref units);
                    }
                    else
                    {
                        //Run away
                        unit.MoveToNewPosition();
                    }
                }
                // Check if unit should be removed
                if(unit.Health <= 0)
                {
                    unit.Team = 0;
                }
            }
            // Cheack if game is over, whether units still exist for both teams
            var team1 = units.Where(x => x.Health > 0 && x.Team == 1).ToList().Count;
            var team2 = units.Where(x => x.Health > 0 && x.Team == 2).ToList().Count;
            if (team1 == 0 || team2 == 0)
            {
                if (team1 == 0 && team2 != 0)
                {
                    winner = "Team Blue won!";
                }
                if (team2 == 0 && team1 != 0)
                {
                    winner = "Team Red won!";
                }
                if (team2 == 0 && team1 == 0)
                {
                    winner = "Draw!";
                }
                return false;
            }
            map.Units = units;
            return true;
        }
    }
}
