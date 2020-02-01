using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using converter.Consumer.Models;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace converter.Consumer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            this.configuration = configuration;

        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult wordToPdf()
        {

            return View();


        }

        [HttpPost]
        public IActionResult wordToPdf(wordToPdf wtp)
        {

            try
            {
                var factory = new ConnectionFactory();
                factory.Uri = new Uri(configuration["ConnectionStrings:your connection string"]);//accesing rabbitmq cloud string from appsettings.json

                using (var connection = factory.CreateConnection())
                {

                    using (var channel = connection.CreateModel())
                    {

                        channel.ExchangeDeclare("convert", ExchangeType.Direct, true, false, null);

                        channel.QueueDeclare(queue: "file", durable: true, exclusive: false, autoDelete: false, arguments: null);

                        channel.QueueBind("file", "convert", "WordToPdf");

                        QueueMessage queueMessage = new QueueMessage();

                        using (MemoryStream stream = new MemoryStream())
                        {

                            wtp.myFile.CopyTo(stream);
                            queueMessage.bytedFile = stream.ToArray();




                        }
                        queueMessage.email = wtp.email;
                        queueMessage.fileName = Path.GetFileNameWithoutExtension(wtp.myFile.FileName);
                        queueMessage.convertMethod = wtp.convertMethod;

                        string message = JsonConvert.SerializeObject(queueMessage);

                        byte[] bytedMessage = Encoding.UTF8.GetBytes(message);


                        var property = channel.CreateBasicProperties();
                        property.Persistent = true;

                        channel.BasicPublish("convert", "WordToPdf", property, bytedMessage);

                        ViewBag.result = "Converting went successfull. Check your email";


                        return View();
                    }


                }
            }

            catch (Exception ex)
            {

                return View("Error Happened while converting");


            }



        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
