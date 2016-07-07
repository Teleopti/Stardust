using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.Win.Forecasting
{
    ///<summary>
    /// handles tab events
    ///</summary>
    public sealed class TabHandler
    {

        /// <summary>
        /// Tabs the back.
        /// document this is really smart or sumthin
        /// moves back to next enabled tab in tabcontroladv
        /// </summary>
        /// <param name="tabCtrl">The tab CTRL.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: östenp
        /// Created date: 2/19/2008
        /// </remarks>
        public static TabPageAdv TabBack(TabControlAdv tabCtrl)
        {
            foreach (TabPageAdv MyTab in tabCtrl.TabPages)
            {
                if (MyTab == tabCtrl.SelectedTab)
                {
                    int mytabindex = tabCtrl.SelectedIndex;
                    for (int i = mytabindex; i > 0; i--)
                    {
                        TabPageAdv otab = tabCtrl.TabPages[i - 1];
                        if (otab.TabEnabled)
                        {
                            return otab;
                        }
                        continue;
                    }
                }
            }//foreach  
            return tabCtrl.SelectedTab;
        }


        /// <summary>
        /// Moves forward to next enabled tab in tabcontroladv.
        /// </summary>
        /// <param name="tabCtrl">The tab CTRL.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: östenp
        /// Created date: 2/19/2008
        /// </remarks>
        public static TabPageAdv TabForward(TabControlAdv tabCtrl)
        {
            bool next = false;
            foreach (TabPageAdv MyTab in tabCtrl.TabPages)
            {
                if (next == true && MyTab.TabEnabled == true)
                {
                    return MyTab;
                }
                if (MyTab == tabCtrl.SelectedTab)
                {
                    next = true;

                }
            }//foreach
            return tabCtrl.SelectedTab;
        }


        private TabHandler()
        {
            //is this enough for fxcop??
        }
    }
}