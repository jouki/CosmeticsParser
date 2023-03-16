using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CosmeticsParser
{
    public class Character {

        public string name;
        public int dbdID;
        public int wikiID;

        private static readonly string charactersJson = Program.GetDataFromUrl(Program.LinkBase + "characters");
        private static Dictionary<string, dynamic> characters = JsonHelper.Deserialize(charactersJson)["data"];

        //Ideal result = 0
        public static readonly List<dynamic> incorrectCharNames = characters.Values.Select(x => x["Name"]).ToList()
            .Where(x => !WikiMappers.Survivors.Values.Any(y => y.Equals(x)) &&
                        !WikiMappers.Killers.Values.Any(y => (/*"The " + */y).Equals(x))).ToList();

        public static readonly bool anyIncorrectCharNames = incorrectCharNames.Count == 0;

        private static Dictionary<int, int> _survivors;
        public static Dictionary<int, int> Survivors
        {
            get
            {
                if( _survivors == null)
                {
                    Console.WriteLine("Processing DBD API Table Survivors...");
                    _survivors = new Dictionary<int, int>();
                    foreach(var (key, value) in WikiMappers.Survivors.Select(x => (x.Key, x.Value)))
                    {
                        if(characters.Any(x => x.Value["Name"] == value))
                        {
                            var survName = int.Parse(characters.First(x => x.Value["Name"] == value).Key);
                            _survivors.Add(key, survName);
                        }
                    }
                }
                return _survivors;
            }
        }

        private static Dictionary<int, int> _killers;
        public static Dictionary<int, int> Killers
        { 
            get
            { 
                if (_killers == null) {
                    Console.WriteLine("Processing DBD API Table Killers...");
                    _killers = new Dictionary<int, int>();
                    foreach(var (key, value) in WikiMappers.Killers.Select(x => (x.Key, x.Value))) {
                        if(characters.Any(x => x.Value["Name"] == value))
                        {
                            var killerName = int.Parse(characters.FirstOrDefault(x => x.Value["Name"] == value).Key);
                            _killers.Add(key, killerName);
                        }
                    }
                }
                return _killers; 
            }
        }


        private static Dictionary<int, int> _allCharacters;
        public static Dictionary<int, int> AllCharacters //<dbdID, wikiID>
        {
            get
            {
                if(_allCharacters == null)
                {
                    _allCharacters = new List<Dictionary<int, int>>() { Survivors, Killers }.SelectMany(x => x).ToDictionary(x => x.Value, x => x.Key); //switch key with value so it's unique Key (DBD ID) having Wiki Character ID
                }
                return _allCharacters;
            }
        }

        public Character(int dbdID)
        {
            dynamic character = new object();
            try
            {
                characters.TryGetValue(dbdID.ToString(), out character);
                //var character = characters.Values.Select(x => x[""]);

                this.dbdID = dbdID;
                this.wikiID = dbdID >= 0 ? AllCharacters[dbdID] : -1;
                this.name = dbdID > 0 ? character["Name"] : "";
            }catch(Exception ex)
            {
                
                throw new Exception("Failed when fetching character. Is the Wiki tables (survivors and killers) up to date?" +
                    "\nName: " + character["Name"] +
                    "\nDBD API ID: " + dbdID);
            }
        }
    }
}
