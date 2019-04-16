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
using System.Drawing;
using System.IO;
using Microsoft.Win32;



namespace Starcoasters_Card_Generator
{
    /// <summary>
    /// Interaction logic for SetViewer.xaml
    /// </summary>
    public partial class SetViewer : Window
    {       
        public string SetToView;
        public SetViewer(string SelectedSet)
        {
            InitializeComponent();
            // Make sure the window gets the value that the other window passed to it
            //This being the name of the table we are playing in
            SetToView = SelectedSet;            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //When the window is loaded fill in the list
            UpdateCardList();
        }

        private void BTN_Edit_Click(object sender, RoutedEventArgs e)
        {
            //First off make sure there is actually something selected
            if(LIV_CardList.SelectedIndex < 0)
            {
                return;
            }
            try
            {
                //What card we get is based on the
                //Card code so we get that first
                //So get the selected item from the list                
                //Pull the card from the selected listitem
                Classes.CardOverview TagCard = (Classes.CardOverview)LIV_CardList.SelectedItem;
                //and get the full set code from it
                string SetCode = TagCard.CardSetCode;
                //Now after all of that we have a value to give to the card viewer
                CardEditor EditorWindow = new CardEditor(SetToView, false, SetCode);
                EditorWindow.ShowDialog();
                //Make sure this window has the right values once this editor returns
                UpdateCardList();
                

            }
            catch(Exception ex)
            {
                //If something goes wrong somehow show an error explaining what went wrong then kill the application
                MessageBox.Show($"An error occured {ex}");
            }
        }

        private void BTN_Delete_Click(object sender, RoutedEventArgs e)
        {
            //Make sure there is actually something selected, if its empty just stop there
            if(LIV_CardList.SelectedIndex < 0||LIV_CardList.Items.Count<2)
            {
                MessageBox.Show("Either no item was selected or there is only one card left in the set");
                return;
            }
            try
            {
                //Get the card out of the selected items tag
                Classes.CardOverview CardToDelete = (Classes.CardOverview)LIV_CardList.SelectedItem;
                //now write onto a file in the current directory that this card code is available
                using(StreamWriter sw = File.AppendText(Directory.GetCurrentDirectory() + $"\\{SetToView}.txt"))
                {
                    sw.WriteLine(CardToDelete.CardSetCode);
                }
                //Prepare an SQLITE query to delete the card we just selected
                string DeleteCardQuery = $"DELETE FROM {SetToView} WHERE card_code = '{CardToDelete.CardSetCode}'";
                //Execute the query
                SQLiteCommand DeleteCardCommand = new SQLiteCommand(DeleteCardQuery, Globals.GlobalVars.DatabaseConnection);
                DeleteCardCommand.ExecuteNonQuery();
                DeleteCardCommand.Dispose();
                //If all goes well, update the list to refelect the lack of the card
                UpdateCardList();
            }
            catch(Exception ex)
            {
                //If something goes wrong somehow show an error explaining what went wrong then kill the application
                MessageBox.Show($"An error occured {ex}");
            }

        }

        private void BTN_Add_Click(object sender, RoutedEventArgs e)
        {
            string CodeToUse = "";
            //first of all check if there is a file containing unused SetCodes for this set exists, if it does get the first line of it 
            //and use that for the Code, then delete it from the file obviously as to avoid confusion
            string UsableIndexFilePath = Directory.GetCurrentDirectory() + $"\\{SetToView}.txt";
            if (File.Exists(UsableIndexFilePath))
            {
                //if it does get the whole thing
                string[] UsableFilestring = File.ReadAllLines(UsableIndexFilePath);
                //make sure to trim the code down just in case there is a space or something that shouldnt be there
                CodeToUse = UsableFilestring[0].Trim();
                //now if the file is only 1 line long simply delete it so the system will go back to generating the codes
                if(UsableFilestring.Length == 1)
                {
                    File.Delete(UsableIndexFilePath);
                }
                else
                {
                    //now if the file is longer than 1 line
                    //now write everything thats in the array barring the first line because we used that 
                    //just make sure that you only write back everything that isnt the first line
                    for(int i =1; i < UsableFilestring.Length; i++)
                    {
                        using(StreamWriter sw = File.AppendText(UsableIndexFilePath))
                        {
                            sw.WriteLine(UsableFilestring[i]);
                        }
                    }                    
                }
            }
            else
            {
                //if the file doesnt exist then there are no breaks in the set codes so you will have to make a new one from scratch
                CodeToUse = GetCleanSetCode(SetToView);
            }
            //now make a new card editor with the 
            CardEditor editor = new CardEditor(SetToView, true, CodeToUse);
            //now show the new window
            editor.ShowDialog();           
            UpdateCardList();
        }

