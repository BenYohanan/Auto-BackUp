using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.IHelper
{
   public interface IGeneralConfiguration
    {
        string AdminEmail { get; set; }
        string DeveloperEmail { get; set; }
        string FolderPath { get; set; }
        string Time { get; set; }
    }
}
