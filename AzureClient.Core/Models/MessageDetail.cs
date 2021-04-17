using AzureClient.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureClient.Core.Models
{
    public class MessageDetail : IMessageDetail
    {
        public string Id { get; set; }
        public string Receipt { get; set; }
    }
}