        //Functions for the window
        public void UpdateCardList()
        {
            //updates the card set list
            try
            {
                //A new list is required for storing the items for the 
                List<Classes.CardOverview> items = new List<Classes.CardOverview>();
                //Get all the data from the table selected with a query
                string GetCardQuery = $"SELECT * FROM {SetToView}";
                SQLiteCommand GetCardCommand = new SQLiteCommand(GetCardQuery, Globals.GlobalVars.DatabaseConnection);
                SQLiteDataReader GetCardReader = GetCardCommand.ExecuteReader();
                //Go through every card in the returned table
                while (GetCardReader.Read())
                {
                    //while there are still cards in the reader to go over, add them to the table 
                    Classes.CardOverview ReaderCard = new Classes.CardOverview();
                    //Fill in the new card with details of the card pulled from the database
                    ReaderCard.CardSetCode = GetCardReader["card_code"].ToString();
                    ReaderCard.CardName = GetCardReader["name_primary"].ToString();
                    ReaderCard.CardNameSecondary = GetCardReader["name_secondary"].ToString();
                    ReaderCard.CardCost = GetCardReader["cost"].ToString();
                    ReaderCard.CardHP = int.Parse(GetCardReader["hp"].ToString());
                    ReaderCard.CardATK = int.Parse(GetCardReader["atk"].ToString());
                    ReaderCard.CardDEF = int.Parse(GetCardReader["def"].ToString());
                    //now the tricky bit, getting the species out of the array of card keywords, however species is always 2nd so thats nice
                    //get the array of keywords
                    string[] CardKeywords = GetCardReader["keywords"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    //Get this keyword into the ReaderCard
                    ReaderCard.CardSpecies = CardKeywords[1];
                    //now gotta get the number of abilities
                    string[] CardAbilities = GetCardReader["ability"].ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    int AbilityCount = 0;
                    //cycle through the split array to set the Ability Count
                    foreach (string Ability in CardAbilities)
                    {
                        AbilityCount++;
                    }
                    //set the ability count to the reader cards ability count
                    ReaderCard.CardAbilityCount = AbilityCount;
                    //add readercard to the list
                    items.Add(ReaderCard);
                }
                //now make the items list the itemsource for the listview
                LIV_CardList.ItemsSource = items;
                //now make a selectionview from the itemsource of the list
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(LIV_CardList.ItemsSource);
                //now sort this based on the cardsetcode and sort it ascendingly
                view.SortDescriptions.Add(new System.ComponentModel.SortDescription("CardSetCode", System.ComponentModel.ListSortDirection.Ascending));
                //and clean up after oneself
                GetCardReader.Close();
                GetCardCommand.Dispose();
            }
            catch (Exception ex)
            {
                //If something goes wrong somehow show an error explaining what went wrong then kill the application
                MessageBox.Show($"An error occured {ex}");
            }
        }
        public string GetCleanSetCode(string SetName)
        {
            try
            {
                //first we need to find out how many elements are in this sets list
                int NumberOfCards = LIV_CardList.Items.Count + 1;
                //now we need to append this to a string that is the finalised code
                //first you need to get the set code prefix from the first item in the list (since there will always be one)                
                Classes.CardOverview Overview = (Classes.CardOverview)LIV_CardList.Items.GetItemAt(0);
                //now this is the card code that will be appended to 
                string ReturnCode = Overview.CardSetCode.Split('-')[0];
                //now we need to add to the set code the number we got earlier, added in with some zeroes as needed
                if (NumberOfCards < 10)
                {
                    ReturnCode += $"-000{NumberOfCards}";
                }
                else if (NumberOfCards < 100)
                {
                    ReturnCode += $"-00{NumberOfCards}";
                }
                else if (NumberOfCards < 1000)
                {
                    ReturnCode += $"-0{NumberOfCards}";
                }
                else
                {
                    ReturnCode += $"-{NumberOfCards}";
                }
                //now return the newly generated code
                return ReturnCode;
            }
            catch(Exception ex)
            {
                //oops something went wrong, tell the user and close the application down
                MessageBox.Show($"An error occured {ex}");
                Application.Current.Shutdown();
                return null;
            }
        }

        private void BTN_ExportBleed_Click(object sender, RoutedEventArgs e)
        {
            //This one will export the cards as full bleed sized to print
            Functions.ExportCards(SetToView, false, false);
        }

        private void BTN_ExportCropped_Click(object sender, RoutedEventArgs e)
        {
            //This one will export the cards as cropped size without the bleed
            Functions.ExportCards(SetToView, true, false);
        }

        private void BTN_ExportVassal_Click(object sender, RoutedEventArgs e)
        {
            //This one will export the cards resized as vassal sized cards for Tabletop Sim
            Functions.ExportCards(SetToView, true, true);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //as the window is closing make sure to close the database connection, to stop it choking 
            //the previous window
            Globals.GlobalVars.DatabaseConnection.Close();
        }
    }
}
