using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Starcoasters_Card_Generator
{
    class Classes
    {
        //This is the template for an ability 
        public class Ability
        {
            public string AbilityName { get; set; }
            public string AbilityTrigger { get; set; }
            public string AbilityEffect { get; set; }
        }
        //These are classes I will use for different objects in this program
        public class SetOverview
        {
            //This Is Used On The Main Window To Display Information Regarding A Given Sets Name Code And Number Of Cards
            public string SetName { get; set; }
            public string SetCode { get; set; }
            public int SetCount { get; set; }
        }
        
        public class CardOverview
        {
            //This is what is used to show an overview of cards on the SetList window
            //The cards two names
            public string CardName { get; set; }
            public string CardNameSecondary { get; set; }
            //Card cost and stats
            public string CardCost { get; set; }
            public int CardHP { get; set; }
            public int CardATK { get; set; }
            public int CardDEF { get; set; }
            //The number of abilities this card has
            public int CardAbilityCount { get; set; }
            //The Cards Species
            public string CardSpecies { get; set; }
            //The cards set code
            public string CardSetCode { get; set; }
        }
        public class CardDetail
        {
            // This One Will Hold The Details Of A Card While It Is Being Modified Or Saved To The Database
            //The Cards Names
            public string CardNamePrimary { get; set; }
            public string CardNameSecondary { get; set; }
            //The Card Stats
            public string CardCost { get; set; }
            public int CardHP { get; set; }
            public int CardATK { get; set; }
            public int CardDEF { get; set; }
            //The Array Of card keywords
            public string[] CardKeywords { get; set; }
            //The Array of Abilities
            public Classes.Ability[] CardAbilities { get; set; }
            //Flavour Text
            public string CardFlavour { get; set; }
            //Cards set number
            public string CardCode { get; set; }
            //The string for the image path
            public string ImagePath { get; set; }
        }
    }
}
