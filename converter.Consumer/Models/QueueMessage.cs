using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace converter.Consumer.Models
{
    public class QueueMessage
    {
        public Byte[] bytedFile { get; set; }

        public string email { get; set; }

        public string fileName { get; set; }

        public string convertMethod { get; set; }

    }
}
