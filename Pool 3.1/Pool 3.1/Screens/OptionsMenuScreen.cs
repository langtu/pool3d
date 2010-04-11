using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XNA_PoolGame.Screens
{
    public class OptionsMenuScreen : MenuScreen
    {
        MenuEntry volumenMenuEntry;
        MenuEntry backMenuEntry;
        public OptionsMenuScreen() 
            : base("Options")
        {
            volumenMenuEntry = new MenuEntry("Volume");
            backMenuEntry = new MenuEntry("Back");

            //Volumen.Selected += Volumen
            backMenuEntry.Selected += OnCancel;

            MenuEntries.Add(volumenMenuEntry);
            MenuEntries.Add(backMenuEntry);
        }
    }
}
