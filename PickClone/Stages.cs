using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PickClone
{
    interface IStage
    {
        void OnNavigateTo(Stage stage);
    }


  

    public enum Stage
    {
        Acquire,
        Setting
    }
}
