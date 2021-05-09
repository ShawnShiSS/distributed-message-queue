using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMQ.API.Models.Order
{
    public class Create
    {
        public Guid OrderId { get; set; }
        public string CustomerNumber { get; set; }
    }
}
