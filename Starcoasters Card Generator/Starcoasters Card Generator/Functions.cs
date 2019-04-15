using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace Starcoasters_Card_Generator
{
    public static class Functions
    {
        //Functions that are used across different windows are stored here.
        //Connect To The Database
        public static void DatabaseConnect()
        {
            try
            {
                //Get the absolute file path of the application for use elsewhere
                string ExecutablePathway = System.IO.Directory.GetCurrentDirectory();
                //Tests if the database exists and creates it if not
                if (File.Exists($"{ExecutablePathway}\\StarcoastersDatabase.db"))
                {
                    //Connect to the card database and open the connection
                    Globals.GlobalVars.DatabaseConnection = new SQLiteConnection($"Data Source={ExecutablePathway}\\StarcoastersDatabase.db; Version=3; Password=KHPJ6SJaT5YPeLmL;");
                    Globals.GlobalVars.DatabaseConnection.Open();
                }
                else
                {
                    //Warn that the database file doesnt exist
                    System.Windows.MessageBox.Show("Tim Clones destroyed the Database, rebuilding.");
                    //Make the database file and make a temporary connection to it
                    SQLiteConnection.CreateFile($"{ExecutablePathway}\\StarcoastersDatabase.db");
                    SQLiteConnection TempConnection = new SQLiteConnection($"Data Source={ExecutablePathway}\\StarcoastersDatabase.db; Version=3;");                    
                    //Set a database password for some arbitrary protection with a password i generated with an online keygen
                    TempConnection.SetPassword("KHPJ6SJaT5YPeLmL");
                    TempConnection.Open();
                    //Close the temporary connection then try this again
                    TempConnection.Close();
                    DatabaseConnect();
                }
            }
            catch(Exception e)
            {
                //If Something goes wrong show a message as to what went wrong and kill the application
                System.Windows.MessageBox.Show($"An error occured {e}");
                System.Windows.Application.Current.Shutdown();
            }
        }
        public static Bitmap GenerateCardImage(string CardSet, string ArtPath, string CardIndex)
        {
            //This will produce and return a bitmap that can either be saved as an image or displayed for preview
            try
            {
                // first of all A bitmap is needed of the background that everything is being placed on top of
                //thankfully this is included in the project, by me
                Bitmap CardBitmap = new Bitmap(System.IO.Directory.GetCurrentDirectory() + "\\CardBackground.png");
                //now draw the artwork and textboxes over this background (which is just there as a backup so there is no pure white background)
                CardBitmap = DrawCardBase(ArtPath, CardBitmap);
                //Now we need to get the information for the Cards text from the database with a query
                string GetCardToRenderQuery = $"SELECT * FROM {CardSet} WHERE card_code='{CardIndex}'";
                SQLiteCommand GetCardToRenderCommand = new SQLiteCommand(GetCardToRenderQuery, Globals.GlobalVars.DatabaseConnection);
                SQLiteDataReader GetCardToRenderReader = GetCardToRenderCommand.ExecuteReader();
                //just as a sanity check make sure the reader returned a legitimate value
                if(GetCardToRenderReader.Read() == true)
                {
                    //Now to get the cards cost value
                    string CardCostString = GetCardToRenderReader["cost"].ToString();
                    //now some variables to make sure the text actually fits the box and alligns properly
                    int FontSize = 72;
                    int TextWidth = 10000;
                    int TextHeight = 10000;                    
                    //make the solidbrush for drawing the cost text
                    SolidBrush CostBrush = new SolidBrush(Color.Black);
                    //Card Cost is 72 Pixel Downlink Bold in Black with its box starting at 83,83 and is an 84 pixel square
                    CardBitmap = DrawText(CardBitmap, 83, 83, 84, 84, CardCostString, 72, "Downlink", "bold", CostBrush, true);
                    CostBrush.Dispose();                    
                    //Now do the same for the cards primary name
                    string CardNamePrimaryString = GetCardToRenderReader["name_primary"].ToString();
                    SolidBrush NameBrush = new SolidBrush(Color.Black);
                    //The Primary Name is written in Bold Classic Robot Condensed at most 72 pixel size in a box at 188,80 that is 540 x 60 pixels
                    CardBitmap = DrawText(CardBitmap, 188, 80, 540, 60, CardNamePrimaryString, 72, "Classic Robot Condensed", "bold", NameBrush, true);                   
                    //Now ditto for the secondary name
                    string CardSecondaryNameString = GetCardToRenderReader["name_secondary"].ToString();
                    //The secondary name is Classic Robot Condensed Regular at 36 pixel size at 208,139 in a box at 500x29 in black
                    CardBitmap = DrawText(CardBitmap, 208, 139, 500, 29, CardSecondaryNameString, 36, "Classic Robot Condensed", "regular", NameBrush, true);
                    //now clean up the brush
                    NameBrush.Dispose();
                    //Now for keywords
                    string KeywordsString = GetCardToRenderReader["keywords"].ToString();                                        
                    //now just split up the keywords, this will get rid of any placeholders
                    string[] KeywordStringArray = KeywordsString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    //some of the keywords are not to be drawn so we need to process this string down to just what we can use
                    KeywordsString = "";
                    int keywordindex = 1;
                    //now make the string with each of the keywords that arent place holder
                    foreach(string keyword in KeywordStringArray)
                    {
                        if (!keyword.Contains("placeholder"))
                        {
                            if (keywordindex == 1 || keyword == " ")
                            {
                                //the first keyword doesnt need the space before it and empty keywords should just be ignored
                                KeywordsString += keyword.Trim();
                            }
                            else if (keyword.Contains("\\"))
                            {
                                //if the keyword is the one containing any abilities draw those abilities in but done include it in the written keywords                                
                                CardBitmap = DrawKeywordIcons(CardBitmap, keyword);
                            }
                            else
                            {
                                //otherwise it needs a comma and the word
                                KeywordsString += ", " + keyword.Trim();
                            }
                        }
                        keywordindex++;
                    }
                    //the brush we will need to draw the keywords in, even though its only black
                    SolidBrush KeywordBrush = new SolidBrush(Color.Black);
                    //now draw the keywords in Classic Robot Condensed, Bold in a box at 85,712 that is 655x36 at most 36 pixel font size
                    CardBitmap = DrawText(CardBitmap, 85, 712, 655, 36, KeywordsString, 36, "Classic Robot Condensed", "bold", KeywordBrush, true);
                    //Now for HP,ATK and DEF
                    string CardHP = GetCardToRenderReader["hp"].ToString();
                    string CardATK = GetCardToRenderReader["atk"].ToString();
                    string CardDEF = GetCardToRenderReader["def"].ToString();
                    //stats are all written in 80 pixel square boxes in downlink bold at at most 72 pixel size of downlink bold
                    //the brush to draw them with is just black
                    SolidBrush StatBrush = new SolidBrush(Color.Black);
                    //start with HP at 85,757
                    CardBitmap = DrawText(CardBitmap, 85, 757, 80, 80, CardHP, 60, "Downlink", "bold", StatBrush, true);
                    //ditto for ATK at 85,847
                    CardBitmap = DrawText(CardBitmap, 85, 847, 80, 80, CardATK, 60, "Downlink", "bold", StatBrush, true);
                    //DEF at 85,937
                    CardBitmap = DrawText(CardBitmap, 85, 937, 80, 80, CardDEF, 60, "Downlink", "bold", StatBrush, true);
                    StatBrush.Dispose();
                    //Onceagain abilities are assholes and need lots of loops and special code to be drawn so i cannot (easily) use my text drawing functions
                    //so this is is for the moment
                    TextWidth = 10000;
                    TextHeight = 10000;
                    int AbilityNameFontSize = 24;
                    int AbilityCostFontSize = 18;
                    FontSize = 16;
                    //3 Fonts are needed here
                    Font AbilityNameFont = new Font("Classic Robot Condensed", AbilityNameFontSize, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);
                    Font AbilityCostFont = new Font("Classic Robot Condensed", AbilityCostFontSize, GraphicsUnit.Pixel);
                    Font AbilityBodyFont = new Font("Classic Robot Condensed", FontSize, GraphicsUnit.Pixel);
                    //This is to determine the Y value of the given abilities top left corner as it will vary based on the number of abilities
                    int AbilityCount=1;
                    string WholeAbilityString = GetCardToRenderReader["ability"].ToString();
                    string[] AbilityArray = WholeAbilityString.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    AbilityCount = AbilityArray.Length;
                    //Because the abilities are evenly spaced out, and with the same X value, the Y value they are drawn at is 714 + the amount of pixels each ability gets times
                    //how many spaces away it is from the first ability at y=714
                    //That number away from the top is what this variable is for
                    int AbilityNumber = 0;
                    foreach(string Ability in AbilityArray)
                    {                        
                        AbilityNameFontSize = 25;
                        AbilityCostFontSize = 24;
                        FontSize = 24;
                        string[] AbilitySplit = Ability.Split(new char[] { ':' });
                        //make the bitmap and graphics
                        //the whole box for abilities is 564x239 so the space has to be shared equally amongst various abilities
                        Bitmap AbilityMap = new Bitmap(564, (int)239 / AbilityCount);
                        Graphics g = Graphics.FromImage(AbilityMap);
                        //make sure the text looks nice
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        do
                        {
                            //clear the bitmap being drawn on
                            g.Clear(Color.Transparent);
                            //start by making the fonts match the new size
                            AbilityNameFont = new Font("Classic Robot Condensed", AbilityNameFontSize, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);
                            AbilityCostFont = new Font("Classic Robot Condensed", AbilityCostFontSize,  GraphicsUnit.Pixel);
                            AbilityBodyFont = new Font("Classic Robot Condensed", FontSize,  GraphicsUnit.Pixel);
                            //Now draw in the ability title, its allowed to be whatever size it wants since it should never be more than a line
                            g.DrawString(AbilitySplit[0], AbilityNameFont,Brushes.Black,0,0);
                            SizeF NameHeight = g.MeasureString(AbilitySplit[0], AbilityNameFont);
                            //now make textheight equal the height of the title
                            TextHeight = (int)NameHeight.Height;
                            TextWidth = (int)g.MeasureString(AbilitySplit[0], AbilityNameFont).Width;
                            //Now do the same for the cost but you have to break up the cost word for word so it doesnt overstep its bounds in the textbox
                            string[] SplitCost = AbilitySplit[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string RenderableCostString = "";
                            foreach(string word in SplitCost)
                            {
                                if(g.MeasureString($"{RenderableCostString}{word} ",AbilityCostFont).Width <= 564)
                                {
                                    //if the current string + the new word and the following space is less than the allowed width just add it
                                    RenderableCostString += $"{word} ";
                                }
                                else
                                {
                                    //if it isnt add a new line and add the word
                                    string Newline = System.Environment.NewLine;
                                    RenderableCostString += $"{Newline}{word} ";
                                }
                            }
                            //Now draw in the ability cost
                            g.DrawString(RenderableCostString, AbilityCostFont, Brushes.Black, 0, TextHeight);
                            //now add the height of this string to the total text height
                            TextHeight += (int)g.MeasureString(RenderableCostString, AbilityCostFont).Height;
                            //and if the text width here is wider than it already is set that width to the new width
                            TextWidth = Math.Max(TextWidth, (int)g.MeasureString(RenderableCostString, AbilityCostFont).Width);
                            //Now do the same thing for the effect of the card
                            string[] SplitEffect = AbilitySplit[2].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string RenderableEffectString = "";
                            foreach(string word in SplitEffect)
                            {
                                if (g.MeasureString($"{RenderableEffectString}{word} ", AbilityCostFont).Width <= 564)
                                {
                                    //if the current string fits the width limitations just add it on
                                    RenderableEffectString += $"{word} ";
                                }
                                else
                                {
                                    //if it doesnt add a new line and the
                                    string Newline = System.Environment.NewLine;
                                    RenderableEffectString += $"{Newline}{word} ";
                                }
                            }
                            //now draw in the effect text
                            g.DrawString(RenderableEffectString, AbilityBodyFont, Brushes.Black, 0, TextHeight);
                            //now update the textwidth and height 
                            TextHeight += (int)g.MeasureString(RenderableEffectString, AbilityBodyFont).Height;
                            //Width is a little odd, because im reusing some variables but if anything is too wide or overall too tall it will be picked up here
                            TextWidth = Math.Max(Math.Max(TextWidth, (int)g.MeasureString(RenderableEffectString, AbilityBodyFont).Height), (int)NameHeight.Width);
                            //if its too big shrink the fontsizes and try again
                            if (TextWidth > 564 || TextHeight > 239 / AbilityCount)
                            {
                                AbilityNameFontSize--;
                                AbilityCostFontSize--;
                                FontSize--;
                            }
                        }
                        while (TextWidth > 564 || TextHeight > 239 / AbilityCount);
                        //now we have an acceptable sized set of text to draw in a nice drawable bitmap draw it in on the card
                        using(Graphics graphics = Graphics.FromImage(CardBitmap))
                        {
                            //because mathimatical order of operations is weird
                            int AbilityHeight = 250 / AbilityCount;
                            AbilityHeight = AbilityHeight * AbilityNumber;
                            AbilityHeight += 755;
                            graphics.DrawImageUnscaledAndClipped(AbilityMap, new Rectangle(176, AbilityHeight, 479, TextHeight));
                        }
                        //now increment the ability number
                        AbilityNumber++;
                        //and clean up
                        g.Dispose();
                        AbilityMap.Dispose();
                    }
                    AbilityNameFont.Dispose();
                    AbilityCostFont.Dispose();
                    AbilityBodyFont.Dispose();
                    //Flavour Text is done in Classic Robot Condensed, Italic at 24 pixels 176,994 in a box 564x25 box in 80%
                    string FlavourString = GetCardToRenderReader["flavour"].ToString();
                    Color FlavourGray = new Color();
                    FlavourGray = ColorTranslator.FromHtml("#414141");
                    SolidBrush FlavourBrush = new SolidBrush(FlavourGray);
                    CardBitmap = DrawText(CardBitmap, 176, 994, 564, 25, FlavourString, 20, "Classic Robot Condensed", "italic", FlavourBrush, false);
                    //now to give credit where credit is due draw the artists name or handle it
                    string ArtistString = GetCardToRenderReader["artist"].ToString();
                    //the artist credit is drawn in the same colour as the flavour text in a box 242x18 at 142,1025 in Classic Robot Condensed Regular
                    CardBitmap = DrawText(CardBitmap, 142, 1025, 142, 20, ArtistString, 20, "Classic Robot Condensed", "regular", FlavourBrush, false);
                    //clean up the extras used
                    FlavourBrush.Dispose();
                    //Lastly for the setcode
                    string Setcode = GetCardToRenderReader["card_code"].ToString();                    
                    //just need to figure out the colour of the set code based on rarity
                    string Rarity = GetCardToRenderReader["rarity"].ToString();
                    Color FontColour = new Color();
                    if (Rarity == "common")
                    {
                        //black for common
                        FontColour = ColorTranslator.FromHtml("#000000");
                    }
                    else if (Rarity == "rare")
                    {
                        //Patriot Blue for rare
                        FontColour = ColorTranslator.FromHtml("#103A5D");
                    }
                    else if (Rarity == "super")
                    {
                        //Soviet Red for Super Rare
                        FontColour = ColorTranslator.FromHtml("#CD0000");
                    }
                    else if (Rarity == "ultima")
                    {
                        //Gold for ultima rare
                        FontColour = ColorTranslator.FromHtml("#FFD700");
                    }
                    //The set code is drawn in the colour based upon rarity in Downlink regular, maximum 16px font size in a box 150x18 pixels at 590,1025
                    SolidBrush SetCodeBrush = new SolidBrush(FontColour);
                    CardBitmap = DrawText(CardBitmap, 590, 1025, 150, 18, Setcode, 16, "Downlink", "regular", SetCodeBrush, true);
                    SetCodeBrush.Dispose();
                    //clean up the card reader
                    GetCardToRenderReader.Close();
                }                
                return CardBitmap;
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show($"An Error Occured {ex}");
                System.Windows.Application.Current.Shutdown();
                return null;
            }

        }
        public static void ExportCards(string Set, bool Cropped, bool Shrunk)
        {
            //first you have to get the directory to save the images to using opendialog because C# doesnt have a folder selecter by default
            string Filepath = "";
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                //make sure the file dialog initially opens in the same folder as the application
                fbd.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                //show a file file dialog to select a folder to save these cards to
                DialogResult result = fbd.ShowDialog();
                if(result== DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    Filepath = fbd.SelectedPath;
                }
            }
            //now the file folder is selected select the whole database of the set to use
            string ExportCardString = $"SELECT * FROM {Set}";
            SQLiteCommand ExportCardCommand = new SQLiteCommand(ExportCardString, Globals.GlobalVars.DatabaseConnection);
            SQLiteDataReader ExportCardReader = ExportCardCommand.ExecuteReader();
            //now go through each of the cards in the set selected            
            while (ExportCardReader.Read())
            {
                //make the card as a bitmap
                Bitmap card = GenerateCardImage(Set, ExportCardReader["imagestring"].ToString(), ExportCardReader["card_code"].ToString());
                //now set the subdirectory the cards will be save to
                string subdirectory = "\\Bleed";
                //now if you have to crop the image crop it down
                if(Cropped == true)
                {
                    card = card.Clone(new Rectangle(37, 37, 750, 1050), System.Drawing.Imaging.PixelFormat.DontCare);
                    //change where the card will be saved
                    subdirectory = "\\Cropped";
                }
                if(Shrunk == true)
                {
                    //if the image needs to be resized do so now
                    Rectangle rectangle = new Rectangle(0, 0, 500, 700);
                    Bitmap resizedimage = new Bitmap(500, 700);
                    //now set some drawing settings to make sure it looks good
                    using(Graphics graphics = Graphics.FromImage(resizedimage))
                    {
                        //this means the new pixels overwrite the old ones, not mix
                        graphics.CompositingMode = CompositingMode.SourceCopy;
                        //dont compress the image colours
                        graphics.CompositingQuality = CompositingQuality.HighQuality;
                        //use the best quality scaling algorithm, lower proformance but good appearance
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        //same deal but for something that needs edge smoothing
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        //if pixels have to be shifted do so at high quality to avoid ghosting
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        using (ImageAttributes wrapMode = new ImageAttributes())
                        {
                            //honestly this i dont understand, only bit of code I dont TODO read about wrap modes
                            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                            graphics.DrawImage(card, rectangle, 0, 0, card.Width, card.Height, GraphicsUnit.Pixel, wrapMode);
                        }
                        //now make the card match the smaller version
                        card.Dispose();
                        card = resizedimage;
                        //and change the subdirectory it will be saved to
                        subdirectory = "\\Vassal";
                    }
                }
                //set some card metadata just because its a good idea for things like inDesign so they know how to draw it
                card.SetResolution(300, 300);
                //now save the card to the location specified as its set name and number
                //now make the directory these will be saved in if it doesnt already exist
                //now get the set code to thus name the card with
                string FileName = ExportCardReader["card_code"].ToString();
                System.IO.Directory.CreateDirectory($"{Filepath}\\{Set}{subdirectory}");
                card.Save($"{Filepath}\\{Set}{subdirectory}\\{FileName}.png", ImageFormat.Png);                
                //Clean up after yourself
                card.Dispose();
            }
        }
        //A function to draw the text where its needed
        public static Bitmap DrawText(Bitmap Card, int CardPosX, int CardPosY, int SizX, int SizY, string Text, int FontSize, string FontFamily , string FontStyle, SolidBrush FontBrush, bool CenterJustified)
        {
            //Start with the bitmap this will be drawn on
            Bitmap Map = new Bitmap(SizX, SizY);
            //Split the text up for rendering
            string[] SplitText = Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //now using statement so it automatically disposes the graphics object once it is finished
            using(Graphics g = Graphics.FromImage(Map))
            {
                //variables needed to measure the size of the string
                int TextWidth = 0;
                int TextHeight = 0;
                //and the renderable string
                string RenderableString = "";
                //we need this loop to execute at least once 
                do
                {                    
                    //empty out the renderable string just in case it already has something in it
                    RenderableString = "";
                    //make the font we will be using accounting for bold or italics
                    Font TestFont = GenerateFont(FontFamily, FontSize, FontStyle);
                    //now loop through every word in the text, adding to the string word by word , if it is too wide add a new line 
                    foreach(string word in SplitText)
                    {
                        //check to see if the string with the new word would be too long
                        if(g.MeasureString($"{RenderableString} {word}",TestFont).Width > SizX)
                        {
                            //if it is add a newline character in then the new word
                            RenderableString += $"\n{word}";
                        }
                        else
                        {
                            //if it isnt, just tack the word on the end
                            RenderableString += $" {word}";
                        }

                    }
                    //now trim off any excess spaces from the renderable string's ends
                    RenderableString = RenderableString.Trim(' ');
                    //now we have a renderable string measure its width and height and if it is too big shrink the font and try again
                    TextWidth = (int)g.MeasureString(RenderableString, TestFont).Width;
                    TextHeight = (int)g.MeasureString(RenderableString, TestFont).Height;                    
                    if(TextWidth > SizX || TextHeight > SizY)
                    {
                        FontSize--;
                    }
                    //dispose of the font because we cannot use it outside of the loop
                    TestFont.Dispose();
                }
                while (TextWidth > SizX || TextHeight > SizY);
                //Now we have a usable string and a font size make the font we will use
                Font TextFont = GenerateFont(FontFamily, FontSize, FontStyle);
                //set some graphical settings for niceness, slows the function down but for nicer results
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                //Use AntiAlias Grid fit since i'm not usuing cleartype fonts using it will actually make everything look jagged and artifacted
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                //and some variables on where to start writing
                int PosX = 0;
                int PosY = 0;
                //now adjust the posistion of the card text if it is center justified, not needing comparison since its already a bool
                if (CenterJustified)
                {
                    //if the text is centered it needs to have its top left corner shifted to the middle then back by half its own width and heigh
                    PosX = (int)((SizX / 2) - (TextWidth / 2));
                    PosY = (int)((SizY / 2) - (TextHeight / 2));
                }
                //Now draw the text in place
                g.DrawString(RenderableString, TextFont, FontBrush, PosX, PosY);                
            }
            //now that map has been drawn as desired it needed to be drawn onto the card we were passed earlier 
            using(Graphics g = Graphics.FromImage(Card))
            {
                //draw the textblock in without mutilating it or rescaling it but cropping it if by some miracle its bigger than asked
                g.DrawImageUnscaledAndClipped(Map, new Rectangle(CardPosX, CardPosY, SizX, SizY));
            }
            //and return the now rendered card bitmap
            return Card;
        }
        public static Font GenerateFont(string FontFamily, int FontSize, string FontStyle)
        {
            //make a new font from the values it is passed
            //a color isnt needed since the font brush determines colours and patterns when the text is actually rendered
            if(FontStyle == "italic")
            {
                //for italic fonts
                Font NewFont = new Font(FontFamily, FontSize, System.Drawing.FontStyle.Italic, GraphicsUnit.Pixel);
                //return the new font
                return NewFont;
            }
            else if (FontStyle == "bold")
            {
                //for bold fonts
                Font NewFont = new Font(FontFamily, FontSize, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);
                //return the new font
                return NewFont;
            }
            else
            {
                //else it is a basic unstylised font
                Font NewFont = new Font(FontFamily, FontSize, GraphicsUnit.Pixel);
                //return the new font
                return NewFont;
            }            
        }
        public static Bitmap DrawCardBase(string ArtworkPath, Bitmap Card)
        {
            //This is to draw the artwork onto the card, resizing and or cropping it as needed to fit the required dimensions
            //using statement to ensure all the graphics resources are released when the code finishes
            using(Graphics g = Graphics.FromImage(Card))
            {
                if (!File.Exists(ArtworkPath))
                {
                    //if the file path is incorrect alert the user and point the program to the 
                    //placeholder artwork so as to avoid concerns
                    System.Windows.MessageBox.Show($"The file at {ArtworkPath} did not exist or the directory was wrong. Placeholder artwork has been substituted for this card!");
                    ArtworkPath = Directory.GetCurrentDirectory() + $"\\PlaceholderArt.png";                    
                }
                //now make a bitmap of the artwork pointed to
                Bitmap Artwork = new Bitmap(ArtworkPath);
                //now to scale the image down to a suitable size, cropping it as needs be to an 11:15 aspect ratio
                //use the smaller of the two scale ratios just to make sure the image stays within its own boundaries
                float Scale = Math.Min((Artwork.Width / 11), (Artwork.Height / 15));
                //now make a rectangle that is of the scale needed and posistioned roughly centered on the image (skews to the top left though)                
                int CroppedWidth = (int)(11 * Scale);
                int CroppedHeight = (int)(15 * Scale);                
                Rectangle CroppedRectangle = new Rectangle((int)(Artwork.Width - CroppedWidth) / 2, (int)(Artwork.Height - CroppedHeight) / 2, CroppedWidth, CroppedHeight);
                //now crop the artwork down
                Artwork = Artwork.Clone(CroppedRectangle, PixelFormat.DontCare);
                //now scale it down to the right size, the using statements are to try and keep memory leakage to a minimum
                using(Bitmap ScaledImage = new Bitmap(825, 1125))
                {
                    using(Graphics Scaler = Graphics.FromImage(ScaledImage))
                    {
                        //need to set some quality of life settings so the down scaling happens cleanly
                        Scaler.CompositingMode = CompositingMode.SourceCopy;
                        Scaler.CompositingQuality = CompositingQuality.HighQuality;
                        Scaler.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        Scaler.SmoothingMode = SmoothingMode.HighQuality;
                        Scaler.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        //now use a wrap mode so that the down scaling does go as well as possible
                        using (ImageAttributes Wrapper = new ImageAttributes())
                        {
                            //honestly this i dont understand, only bit of code I dont TODO read about wrap modes
                            Wrapper.SetWrapMode(WrapMode.TileFlipXY);
                            Scaler.DrawImage(Artwork, new Rectangle(0, 0, 825, 1125), 0, 0, Artwork.Width, Artwork.Height, GraphicsUnit.Pixel, Wrapper);
                        }
                    }
                    //now clean out Artwork so that it isnt hogging resources
                    Artwork.Dispose();
                    //and make it take a copy of the scaled image to replace what it was
                    Artwork = new Bitmap(ScaledImage);
                }
                //if the artwork is entirely too smal the graphics will upscale it automatically, and if it is the right size obviously nothing needs be done
                //now draw the artwork onto the card
                g.DrawImage(Artwork, new Rectangle(0, 0, 825, 1125));                
                //now artwork can be disposed of
                Artwork.Dispose();
                //now a bitmap for the textboxes that are drawn over the top of this artwork
                Bitmap TextBoxes = new Bitmap(Directory.GetCurrentDirectory() + "\\TextBoxes.png");
                //no special processing is needed for these so just draw the whole box in place
                g.DrawImage(TextBoxes, new Rectangle(80, 80, 665, 965));
                TextBoxes.Dispose();
            }
            //now return the now based card
            return Card;
        }
        public static Bitmap DrawKeywordIcons(Bitmap Card, String Keywords)
        {
            //for when the string for keywords is being drawn some keywords mean abilities with icons, this draws those icons in
            //now we need a graphics object to draw these icons in with
            using (Graphics g = Graphics.FromImage(Card))
            {
                //now split up the keywords sent, divided up by \\
                string[] Abilities = Keywords.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string ability in Abilities)
                {
                    string IconString = ability.Trim();
                    //now depending on what that ability icon is, draw the appropriate icon in its place, usings are just for tidiness sake
                    switch (IconString)
                    {
                        case "Range":
                            using(Bitmap Icon = new Bitmap(Directory.GetCurrentDirectory() + "\\RangeIcon.png"))
                            {
                                g.DrawImageUnscaled(Icon, new Rectangle(130, 637, 70, 70));
                            }
                            break;
                        case "Marksman":
                            using (Bitmap Icon = new Bitmap(Directory.GetCurrentDirectory() + "\\MarksmanIcon.png"))
                            {
                                g.DrawImageUnscaled(Icon, new Rectangle(229, 637, 70, 70));
                            }
                            break;
                        case "Vanguard":
                            using (Bitmap Icon = new Bitmap(Directory.GetCurrentDirectory() + "\\VanguardIcon.png"))
                            {
                                g.DrawImageUnscaled(Icon, new Rectangle(328, 637, 70, 70));
                            }
                            break;
                        case "Supporter":
                            using (Bitmap Icon = new Bitmap(Directory.GetCurrentDirectory() + "\\SupportIcon.png"))
                            {
                                g.DrawImageUnscaled(Icon, new Rectangle(427, 637, 70, 70));
                            }
                            break;
                        case "Mage":
                            using (Bitmap Icon = new Bitmap(Directory.GetCurrentDirectory() + "\\MageIcon.png"))
                            {
                                g.DrawImageUnscaled(Icon, new Rectangle(526, 637, 70, 70));
                            }
                            break;
                        case "Psychic":
                            using (Bitmap Icon = new Bitmap(Directory.GetCurrentDirectory() + "\\PsykerIcon.png"))
                            {
                                g.DrawImageUnscaled(Icon, new Rectangle(625,637,70,70));
                            }
                            break;
                        default:
                            //just in case if you feed it something it doesnt know it just ignores it
                            break;

                    }
                }
            }
            //now the icons are drawn in all thats left to do is return the card
            return Card;
        }
    }
}
