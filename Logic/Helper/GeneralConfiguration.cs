using Logic.IHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Helper
{
  public class GeneralConfiguration : IGeneralConfiguration
    {
       public string AdminEmail { get; set; }
       public  string DeveloperEmail { get; set; }
       public string FolderPath { get; set; }
       public string Time { get; set; }
    }
}
