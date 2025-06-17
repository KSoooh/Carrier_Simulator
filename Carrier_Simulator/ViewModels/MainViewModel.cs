using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using log4net;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carrier_Simulator.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        static readonly ILog Logger =
           LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MainViewModel()
        {
            Logger.Info("Carrier Simulator Start");
        }
    }
}
