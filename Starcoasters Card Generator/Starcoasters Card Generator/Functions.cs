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
                //now just in case if the file path given is incorrect or the file doesnt exist just return the background
                if (!File.Exists(ArtPath))
                {
                    System.Windows.MessageBox.Show($"The file path for the image at {ArtPath} does not exist or the directory is wrong");
                    ArtPath = Directory.GetCurrentDirectory() + $"\\PlaceholderArt.png";
                }
                //Now because the card artwork may or may not be the correct size manipulate it just a little bit so it fits the 600 x 900 size needed with cropping
                Bitmap CardArtwork = new Bitmap(ArtPath);
                if(CardArtwork.Width> 600 && CardArtwork.Height> 900)
                {
                    //if the card is too big in both dimension take out the middle 600 x 900 pixels
                    //get the middle pixel of the card as an interger (since odd numbered pixels will produce a .5 at the end)
                    float CardMidWidth = CardArtwork.Width / 2;
                    int CardMidWidthInt = (int)Math.Truncate(CardMidWidth);
                    //Get the middle point for the height as an int
                    float CardMidHeight = CardArtwork.Height / 2;
                    int CardMidHeightInt = (int)Math.Truncate(CardMidHeight);
                    //Now crop the image down so its 600 x 900. Because it will skew to the top left because of the above math you only subtract 299 and 449 for the top left corner
                    CardArtwork = CardArtwork.Clone(new Rectangle(CardMidWidthInt - 299, CardMidHeightInt - 449, 600, 900), System.Drawing.Imaging.PixelFormat.DontCare);                    
                }
                else if(CardArtwork.Width > 600 && CardArtwork.Height <= 900)
                {
                    // if the image is too wide but close enough tall just clone the image to itself but shrink its width down but leave its height untouched
                    //get the middle width pizel of the card as an interger (since odd numbered pixels will produce a .5 at the end)
                    float CardMidWidth = CardArtwork.Width / 2;
                    int CardMidWidthInt = (int)Math.Truncate(CardMidWidth);
                    int ArtHeight = CardArtwork.Height;
                    //now overwrite the image with the section cropped to fit from the center (not from the top left like the built in clipping does
                    CardArtwork = CardArtwork.Clone(new Rectangle(CardMidWidthInt - 299, 0, 600, ArtHeight), System.Drawing.Imaging.PixelFormat.DontCare);
                }
                else if(CardArtwork.Height >900 && CardArtwork.Width <= 600)
                {
                    // If the image is taller than needed but close enough wide do the same as above but to the height instead of width
                    //Get the middle point for the height as an int
                    float CardMidHeight = CardArtwork.Height / 2;
                    int CardMidHeightInt = (int)Math.Truncate(CardMidHeight);
                    //and get the width becuase the clone method is a baby and cant do the math for you
                    int ArtworkWidth = CardArtwork.Width;
                    //now overwrite the image with an adjusted version
                    CardArtwork = CardArtwork.Clone(new Rectangle(0, CardMidHeightInt - 449, ArtworkWidth, 900), System.Drawing.Imaging.PixelFormat.DontCare);
                }
                //If the image is smaller than 600x900 drawing it unscaled and clipped wont do anything and it will stretch itself to fit the rectangle
                //and if the image is 600x900 well no adjustments need be made
                using (Graphics graphics = Graphics.FromImage(CardBitmap))
                {
                    //Actually drawing the image over the prior background
                    graphics.DrawImageUnscaledAndClipped(CardArtwork, new Rectangle(112, 112, 600, 900));
                }
                //Now draw the textboxes over the top of that
                //The graphics class understands transparency thankfully so now we can do the same as above but for the textboxes
                Bitmap CardBoxBitmap = new Bitmap(System.IO.Directory.GetCurrentDirectory() + "\\CardTextBoxes.png");
                using (Graphics graphics = Graphics.FromImage(CardBitmap))
                {
                    graphics.DrawImage(CardBoxBitmap, new Rectangle(117, 117, 590, 920));
                }
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
                    //And the font we will be using                    
                    Font CostFont = new Font("Downlink", FontSize, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);
                    //now to draw the text until it fits into a 79x79 box
                    while (TextWidth >79 || TextHeight > 79)
                    {
                        //make a bitmap of the exact maximum size the text can be
                        Bitmap TextCostMap = new Bitmap(79, 79);
                        //Now make it able to be drawn on
                        Graphics g = Graphics.FromImage(TextCostMap);
                        //Now change the font, of font size of Classic Robot Condensed Bold at the FontSize
                        CostFont = new Font("Downlink", FontSize, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);
                        //now on g testdraw the cost, setting TextWidth and TextHeight as whatever the texts width and height would be in the name font
                        SizeF CostSize = g.MeasureString(CardCostString, CostFont);
                        //Now set TextWidth and TextHeight to the texts height and width
                        TextWidth = (int)CostSize.Width;
                        TextHeight = (int)CostSize.Height;
                        //now if the text would exceed the necessary constraints in either dimension shrink the font size 
                        if(TextHeight > 79 || TextWidth > 79)
                        {
                            FontSize--;
                        }
                    }
                    //now we have a usable font size draw it into posistion centered in the cost textbox at the size                    
                    using(Graphics graphics = Graphics.FromImage(CardBitmap))
                    {
                        //first set some niceties to make the font look nice
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;   
                        //Now Draw
                        graphics.DrawString(CardCostString, CostFont, Brushes.Black, (int)170 - TextWidth / 2, (int)166 - TextHeight / 2);
                    }
                    //Now do the same for the cards primary name
                    string CardNamePrimaryString = GetCardToRenderReader["name_primary"].ToString();
                    FontSize = 72;
                    TextWidth = 10000;
                    TextHeight = 10000;
                    Font PrimaryNameFont = new Font("Classic Robot Condensed", FontSize, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);
                    //Now test what size the Name can be drawn is like done for cost above
                    while (TextWidth > 448 || TextHeight > 72)
                    {
                        //as before make a bitmap that can be drawn all over to test
                        Bitmap PrimaryNameMap = new Bitmap(448, 72);
                        //now make it drawable
                        Graphics g = Graphics.FromImage(PrimaryNameMap);
                        //now make the font match the current font size
                        PrimaryNameFont = new Font("Downlink", FontSize, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);
                        SizeF PrimaryNameSize = g.MeasureString(CardNamePrimaryString, PrimaryNameFont);
                        //now set textwidth and textheight to the width and height of the drawn text
                        TextWidth = (int)PrimaryNameSize.Width;
                        TextHeight = (int)PrimaryNameSize.Height;
                        //and if its too big shrink the font size and try again
                        if(TextWidth>448 || TextHeight > 72)
                        {
                            FontSize--;
                        }
                    }
                    //now draw the name onto the actual image
                    using(Graphics graphics = Graphics.FromImage(CardBitmap))
                    {
                        //first set some niceties to make the font look nice
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                        //Now Draw
                        graphics.DrawString(CardNamePrimaryString, PrimaryNameFont, Brushes.Black, (int)463 - TextWidth / 2, (int)150 - TextHeight / 2);
                    }
                    //Now ditto for the secondary name
                    string CardSecondaryNameString = GetCardToRenderReader["name_secondary"].ToString();
                    FontSize = 36;
                    TextWidth = 10000;
                    TextHeight = 10000;
                    Font SecondaryNameFont = new Font("Classic Robot Condensed", FontSize, GraphicsUnit.Pixel);
                    //as above find out what font size the secondary name can be drawn as
                    while(TextWidth>448 || TextHeight > 36)
                    {
                        //Bitmap to drawn all over
                        Bitmap SecondaryNameMap = new Bitmap(448, 36);
                        //make it so you can draw all over it
                        Graphics g = Graphics.FromImage(SecondaryNameMap);
                        //make the font size match the font size
                        SecondaryNameFont = new Font("Classic Robot Condensed", FontSize, GraphicsUnit.Pixel);
                        //see how big it would be
                        SizeF SecondaryNameSize = g.MeasureString(CardSecondaryNameString, SecondaryNameFont);
                        //make textwidth and textheight match the texts size
                        TextWidth = (int)SecondaryNameSize.Width;
                        TextHeight = (int)SecondaryNameSize.Height;
                        //and if it is too big shrink it down a size and try again
                        if (TextWidth > 448 || TextHeight > 36)
                        {
                            FontSize--;
                        }
                    }
                    //Draw the secondary name in
                    using(Graphics graphics = Graphics.FromImage(CardBitmap))
                    {
                        //first set some niceties to make the font look nice
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                        //Now Draw
                        graphics.DrawString(CardSecondaryNameString, SecondaryNameFont, Brushes.Black, (int)463 - TextWidth / 2, (int)199 - TextHeight / 2);
                    }
                    //Now for keywords
                    string KeywordsString = GetCardToRenderReader["keywords"].ToString();
                    Font KeywordsFont = new Font("Classic Robot Condensed", FontSize, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);
                    FontSize = 48;
                    TextWidth = 10000;
                    TextHeight = 10000;
                    //now just split up the keywords, this will get rid of any placeholders
                    string[] KeywordStringArray = KeywordsString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    KeywordsString = "";
                    int keywordindex = 1;
                    //now make the string with each of the keywords that arent place holder
                    foreach(string keyword in KeywordStringArray)
                    {
                        if (!keyword.Contains("placeholder"))
                        {
                            if (keywordindex == 1 || keyword == " ")
                            {
                                KeywordsString += keyword.Trim();
                            }
                            else
                            {
                                KeywordsString += ", " + keyword.Trim();
                            }
                        }
                        keywordindex++;
                    }
                    while (TextWidth > 580 || TextHeight > 50)
                    {
                        //Bitmap to draw on
                        Bitmap KeywordsBitmap = new Bitmap(580, 50);
                        Graphics g = Graphics.FromImage(KeywordsBitmap);
                        //make the font match
                        KeywordsFont = new Font("Classic Robot Condensed", FontSize, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);
                        SizeF KeywordSize = g.MeasureString(KeywordsString, KeywordsFont);
                        TextWidth = (int)KeywordSize.Width;
                        TextHeight = (int)KeywordSize.Height;
                        //if its too big shrink the font size and roll again
                        if (TextWidth > 580 || TextHeight > 50)
                        {
                            FontSize--;
                        }
                    }
                    //Now draw the keywords in
                    using(Graphics graphics = Graphics.FromImage(CardBitmap))
                    {
                        //first set some niceties to make the font look nice
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                        //Now Draw
                        graphics.DrawString(KeywordsString, KeywordsFont, Brushes.Black, (int)414 - TextWidth / 2, (int)678 - TextHeight / 2);
                    }
                    //Now for HP,ATK and DEF
                    string CardHP = GetCardToRenderReader["hp"].ToString();
                    string CardATK = GetCardToRenderReader["atk"].ToString();
                    string CardDEF = GetCardToRenderReader["def"].ToString();
                    FontSize = 60;
                    TextWidth = 10000;
                    TextHeight = 10000;
                    //since all the stats are written in the same font only one needs to be made
                    Font StatFont = new Font("Downlink", FontSize, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);
                    while (TextWidth > 90 || TextHeight > 90)
                    {
                        //Bitmap to draw on
                        Bitmap StatBitmap = new Bitmap(90, 90);
                        Graphics g = Graphics.FromImage(StatBitmap);
                        StatFont = new Font("Downlink", FontSize, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);
                        SizeF StatSize = g.MeasureString(CardHP, StatFont);
                        TextWidth = (int)StatSize.Width;
                        TextHeight = (int)StatSize.Height;
                        if (TextWidth > 90 || TextHeight > 90)
                        {
                            FontSize--;
                        }
                    }
                    //now draw HP
                    using(Graphics graphics = Graphics.FromImage(CardBitmap))
                    {
                        //first set some niceties to make the font look nice
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                        //Now Draw
                        graphics.DrawString(CardHP, StatFont, Brushes.Black, (int)167 - TextWidth / 2, (int)759 - TextHeight / 2);
                    }
                    //ATK
                    FontSize = 60;
                    TextWidth = 10000;
                    TextHeight = 10000;
                    while (TextWidth > 90 || TextHeight > 90)
                    {
                        //Bitmap to draw on
                        Bitmap StatBitmap = new Bitmap(90, 90);
                        Graphics g = Graphics.FromImage(StatBitmap);
                        StatFont = new Font("Downlink", FontSize, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);
                        SizeF StatSize = g.MeasureString(CardATK, StatFont);
                        TextWidth = (int)StatSize.Width;
                        TextHeight = (int)StatSize.Height;
                        if (TextWidth > 90 || TextHeight > 90)
                        {
                            FontSize--;
                        }
                    }
                    //now draw ATK
                    using (Graphics graphics = Graphics.FromImage(CardBitmap))
                    {
                        //first set some niceties to make the font look nice
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                        //Now Draw
                        graphics.DrawString(CardATK, StatFont, Brushes.Black, (int)167 - TextWidth / 2, (int)859 - TextHeight / 2);
                    }
                    //DEF
                    FontSize = 60;
                    TextWidth = 10000;
                    TextHeight = 10000;
                    while (TextWidth > 90 || TextHeight > 90)
                    {
                        //Bitmap to draw on
                        Bitmap StatBitmap = new Bitmap(90, 90);
                        Graphics g = Graphics.FromImage(StatBitmap);
                        StatFont = new Font("Downlink", FontSize, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);
                        SizeF StatSize = g.MeasureString(CardDEF, StatFont);
                        TextWidth = (int)StatSize.Width;
                        TextHeight = (int)StatSize.Height;
                        if (TextWidth > 90 || TextHeight > 90)
                        {
                            FontSize--;
                        }
                    }
                    //now draw DEF
                    using (Graphics graphics = Graphics.FromImage(CardBitmap))
                    {
                        //first set some niceties to make the font look nice
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                        //Now Draw
                        graphics.DrawString(CardDEF, StatFont, Brushes.Black, (int)167 - TextWidth / 2, (int)959 - TextHeight / 2);
                    }
                    //Onceagain abilities are assholes and need lots of loops and special code to be drawn
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
                    string[] AbilityArray = WholeAbilityString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    AbilityCount = AbilityArray.Length;
                    //Because the abilities are evenly spaced out, and with the same X value, the Y value they are drawn at is 714 + the amount of pixels each ability gets times
                    //how many spaces away it is from the first ability at y=714
                    //That number away from the top is what this variable is for
                    int AbilityNumber = 0;
                    foreach(string Ability in AbilityArray)
                    {                        
                        AbilityNameFontSize = 17;
                        AbilityCostFontSize = 16;
                        FontSize = 16;
                        string[] AbilitySplit = Ability.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        //make the bitmap and graphics
                        Bitmap AbilityMap = new Bitmap(479, (int)250 / AbilityCount);
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
                            //Now draw in the ability title
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
                                if(g.MeasureString($"{RenderableCostString}{word} ",AbilityCostFont).Width <= 479)
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
                                if (g.MeasureString($"{RenderableEffectString}{word} ", AbilityCostFont).Width <= 479)
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
                            TextWidth = Math.Max(TextWidth, (int)g.MeasureString(RenderableEffectString, AbilityBodyFont).Height);
                            //if its too big shrink the fontsizes and try again
                            if (TextWidth > 479 || TextHeight > 250 / AbilityCount)
                            {
                                AbilityNameFontSize--;
                                AbilityCostFontSize--;
                                FontSize--;
                            }
                        }
                        while (TextWidth > 479 || TextHeight > 250 / AbilityCount);
                        //now we have an acceptable sized set of text to draw in a nice drawable bitmap draw it in on the card
                        using(Graphics graphics = Graphics.FromImage(CardBitmap))
                        {
                            //because mathimatical order of operations is weird
                            int AbilityHeight = 250 / AbilityCount;
                            AbilityHeight = AbilityHeight * AbilityNumber;
                            AbilityHeight += 714;
                            graphics.DrawImageUnscaledAndClipped(AbilityMap, new Rectangle(223, AbilityHeight, 479, TextHeight));
                        }
                        //now increment the ability number
                        AbilityNumber++;
                    }
                    //Now for the flavourtext like the card ability effects is split up and drawn one word at a time, or at least
                    //converted one word at a time to a drawable string
                    FontSize = 18;
                    string FlavourString = GetCardToRenderReader["flavour"].ToString();
                    string RenderableFlavourString = "";
                    Font FlavourFont = new Font("Classic Robot Condensed", FontSize, System.Drawing.FontStyle.Italic, GraphicsUnit.Pixel);
                    //both make the string that can be drawn and what size it can be drawn at
                    do
                    {
                        //Reset the renderable flavour string
                        RenderableFlavourString = "";
                        Bitmap FlavourMap = new Bitmap(479, 46);
                        Graphics g = Graphics.FromImage(FlavourMap);
                        //update the font
                        FlavourFont = new Font("Classic Robot Condensed", FontSize, System.Drawing.FontStyle.Italic, GraphicsUnit.Pixel);
                        //split the flavourtext up based on spaces
                        string[] SplitFlavour = FlavourString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        //now for each word in the flavour text
                        foreach(string word in SplitFlavour)
                        {
                            if (g.MeasureString($"{RenderableFlavourString} {word}", FlavourFont).Width <= 479)
                            {
                                //if the new word on the string would fit just add it onto the string
                                RenderableFlavourString += $" {word}";
                            }
                            else
                            {
                                //if it wont make a new line and then add it on
                                string Newline = System.Environment.NewLine;
                                RenderableFlavourString += $"{Newline}{word}";
                            }
                        }
                        //now check to see if the rendered text fits into its allotted space if it doesnt shrink the font size and try again
                        TextWidth = (int)g.MeasureString(RenderableFlavourString, FlavourFont).Width;
                        TextHeight = (int)g.MeasureString(RenderableFlavourString, FlavourFont).Height;
                        if (TextWidth>479|| TextHeight  > 46)
                        {
                            FontSize--;
                        }
                        
                    }
                    while (TextWidth > 479 || TextHeight > 46);
                    //now we have a usable size for the text draw it into place
                    using(Graphics graphics = Graphics.FromImage(CardBitmap))
                    {
                        graphics.DrawString(RenderableFlavourString, FlavourFont, new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(255, 31, 31, 31)), (int)223, (int)1009 - TextHeight);
                    }
                    //Lastly for the setcode
                    string Setcode = GetCardToRenderReader["card_code"].ToString();
                    FontSize = 14;
                    Font SetCodeFont = new Font("Downlink", FontSize, GraphicsUnit.Pixel);
                    do
                    {
                        Bitmap SetCodeMap = new Bitmap(190, 18);
                        Graphics g = Graphics.FromImage(SetCodeMap);
                        //update the font
                        SetCodeFont = new Font("Downlink", FontSize, GraphicsUnit.Pixel);
                        SizeF SetCodeSize = g.MeasureString(Setcode, SetCodeFont);
                        //get the height and width of the text
                        TextHeight = (int)SetCodeSize.Height;
                        TextWidth = (int)SetCodeSize.Width;
                        //if its to big shrink the font size and try again
                        if (TextHeight > 18 || TextWidth > 190)
                        {
                            FontSize--;
                        }
                    }
                    while (TextHeight>18||TextWidth>190);
                    //now draw the setcode in place
                    using(Graphics graphics = Graphics.FromImage(CardBitmap))
                    {
                        //first set some niceties to make the font look nice
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                        //Now Draw
                        graphics.DrawString(Setcode, SetCodeFont, Brushes.Black, (int)632 - TextWidth / 2, (int)1027 - TextHeight / 2);
                    }
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
            int i = 1;
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
                        //use the best quality scaling algorithm, lower proformance but good
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        //same deal but for something
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
    }
}
