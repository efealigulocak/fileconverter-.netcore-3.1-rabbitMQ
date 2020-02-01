using System;
using System.Collections.Generic;
using System.Text;

namespace Consumer
{
    class QueueMessage
    {

        public Byte[] bytedFile { get; set; }

        public string email { get; set; }

        public string fileName { get; set; }

        public string convertMethod { get; set; }
    }
}
