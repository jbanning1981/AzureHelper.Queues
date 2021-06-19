using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureClient.Core
{
    public enum QueueMessageSerializer
    {
        SystemTextJson = 0,
        Newtonsoft = 1,
        External = 2
    }
}
