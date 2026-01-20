using Microsoft.Data.Sqlite;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;


namespace vyaauma.Services
{
    public interface ISqliteFormService
    {
        void SaveFormData(
            string firstName,
            string lastName,
            string email,
            string contactNumber,
            string message,
            string appylIn,
            string formInfoType
        );

        void SendBrevoTemplateEmail(
           string firstName, string lastName, string email, string formInfoType
        );
        void SendEmail(string mailBody);
    }

    public class SqliteFormService : ISqliteFormService
    {
        private readonly string _connectionString;
        private readonly IPublishedContentQuery _contentQuery;

        public SqliteFormService(
        IConfiguration config,
        IPublishedContentQuery contentQuery,
        IUmbracoContextAccessor umbracoContextAccessor)
        {
            _connectionString = config.GetConnectionString("umbracoDbDSN");
            _contentQuery = contentQuery;
        }

        public void SaveFormData(string firstName, string lastName, string email, string contactNumber, string message, string appylIn, string formInfoType)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = @"
                INSERT INTO ContactUsForm 
                (FirstName, LastName, Email, ContactNumber, ApplyIn, Message, SubmittedAt, FormInfoType)
                VALUES 
                ($firstName, $lastName, $email, $contactNumber, $appylIn, $message, $submittedAt, $formInfoType)
            ";

            command.Parameters.AddWithValue("$firstName", firstName);
            command.Parameters.AddWithValue("$lastName", lastName);
            command.Parameters.AddWithValue("$email", email);
            command.Parameters.AddWithValue("$contactNumber", contactNumber);
            command.Parameters.AddWithValue("$appylIn", appylIn ?? string.Empty);
            command.Parameters.AddWithValue("$message", message);
            command.Parameters.AddWithValue("$submittedAt", DateTime.UtcNow);
            command.Parameters.AddWithValue("$formInfoType", formInfoType ?? string.Empty);

            command.ExecuteNonQuery();
        }

        private IPublishedContent? GetEmailSettings()
        {
            return _contentQuery
        .ContentAtRoot()
        .FirstOrDefault(x => x.ContentType.Alias == "smtpEmailCredentials");
        }


        public void SendBrevoTemplateEmail(
        string firstName,
        string lastName,
        string email, string formInfoType
     )
        {
            var settings = GetEmailSettings();
            if (settings == null) return;

            var apiKey = settings.Value<string>("smtpAPIKey");
            var userReciverEmail = email;
            var senderEmail =  settings.Value<string>("senderEmail");
            var fromName = settings.Value<string>("fromName");
            var fullName = firstName + lastName;
            var templateId = settings.Value<string>("tempId");

            Dictionary<string, object> templateParams = new Dictionary<string, object>
               {
                   { "customer", fullName }
               };



            var payload = new
            {
                to = new[]
                {
            new { email = userReciverEmail, name = fullName }
        },
                sender = new
                {
                    email = senderEmail,
                    name = fromName
                },
                templateId = templateId,
                @params = templateParams,
                replyTo = new
                {
                    email = email,
                    name = fullName
                }
            };

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("api-key", apiKey);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = client.PostAsync(
                "https://api.brevo.com/v3/smtp/email",
                content
            ).Result;

            if (!response.IsSuccessStatusCode)
            {
                var error = response.Content.ReadAsStringAsync();

                // Log error (choose one)
                Console.WriteLine("Brevo email failed: " + error);
                // _logger.LogError("Brevo email failed: {Error}", error);

                return;
            }
        }

        public void SendEmail(string mailBody)
        {
            var settings = GetEmailSettings();
            if (settings == null)
                return;

            //  FETCH DATA FROM ADMIN HERE
            var host = settings.Value<string>("smtpHost");
            var port = settings.Value<int>("smtpPort");
            var enableSsl = settings.Value<bool>("enableSsl");
            var userName = settings.Value<string>("smtpUsername");
            var password = settings.Value<string>("smtpKey");

            var senderEmail = settings.Value<string>("senderEmail");

            var adminToEmail =  settings.Value<string>("senderEmail");

            var fromName = settings.Value<string>("fromName");
            var subject = settings.Value<string>("emailSubject");

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(userName, password),
                EnableSsl = enableSsl
            };

            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail, fromName),
                Subject = subject,
                Body = mailBody,
                IsBodyHtml = true
            };

            mail.To.Add(adminToEmail);

            client.Send(mail);
        }
    }
}
