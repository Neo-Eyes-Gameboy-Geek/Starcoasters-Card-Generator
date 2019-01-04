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
using System.IO;
using Microsoft.Win32;
using System.Drawing;

namespace Starcoasters_Card_Generator
{
    /// <summary>
    /// Interaction logic for CardEditor.xaml
    /// </summary>
    public partial class CardEditor : Window
    {
        //these are varibles the whole window will need
        public string CardsSet;
        public string CardsCode;
        public bool IsCardNew;
        //This one is specifically for generating a card code thats displayed on the card
        public string CardCode;
        public CardEditor(string CardSet, bool NewCard, string CardCode)
        {
            InitializeComponent();
            //set the values of the necessary varibles
            CardsSet = CardSet;           
            IsCardNew = NewCard;
            CardsCode = CardCode;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //Get the list of card types from a text file
                string ExecutePath = System.IO.Directory.GetCurrentDirectory();
                //assuming the file exists get its full contents
                if (File.Exists($"{ExecutePath}\\CardTypes.txt"))
                {
                    string FullCardTypes = System.IO.File.ReadAllText($"{ExecutePath}\\CardTypes.txt");
                    //split it up based on # deliminators into an array
                    string[] CardTypes = FullCardTypes.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                    // for each of these array elements make a combo box item for the first combo box
                    foreach(string CardType in CardTypes)
                    {                        
                        ComboBoxItem item = new ComboBoxItem();
                        item.Content = CardType.Trim();
                        CMB_CardType.Items.Add(item);
                    }
                }
                if(IsCardNew == false)
                {
                    //Needed for passing to the combo box updater
                    string TypeComboIndex = "";
                    //if the card is a pre-existing one thats been brought in from the set list not the add new button
                    //query the database to get the information needed from the index required
                    string GetCard = $"SELECT * FROM {CardsSet} WHERE card_code='{CardsCode}'";
                    SQLiteCommand GetCardCommand = new SQLiteCommand(GetCard, Globals.GlobalVars.DatabaseConnection);
                    SQLiteDataReader GetCardReader = GetCardCommand.ExecuteReader();
                    //Get the card details out of the reader
                    if (GetCardReader.Read())
                    {
                        //First set the names in the text boxes at the top
                        TBX_CardName.Text = GetCardReader["name_primary"].ToString();
                        TBX_CardSub.Text = GetCardReader["name_secondary"].ToString();
                        //Next get the array of keywords and split them up
                        string[] KeywordArray = GetCardReader["keywords"].ToString().Split(',');
                        // go through all of these keywords and strip out any unnecessary spaces 
                        for(int i = 0; i < KeywordArray.Length; i++)
                        {
                            KeywordArray[i] = KeywordArray[i].ToString().Trim(' ');
                        }
                        //Now take the cards type from the first item in this array
                        //and compare it to the contents of the first combo box, selecting the index that matches if any
                        for(int i = 0; i < CMB_CardType.Items.Count; i++)
                        {
                            //First you need to get the item since you cant do that in one line
                            ComboBoxItem TestedItem = (ComboBoxItem)CMB_CardType.Items.GetItemAt(i);
                            //now compare the contents of that item with what you got from the database
                            if(KeywordArray[0].ToString() == TestedItem.Content.ToString())
                            {
                                //if it matches, set the index and end the loop and save the contents of the matching box
                                CMB_CardType.SelectedIndex = i;
                                TypeComboIndex = TestedItem.Content.ToString();
                                i = CMB_CardType.Items.Count + 1;
                            }                            
                        }
                        //Now update the rest of the combo boxes so that the rest of this code functions correctly
                        UpdateSubComboBoxes(TypeComboIndex);
                        //Now loop through all the contents just updated and get the one used for each combo box
                        //Start with CMB_Form
                        for(int i =0; i<CMB_Form.Items.Count; i++)
                        {
                            //Get the item out of the combobox
                            ComboBoxItem item = new ComboBoxItem();
                            item = (ComboBoxItem)CMB_Form.Items.GetItemAt(i);
                            //Get its contents and compare to the second Keyword, if it matches set the Combobox index to i and break the loop
                            if (KeywordArray[1] == item.Content.ToString())
                            {
                                CMB_Form.SelectedIndex = i;
                                break;
                            }
                        }
                        //Now CMB_Species
                        for (int i = 0; i < CMB_Species.Items.Count; i++)
                        {
                            //Get the item out of the combobox
                            ComboBoxItem item = new ComboBoxItem();
                            item = (ComboBoxItem)CMB_Species.Items.GetItemAt(i);
                            //Get its contents and compare to the second Keyword, if it matches set the Combobox index to i and break the loop
                            if (KeywordArray[2] == item.Content.ToString())
                            {
                                CMB_Species.SelectedIndex = i;
                                break;
                            }
                        }
                        //CMB_Gender                        
                        for (int i = 0; i < CMB_Gender.Items.Count; i++)
                        {
                            //Get the item out of the combobox
                            ComboBoxItem item = new ComboBoxItem();
                            item = (ComboBoxItem)CMB_Gender.Items.GetItemAt(i);
                            //Get its contents and compare to the second Keyword, if it matches set the Combobox index to i and break the loop
                            if (KeywordArray[3] == item.Content.ToString())
                            {
                                CMB_Gender.SelectedIndex = i;
                                break;
                            }
                        }
                        //CMB_Affiliation
                        for (int i = 0; i < CMB_Affiliation.Items.Count; i++)
                        {
                            //Get the item out of the combobox
                            ComboBoxItem item = new ComboBoxItem();
                            item = (ComboBoxItem)CMB_Affiliation.Items.GetItemAt(i);
                            //Get its contents and compare to the second Keyword, if it matches set the Combobox index to i and break the loop
                            if (KeywordArray[4] == item.Content.ToString())
                            {
                                CMB_Affiliation.SelectedIndex = i;
                                break;
                            }
                        }
                        //Now CMB_Class
                        for (int i = 0; i < CMB_Class.Items.Count; i++)
                        {
                            //Get the item out of the combobox
                            ComboBoxItem item = new ComboBoxItem();
                            item = (ComboBoxItem)CMB_Class.Items.GetItemAt(i);
                            //Get its contents and compare to the second Keyword, if it matches set the Combobox index to i and break the loop
                            if (KeywordArray[5] == item.Content.ToString())
                            {
                                CMB_Class.SelectedIndex = i;
                                break;
                            }
                        }
                        //Lastly CMB_Rules
                        for (int i = 0; i < CMB_Rules.Items.Count; i++)
                        {
                            //Get the item out of the combobox
                            ComboBoxItem item = new ComboBoxItem();
                            item = (ComboBoxItem)CMB_Rules.Items.GetItemAt(i);
                            //Get its contents and compare to the second Keyword, if it matches set the Combobox index to i and break the loop
                            if (KeywordArray[6] == item.Content.ToString())
                            {
                                CMB_Rules.SelectedIndex = i;
                                break;
                            }
                        }
                        // And for any edgelords with a custom keyword
                        TBX_CustomKeyword.Text = KeywordArray[7];
                        //Now do the same as above for the cost box
                        for(int i = 0; i < CMB_CostSelector.Items.Count; i++)
                        {
                            //as per normal get the item out of the combobox at i
                            ComboBoxItem item = new ComboBoxItem();
                            item = (ComboBoxItem)CMB_CostSelector.Items.GetItemAt(i);
                            //if it matches whats in the cost column of the grabbed data
                            if(item.Content.ToString() == GetCardReader["cost"].ToString())
                            {
                                // make the combobox match the current index
                                CMB_CostSelector.SelectedIndex = i;
                                //and break the loop since we are done here
                                break;

                            }
                        }
                        //now for the HP, ATK and DEF textboxes which are simple copies
                        TBX_CardHP.Text = GetCardReader["hp"].ToString();
                        TBX_CardATK.Text = GetCardReader["atk"].ToString();
                        TBX_CardDEF.Text = GetCardReader["def"].ToString();
                        //More substitution for the flavour text and image path by the same means
                        TBX_FlavourText.Text = GetCardReader["flavour"].ToString();
                        TBX_ImagePath.Text = GetCardReader["imagestring"].ToString();
                        //Did the Abilities Last cos i hadnt figured out how to format them yet
                        // put the full set of text into a string because its cleaner to type
                        string AbilityString = GetCardReader["ability"].ToString();
                        //Split it up based on commas as this is how I deliminated different abilities
                        string[] AbilityArray = AbilityString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        // then for each ability they have make a new item in the stack panel
                        foreach(string Ability in AbilityArray)
                        {                            
                            //Split the ability into its 3 parts of name , cost and effect
                            string[] SplitAbility = Ability.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            //Now make a listviewitem for this ability
                            MakeAbilityBox(SplitAbility[0], SplitAbility[1], SplitAbility[2]);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                // If something goes wrong somehow show an error explaining what went wrong then kill the application
                MessageBox.Show($"An error occured {ex}");
                Application.Current.Shutdown();
            }
        }
        public void UpdateSubComboBoxes(string TypeComboString)
        {
            //This basically just sees what type of card is selected in the first combo box and changes the contents of their collection based on a text file
            //First clear all the existing combo boxes            
            CMB_Form.Items.Clear();
            CMB_Species.Items.Clear();
            CMB_Gender.Items.Clear();
            CMB_Affiliation.Items.Clear();
            CMB_Class.Items.Clear();
            CMB_Rules.Items.Clear();
            //get the filepath of the text file, this will be the same path as the executable file
            string ExecutablePathway = System.IO.Directory.GetCurrentDirectory();
            //Make sure the file actually exists and if not create it
            if (File.Exists($"{ExecutablePathway}\\KeywordList.txt"))
            {
                // if it does get the full set of its contents
                string FullKeywordText = System.IO.File.ReadAllText($"{ExecutablePathway}\\KeywordList.txt");
                //next split the full set of text up based on the card type, these being deliminated by # and ignore the empty entry that seems to occur with the text file
                string[] KeywordBlocks = FullKeywordText.Split(new char[] {'#'}, StringSplitOptions.RemoveEmptyEntries);
                //now go through these blocks and compare them to the content of the first combo box stopping if the match is found                            
                foreach (string KeywordBlock in KeywordBlocks)
                {                    
                    if (KeywordBlock.Contains(TypeComboString))
                    {                       
                        //now we know what type of card this is (since we are in this code block) split up its paticular block of keywords into groups for each combobox
                        //Deliminated by |                        
                        string[] ComboKeywords = KeywordBlock.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        //Now split each seperate type up into its individual keywords and put them into their respective combo boxes
                        //Start with the second combobox which holds the form keywords like humanoid
                        string[] FormKeywords = ComboKeywords[1].Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        //now for every entry in this list barring the first which just reads CMB_Form make a combo box item for it and add it to CMB_Form
                        for(int i = 1; i< FormKeywords.Length; i++)
                        {
                            ComboBoxItem item = new ComboBoxItem();
                            item.Content = FormKeywords[i].Trim();
                            CMB_Form.Items.Add(item);
                        }
                        //Do the same for the other comboboxes Species, Gender, Affiliation, Class and Rules
                        string[] SpeciesKeywords = ComboKeywords[2].Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 1; i < SpeciesKeywords.Length; i++)
                        {
                            ComboBoxItem item = new ComboBoxItem();
                            item.Content = SpeciesKeywords[i].Trim();
                            CMB_Species.Items.Add(item);
                        }
                        string[] GenderKeywords = ComboKeywords[3].Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 1; i < GenderKeywords.Length; i++)
                        {
                            ComboBoxItem item = new ComboBoxItem();
                            item.Content = GenderKeywords[i].Trim();
                            CMB_Gender.Items.Add(item);
                        }
                        string[] AffiliationKeywords = ComboKeywords[4].Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 1; i < AffiliationKeywords.Length; i++)
                        {
                            ComboBoxItem item = new ComboBoxItem();
                            item.Content = AffiliationKeywords[i].Trim();
                            CMB_Affiliation.Items.Add(item);
                        }
                        string[] ClassKeywords = ComboKeywords[5].Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 1; i < ClassKeywords.Length; i++)
                        {
                            ComboBoxItem item = new ComboBoxItem();
                            item.Content = ClassKeywords[i].Trim();
                            CMB_Class.Items.Add(item);
                        }
                        string[] RuleKeywords = ComboKeywords[6].Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 1; i < RuleKeywords.Length; i++)
                        {
                            ComboBoxItem item = new ComboBoxItem();
                            item.Content = RuleKeywords[i].Trim();
                            CMB_Rules.Items.Add(item);
                        }
                        //then break the loop to avoid wasting clock cycles
                        break;
                    }                    
                }                
            }

        }

        private void CMB_CardType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //Get the contents of the new selection then update all of the comboboxes to match the new card type
                ComboBoxItem NewSelection = (ComboBoxItem)CMB_CardType.SelectedItem;
                string NewSelectedType = NewSelection.Content.ToString();
                //update the combo boxes giving it the new selection
                UpdateSubComboBoxes(NewSelectedType);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"An error has occured \r\n {ex}");
            }
        }
        public void MakeAbilityBox(string AbilName , string AbilCost , string AbilEffect)
        {
            //Makes a listview item and adds it to the ability list stack panel
            ListViewItem item = new ListViewItem();
            //and three text boxes for the ability one for name, cost and effect
            TextBox TBX_Name = new TextBox();            
            TBX_Name.Text = AbilName.Trim();
            TextBox TBX_Cost = new TextBox();
            TBX_Cost.Text = AbilCost.Trim();
            TextBox TBX_Effect = new TextBox();
            TBX_Effect.Text = AbilEffect.Trim();
            //Make a stackpanel and put the textboxes in it
            StackPanel cnt = new StackPanel();
            cnt.Children.Add(TBX_Name);
            cnt.Children.Add(TBX_Cost);
            cnt.Children.Add(TBX_Effect);
            item.Content = cnt;
            //add item as a child of the stack panel in the XAML
            LIV_AbilityPanel.Items.Add(item);
        }

        private void BTN_AddAbility_Click(object sender, RoutedEventArgs e)
        {
            //This simply adds another ability box to the list view by calling
            //the Make Ability Box with a set of dummy text
            MakeAbilityBox("Name", "Cost", "Effects");
        }

        private void BTN_RemoveAbility_Click(object sender, RoutedEventArgs e)
        {
            //This removes the selected list box item
            //so long as you have something selected in the abilities table list box anyway
            if(LIV_AbilityPanel.SelectedIndex >= 0)
            {
                LIV_AbilityPanel.Items.RemoveAt(LIV_AbilityPanel.SelectedIndex);
            }
        }

        private void BTN_ImagePathSearch_Click(object sender, RoutedEventArgs e)
        {
            //This lets the user choose what image is used for this paticular card, getting its file path
            OpenFileDialog ImageSelector = new OpenFileDialog();
            //Filter it to just show image files, ones i like anyway
            ImageSelector.Filter = "Image Files (*.png, *.jpg, *.bmp)| *.jpg; *.png; *.bmp;";
            //since you will only ever need one image file disable multiselect
            ImageSelector.Multiselect = false;
            ImageSelector.ShowDialog();
            //now if they select a file get its path and set the image path textbox to it
            if(ImageSelector.FileName != null)
            {
                TBX_ImagePath.Text = ImageSelector.FileName.ToString();                
            }
        }

        private void BTN_SaveCard_Click(object sender, RoutedEventArgs e)
        {
            //Save the card then close the window
            SaveCard();
            this.Close();
        }
        public void SaveCard()
        {
            try
            {
                // Get the values are write them back into the database for future perusal, then close the window
                // First of all get the Primary and Secondary Names from the name textboxes
                string CardNamePrimary = MakeStringEscaped(TBX_CardName.Text);
                string CardNameSub = MakeStringEscaped(TBX_CardSub.Text);
                //Now for the awful bit, getting the keywords from the various combo boxes
                string KeywordsString = "";
                // Start with the card type
                KeywordsString += GetComboboxText(CMB_CardType);
                // and for the rest
                KeywordsString += ", " + GetComboboxText(CMB_Form);
                KeywordsString += ", " + GetComboboxText(CMB_Species);
                KeywordsString += ", " + GetComboboxText(CMB_Gender);
                KeywordsString += ", " + GetComboboxText(CMB_Affiliation);
                KeywordsString += ", " + GetComboboxText(CMB_Class);
                KeywordsString += ", " + GetComboboxText(CMB_Rules);
                //Lastly just add whatever is in the custom keyword textbox
                KeywordsString += ", " + TBX_CustomKeyword.Text;
                //now make sure the keyword string is escaped
                KeywordsString = MakeStringEscaped(KeywordsString);
                //Now for the card cost
                string CardCostString = MakeStringEscaped(GetComboboxText(CMB_CostSelector));
                //Now for Hp, ATK and DEF
                string CardHP = MakeStringEscaped(TBX_CardHP.Text);
                string CardATK = MakeStringEscaped(TBX_CardATK.Text);
                string CardDEF = MakeStringEscaped(TBX_CardDEF.Text);
                //Now for the abilities
                string Abilities = "";
                for (int i = 0; i < LIV_AbilityPanel.Items.Count; i++)
                {
                    //First of all get the list view item
                    ListViewItem item = (ListViewItem)LIV_AbilityPanel.Items.GetItemAt(i);
                    //now pull the stack panel out of the listview item
                    StackPanel panel = (StackPanel)item.Content;
                    string abilitytext = "";
                    //now get the 3 textboxes out of the stackpanel
                    foreach (TextBox box in panel.Children)
                    {
                        abilitytext += box.Text + ":";
                    }
                    Abilities += abilitytext + ",";
                }
                //now make sure the ability string is properly escaped
                Abilities = MakeStringEscaped(Abilities);
                // now just for the flavour text and filepath for the image
                string FlavourString = MakeStringEscaped(TBX_FlavourText.Text);
                string FilepathString = MakeStringEscaped(TBX_ImagePath.Text);
                //now to make an sqlite query and update the table
                //The query differs depending on if you are updating a card or adding a new one
                string SaveCardQuery = "";
                if (IsCardNew == true)
                {
                    //if the card is new the Card Code that was sent here needs to be written into the table
                    string NewCardSetCode = CardsCode;
                    SaveCardQuery = $"INSERT INTO {CardsSet} (card_code, name_primary, name_secondary, cost, hp, atk, def, keywords, ability, flavour, imagestring)" +
                    $"VALUES ('{NewCardSetCode}', '{CardNamePrimary}', '{CardNameSub}', '{CardCostString}', '{CardHP}', '{CardATK}', '{CardDEF}', '{KeywordsString}', " +
                    $"'{Abilities}', '{FlavourString}', '{FilepathString}')";
                }
                else
                {
                    SaveCardQuery = $"UPDATE {CardsSet} SET name_primary = '{CardNamePrimary}', name_secondary = '{CardNameSub}', cost = '{CardCostString}', hp = '{CardHP}', " +
                        $"atk = '{CardATK}', def = '{CardDEF}', keywords = '{KeywordsString}', ability = '{Abilities}', flavour = '{FlavourString}', imagestring =' {FilepathString}'" +
                        $" WHERE card_code = '{CardsCode}'";
                }
                //Now actually execute the query
                SQLiteCommand SaveCardCommand = new SQLiteCommand(SaveCardQuery, Globals.GlobalVars.DatabaseConnection);
                SaveCardCommand.ExecuteNonQuery();
                IsCardNew = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occured {ex}");
            }
        }
        private string GetComboboxText(ComboBox item)
        {
            //get the combobox item that is selected from the sent combobox assuming one is selected
            if (item.SelectedItem != null)
            {
                ComboBoxItem SelectedItem = (ComboBoxItem)item.SelectedItem;
                //Get the text from this item
                string Words = SelectedItem.Content.ToString();
                return Words;
            }
            else
            {
                return "placeholder";
            }
        }

        private void BTN_PreviewCard_Click(object sender, RoutedEventArgs e)
        {
            //Save the card first so the preview is as accurate as possible
            SaveCard();
            //Now get a bitmap generated for the card
            Bitmap map = Functions.GenerateCardImage(CardsSet,TBX_ImagePath.Text, CardsCode);
            //Convert that bitmap to a bitmap source that can be displayed and display it
            BitmapSource preview = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(map.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(map.Width, map.Height));
            IMG_CardPreviewer.Source = preview;
            //Cleaning up after myself
            map.Dispose();
        }
        public string MakeStringEscaped(string input)
        {
            //this will make sure any strings put into the database have the ' properly escaped since it doesnt really care if any
            //other character is inserted in a string
            string returnstring = "";
            //now I need to go through for every character in the given string and make sure if its a ' add another ' to escape it
            foreach(char c in input)
            {
                if (c == '\'')
                {
                    returnstring += $"''";
                }
                else
                {
                    returnstring += c;
                }
            }
            //once all the string has been properly escaped return the modified string
            return returnstring;
        }
    }
}
