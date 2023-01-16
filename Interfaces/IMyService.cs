using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAA_Project.Interfaces
{
    public interface IMyService
    {
        event Action RefreshRequested;
        void CallRequestRefresh();
    }
}
