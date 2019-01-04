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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SQLite;
using System.IO;

namespace Starcoasters_Card_Generator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BTN_Edit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Extract the listview item that is selected
                if(LIV_SetList.SelectedItem != null)
                {
                    //extract the listview item from the listview
                    ListViewItem SelectedItem = (ListViewItem)LIV_SetList.SelectedItem;
                    //Get the SetOverview from the listview item
                    Classes.SetOverview SelectedSet = (Classes.SetOverview)SelectedItem.Tag;
                    //extract the name from the SetOverview
                    string SelectedSetName = SelectedSet.SetName;
                    //make a new Setviewer window passing it the set name extracted above
                    SetViewer SetViewer = new SetViewer(SelectedSetName);
                    SetViewer.ShowDialog();
                }
                else
                {
                    // if they havent selected anything just return the function
                    return;
                }                
            }
            catch(Exception ex)
            {
                // If something goes wrong somehow show an error explaining what went wrong then kill the application
                MessageBox.Show($"An error occured {ex}");
                Application.Current.Shutdown();
            }
            UpdateSetList();
        }

        private void BTN_Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Make sure there is an item selected to delete
                if (LIV_SetList.SelectedItem != null)
                {
                    //Extract the List Item from the listview
                    ListViewItem SelectedItem = (ListViewItem)LIV_SetList.SelectedItem;
                    //Extract the set overview from the listview item
                    Classes.SetOverview SetToDelete = (Classes.SetOverview)SelectedItem.Tag;
                    string TableToDelete = SetToDelete.SetName.ToString();
                    //now if the table being deleted has an index text file associated with it delete that too because its tidy
                    if (File.Exists(Directory.GetCurrentDirectory() + $"\\{TableToDelete}.txt"))
                    {
                        File.Delete(Directory.GetCurrentDirectory() + $"\\{TableToDelete}.txt");
                    }
                    //Query the table to drop it like its thot
                    string TableDeleteQuery = $"DROP TABLE {TableToDelete}";
                    SQLiteCommand DropTableCommand = new SQLiteCommand(TableDeleteQuery, Globals.GlobalVars.DatabaseConnection);
                    DropTableCommand.ExecuteNonQuery();
                    //cleaning up
                    DropTableCommand.Dispose();
                }
                UpdateSetList();
            }
            catch(Exception ex)
            {
                // If something goes wrong somehow show an error explaining what went wrong then kill the application
                MessageBox.Show($"An error occured {ex}");
                Application.Current.Shutdown();
            }
        }

        private void BTN_Create_Click(object sender, RoutedEventArgs e)
        {
            SetCreatorDialogue SetDialogue = new SetCreatorDialogue();
            SetDialogue.ShowDialog();
            UpdateSetList();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //When The Window Starts, Connect to the database
            Functions.DatabaseConnect();
            UpdateSetList();
        }
        //Functions for the window
        public void UpdateSetList()
        {
            try
            {
                //In Case There Is Anything In The ListView Clear It
                LIV_SetList.Items.Clear();
                //Get information on all the tables in the database
                string GetTableNames = "SELECT name FROM sqlite_master WHERE type='table'";
                SQLiteCommand GetSetCommand = new SQLiteCommand(GetTableNames, Globals.GlobalVars.DatabaseConnection);
                SQLiteDataReader SetReader = GetSetCommand.ExecuteReader();
                //Go through the rows returned by the query, mine them for data and uncerimoniously stuff each into the listview
                while (SetReader.Read())
                {
                    //Check if the table selected is an internal one for sqlite
                    bool TableInternal = SetReader["name"].ToString().Contains("sqlite_");
                    //Make sure it isnts pulling shit from the internally designated databases (which contain sqlite_)
                    if (TableInternal == false)
                    {
                        //Execute a query that selects the table so we can gather data from it
                        string GetSetCode = $"SELECT * FROM {SetReader["name"].ToString()}";
                        SQLiteCommand GetTableCodeCommand = new SQLiteCommand(GetSetCode, Globals.GlobalVars.DatabaseConnection);
                        SQLiteDataReader CodeReader = GetTableCodeCommand.ExecuteReader();
                        //extracts the returned cards code and splits it into two part, the 4 letter code and the 4 digit number 
                        string TempCode = "";
                        if (CodeReader.Read())
                        {
                            string CodeToSplit = CodeReader["card_code"].ToString();
                            string[] SplitCode = CodeToSplit.Split('-');
                            TempCode = SplitCode[0].ToString();
                        }
                        //Do another query to find out the length of the table by looping through every row the data reader can find
                        SQLiteCommand GetLengthCommand = new SQLiteCommand(GetSetCode, Globals.GlobalVars.DatabaseConnection);
                        SQLiteDataReader LengthReader = GetLengthCommand.ExecuteReader();
                        int SetSize = 0;
                        while (LengthReader.Read())
                        {
                            SetSize++;
                        }
                        // Shove all the data just gathered into a Set Overview
                        Classes.SetOverview SetData = new Classes.SetOverview { SetName = SetReader["name"].ToString(), SetCode = TempCode, SetCount = SetSize };
                        //Make a list view item to store this data as a tag
                        ListViewItem SetItem = new ListViewItem();
                        SetItem.Tag = SetData;
                        //also make the set data the item so it displays properly
                        SetItem.Content = SetData;
                        //Add the item to the listview
                        LIV_SetList.Items.Add(SetItem);
                    }
                }
                //cleaning up
                SetReader.Close();
                GetSetCommand.Dispose();
            }
            catch (Exception ex)
            {
                // If something goes wrong somehow show an error explaining what went wrong then kill the application
                MessageBox.Show($"An error occured {ex}");
                Application.Current.Shutdown();
            }

        }
    }
}
