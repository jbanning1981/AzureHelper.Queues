using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBanning.AzureHelper.Queues
{
    public enum QueueMessageSerializer
    {
        SystemTextJson = 0,
        Newtonsoft = 1,
        External = 2
    }
}
