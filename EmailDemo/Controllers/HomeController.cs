using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace EmailDemo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(Models.Message model, List<HttpPostedFileBase> files)
        {
            string EmailId = ConfigurationManager.AppSettings["EmailId"];
            string EmailPassword = ConfigurationManager.AppSettings["EmailPassword"];
            try
            {
                using (MailMessage mail = new MailMessage(EmailId, model.To))
                {
                    mail.Subject = model.Subject;
                    mail.Body = PopulateBody(model.Body);
                    foreach (HttpPostedFileBase file in files)
                    {
                        if (file != null)
                        {
                            string fileName = Path.GetFileName(file.FileName);
                            mail.Attachments.Add(new Attachment(file.InputStream, fileName));
                        }
                    }
                    mail.IsBodyHtml = true;
                    SmtpClient smtp = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        EnableSsl = true,
                        Port = 587
                    };
                    NetworkCredential networkCredential = new NetworkCredential(EmailId, EmailPassword);
                    smtp.UseDefaultCredentials = true;
                    smtp.Credentials = networkCredential;
                    smtp.Port = 587;
                    smtp.Send(mail);
                }
            }
            catch (Exception ex)
            {
                MailLog(ex);
            }
            return View();
        }

        private string PopulateBody(string message)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Template/EmailPage.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{message}", message);
            return body;
        }
        public static void MailLog(Exception ex)
        {
            string lines = "Error occured at " + DateTime.Now.ToString("MM/dd/yyyy h:mm tt") + "\r\n";
            lines += "****************************************************************** \r\n";
            lines += "StackTrace: " + ex.StackTrace?.ToString() + " \r\n";
            lines += "ErrorMessage: " + ex.Message?.ToString() + " \r\n";
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Mail Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "\\Mail Logs\\" + "MailLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + "_" + DateTime.Now.Ticks.ToString() + ".txt";
            if (!System.IO.File.Exists(filePath))
            {
                using (StreamWriter sw = System.IO.File.CreateText(filePath))
                {
                    sw.WriteLine(lines);
                }
            }
            else
            {
                using (StreamWriter sw = System.IO.File.AppendText(filePath))
                {
                    sw.WriteLine(lines);
                }
            }
        }
    }
}