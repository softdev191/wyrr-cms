using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PENPAL.DataStore.StellarModel
{
    public class StellarAPIErrorModel
    {
        public class Results
        {
            public string Error { get; set; }
        }

        public class ErrorObject
        {
            public List<Results> Result { get; set; }
        }


    }
}
