using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace converter.Consumer.Models
{
    public class wordToPdf
    {

        public String email { get; set; }

        public IFormFile myFile { get; set; }

        public string convertMethod { get; set; }
        
    }
}
