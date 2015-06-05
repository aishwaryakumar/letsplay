using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hackday
{
    public interface DataListener
    {
        void OnDataFromMaster(string data)
        void OnDataFromSlaves(string data)
    }
}
