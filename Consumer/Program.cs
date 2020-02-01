using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Spire.Doc;
using Spire.Pdf;
using System;
using System.Drawing;
using System.IO;
using System.Net.Mail;
using System.Text;

namespace Consumer
{
    class Program
    {

        public static bool emailSend(string email, MemoryStream stream, string fname,string convertMethod)
        {

            Console.WriteLine("Destination Email:"+email);
            
            try
            {

               
                stream.Position = 0;
                System.Net.Mime.ContentType contentType = new System.Net.Mime.ContentType(System.Net.Mime.MediaTypeNames.Application.Pdf);

                Attachment attach = new Attachment(stream, contentType);
                attach.ContentDisposition.FileName = fname +"."+convertMethod;

              

                MailMessage mailMessage = new MailMessage();

                SmtpClient smtpClient = new SmtpClient();

                mailMessage.From = new MailAddress("your email adress");
                mailMessage.To.Add(email);
                mailMessage.Body = "Your "+fname+"."+convertMethod+" is ready.";
                mailMessage.Subject = convertMethod+" File";
                mailMessage.Attachments.Add(attach);
                smtpClient.Host = "host"; 
                smtpClient.Port = 587;
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new System.Net.NetworkCredential("your email", "your password");
                smtpClient.Send(mailMessage);
                Console.WriteLine("File has sent to the "+email);

                stream.Close();
                stream.Dispose();


                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An ERROR Occured While Sending Message \n" + ex.Message);
                throw;
                return false;


            }


        }

        static void Main(string[] args)
        {

            var success = false;
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("");//accesing rabbitmq cloud string from appsettings.json

            using (var connection = factory.CreateConnection())
            {

                using (var channel = connection.CreateModel())
                {

                    channel.ExchangeDeclare("convert", ExchangeType.Direct, true, false, null);

                    channel.QueueDeclare(queue: "file", durable: true, exclusive: false, autoDelete: false, arguments: null);
                    channel.QueueBind("file", "convert", "WordToPdf");

                    channel.BasicQos(0, 1, false);


                    var consumer = new EventingBasicConsumer(channel);

                    channel.BasicConsume("file", false, consumer);

                    consumer.Received += (model, data) =>
                    {

                        try
                        {
                            Console.WriteLine("Message Received");
                            Document document = new Document();
                            PdfDocument pdfFile = new PdfDocument();
                            
                            
                            string message = Encoding.UTF8.GetString(data.Body);
                            QueueMessage queueMessage = JsonConvert.DeserializeObject<QueueMessage>(message);

                            if (queueMessage.convertMethod == "pdf")
                            {
                                Console.WriteLine("Method: " + queueMessage.convertMethod);
                                document.LoadFromStream(new MemoryStream(queueMessage.bytedFile), Spire.Doc.FileFormat.Docx2013);

                                using (MemoryStream stream = new MemoryStream())
                                {

                                    document.SaveToStream(stream, Spire.Doc.FileFormat.PDF);

                                    success = emailSend(queueMessage.email, stream, queueMessage.fileName, queueMessage.convertMethod);

                                }
                            }

                            else if (queueMessage.convertMethod == "docx")
                            {
                                Console.WriteLine("Method: " + queueMessage.convertMethod);
                                pdfFile.LoadFromStream(new MemoryStream(queueMessage.bytedFile));

                                using (MemoryStream stream = new MemoryStream())
                                {
                                    pdfFile.SaveToStream(stream, Spire.Pdf.FileFormat.DOC);

                                    success = emailSend(queueMessage.email, stream, queueMessage.fileName, queueMessage.convertMethod);
                                }

                            }

                            else if (queueMessage.convertMethod == "jpeg")
                            {
                                Console.WriteLine("Method: " + queueMessage.convertMethod);
                                pdfFile.LoadFromStream(new MemoryStream(queueMessage.bytedFile));
                                using (MemoryStream stream = new MemoryStream())
                                { 


                                    Image img = pdfFile.SaveAsImage(0);
                                    img.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                                    success = emailSend(queueMessage.email, stream, queueMessage.fileName, queueMessage.convertMethod);




                                }
                            }
                            else if(queueMessage.convertMethod == "html")
                            {
                                Console.WriteLine("Method: " + queueMessage.convertMethod);
                                pdfFile.LoadFromStream(new MemoryStream(queueMessage.bytedFile));
                                using (MemoryStream stream = new MemoryStream())
                                {

                                    pdfFile.SaveToStream(stream, Spire.Pdf.FileFormat.HTML);

                                    success = emailSend(queueMessage.email, stream, queueMessage.fileName, queueMessage.convertMethod);





                                }



                            }





                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("An error Occured\n" + ex.Message);

                        }
                        if (success)
                        {

                            Console.WriteLine("Message has sent succesfully.");
                            channel.BasicAck(data.DeliveryTag, false);

                        }

                    };

                    Console.WriteLine("Press any key to exit..");
                    Console.ReadLine();


                    


    



                }
            }
        }
    }
}
