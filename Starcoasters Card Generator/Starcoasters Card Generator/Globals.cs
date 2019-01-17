using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace Starcoasters_Card_Generator
{
    class Globals
    {
        //This is where any global varibles needed are stored
        public static class GlobalVars
        {
            //The SQLite Connection Object
            public static SQLiteConnection DatabaseConnection;
            //the list of cards to be used for the battle mode
            public static List<Classes.CardDetail> AllCards;
        }
    }
}
