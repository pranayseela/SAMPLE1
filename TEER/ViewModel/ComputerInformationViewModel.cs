using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEER.ViewModel
{
    public class ComputerInformationViewModel : BaseUIViewModel
    {
        public string ipAddress { get; set; }
        public string computerName { get; set; }
        public string macAddress { get; set; }

    }
}