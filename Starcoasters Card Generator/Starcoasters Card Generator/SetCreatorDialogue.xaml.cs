using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SQLite;

namespace Starcoasters_Card_Generator
{
    /// <summary>
    /// Interaction logic for SetCreatorDialogue.xaml
    /// </summary>
    public partial class SetCreatorDialogue : Window
    {
        public SetCreatorDialogue()
        {
            InitializeComponent();
        }

        private void BTN_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TBX_SetName_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                //Get Whatever is in the textbox as a string and split it up into an array of strings
                string TextboxContents = TBX_SetName.Text.ToString();
                string[] SplitName = TextboxContents.Split(' ');
                //This String Holds The Set Code
                string SetCode = "";
                //Get the first letter of each word in the textbox
                foreach(string word in SplitName)
                {
                    //make sure the index actually exists and the set code isnt already too long
                    if (word.Length > 0 && word[0].ToString() != "" && word[0].ToString() != " " && SetCode.Length < 4)
                    {
                        SetCode += word[0];
                    }
                }
                //Make sure the set code is 4 characters long
                if(SetCode.Length < 4)
                {
                    //append stuff depending on string length to make it 4 characters long
                    if(SetCode.Length == 0)
                    {
                        SetCode += "xxxx";
                    }
                    else if(SetCode.Length == 1)
                    {
                        SetCode += "xxx";
                    }
                    else if (SetCode.Length == 2)
                    {
                        SetCode += "xx";
                    }
                    else
                    {
                        SetCode += "x";
                    }
                }               
                //Once the string is at 4 characters convert it all to lower case and set the text to it
                TBL_SetCode.Text = SetCode.ToLowerInvariant();
            }
            catch (Exception ex)
            {
                //Something went wrong but the outer catch handles it else something explodes
                MessageBox.Show($"An Error Occured {ex}");
            }
        }

        private void BTN_MakeSet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //make sure they havent given you nothing
                if (TBX_SetName.Text == null || TBX_SetName.Text==" ")
                {
                    return;
                }
                //Get information on all the tables in the database
                string GetTableNames = "SELECT * FROM sqlite_master WHERE type='table'";
                SQLiteCommand GetCodeCommand = new SQLiteCommand(GetTableNames, Globals.GlobalVars.DatabaseConnection);
                SQLiteDataReader SetCodeReader = GetCodeCommand.ExecuteReader();
                //a flag as to whether the code conflicts
                bool ConflictFlag = false;
                //Go through each of the rows the reader returns
                while (SetCodeReader.Read())
                {
                    //make sure its not a system table
                    bool Internaltable = false;
                    if (SetCodeReader["name"].ToString().Contains("sqlite_"))
                    {
                        Internaltable = true;
                    }
                    if (Internaltable != true)
                    {
                        //Execute a query that selects the table so we can gather data from it
                        string GetSetCode = $"SELECT * FROM {SetCodeReader["name"].ToString()}";
                        SQLiteCommand GetTableCodeCommand = new SQLiteCommand(GetSetCode, Globals.GlobalVars.DatabaseConnection);
                        SQLiteDataReader CodeReader = GetTableCodeCommand.ExecuteReader();
                        if (CodeReader.Read())
                        {
                            //extracts the returned cards code and splits it into two part, the 4 letter code and the 4 digit number 
                            string CodeToSplit = CodeReader["card_code"].ToString();
                            string[] SplitCode = CodeToSplit.Split('-');
                            //Compares the 4 letter code to the text generated before hand 
                            if (TBL_SetCode.Text.ToString() == SplitCode[0].ToString())
                            {
                                ConflictFlag = true;
                            }
                        }
                    }
                }
                //If there isnt a conflict with the setcode make the table with the name and code
                if (ConflictFlag != true)
                {
                    //Get the Set Name and Setcode as varibles so it can be used in the make table command
                    string SetName = TBX_SetName.Text.ToString();
                    //remove whitespacing from the name cos SQL cant handle that shit
                    SetName = SetName.Replace(' ','_');
                    //remove special characters cos SQL also cant handle this shit (fuckin Crona)
                    SetName = SetName.Replace('-', '_').Replace('&','_');
                    string SetCode = TBL_SetCode.Text.ToString();
                    //Make the Command string, make a command, execute the command
                    string MakeSetTable = $"CREATE TABLE {SetName}" +
                        $"(id INTEGER PRIMARY KEY AUTOINCREMENT," +
                        $"card_code VARCHAR NOT NULL," +
                        $"name_primary VARCHAR NOT NULL," +
                        $"name_secondary VARCHAR NOT NULL," +
                        $"cost VARCHAR NOT NULL," +
                        $"hp INTEGER NOT NULL," +
                        $"atk INTEGER NOT NULL," +
                        $"def INTEGER NOT NULL," +
                        $"keywords TEXT NOT NULL," +
                        $"ability TEXT NOT NULL," +
                        $"flavour TEXT NOT NULL," +
                        $"imagestring TEXT NOT NULL," +
                        $"rarity TEXT NOT NULL," +
                        $"artist TEXT NOT NULL)";
                    SQLiteCommand MakeTable = new SQLiteCommand(MakeSetTable, Globals.GlobalVars.DatabaseConnection);
                    MakeTable.ExecuteNonQuery();
                    //Add a single card to the table to avoid errors
                    string InitialCardCommand = $"INSERT INTO {SetName} (card_code, name_primary, name_secondary, cost, hp, atk, def, keywords, ability, flavour, imagestring, rarity, artist)" +
                        $" VALUES ('{SetCode}-0001', 'Testacles', 'The Debug Centurion', '0', 100, 100, 100, 'Character, Humanoid, Human, Male, Foundation, Centurion, Marksman, OPPLZNERF', " +
                        $"'Debug God: When you deploy this card: You Win The Game| Yes Really: When You Deploy This Card: You Win The Game', 'Bow before the might of Testacles','C:\','ultima','Ya Mum')";
                    SQLiteCommand InsertCard = new SQLiteCommand(InitialCardCommand, Globals.GlobalVars.DatabaseConnection);
                    InsertCard.ExecuteNonQuery();
                    //once the table is made close this window cos its done its job
                    this.Close();
                }
                //if there is a conflict alert the user to it and continue on
                else
                {
                    MessageBox.Show("The setcode generated by the name provided would conflict with the code of an existing set. Try and different name.");
                }
            }
            catch(Exception ex)
            {
                // What went wrong? Better kill it before it breeds.
                MessageBox.Show($"An Error Occured {ex}");
                Application.Current.Shutdown();
            }
        }
    }
}
