using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Extensions;
using vyaauma.Services;
namespace vyaauma.Controllers
{
    public class FormController : SurfaceController
    {
        private readonly IPublishedContentQuery _contentQuery;
        private readonly ISqliteFormService _formService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FormController> _logger;
        public FormController(
            ISqliteFormService formService,
            IConfiguration configuration,
            IPublishedContentQuery contentQuery,
              ILogger<FormController> logger,
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext serviceContext,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider)
            : base(umbracoContextAccessor, databaseFactory, serviceContext, appCaches, profilingLogger, publishedUrlProvider)
        {
            _formService = formService;
            _contentQuery = contentQuery;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult SubmitForm(string firstName, string lastName, string email, string contactNumber, string message, string appylIn, string formInfoType)
        {

            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(contactNumber) ||
                string.IsNullOrWhiteSpace(message))
            {
                return Json(new
                {
                    success = false,
                    message = "Fill out all required fields."
                });
            }
            if (!Regex.IsMatch(contactNumber, @"^\d{10}$"))
            {
                return Json(new { success = false, message = "Invalid phone number" });
            }


            var settings = GetEmailSettings();
            if (settings == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Email settings are not configured."
                });
            }

            string formInfo = string.Empty;
            string url = string.Empty;


            if (formInfoType == "ContactUs")
            {
                formInfo = "contactUsInfo";
                var thankYouLink = settings.Value<IEnumerable<Link>>("thankYouPageUrl");
                url = thankYouLink?.FirstOrDefault()?.Url;
            }
            else if (formInfoType == "Career")
            {
                formInfo = "careerInfo";
                var thankYouLink = settings.Value<IEnumerable<Link>>("thankYouPageUrl");
                url = thankYouLink?.FirstOrDefault()?.Url;
            }

            _formService.SaveFormData(firstName, lastName, email, contactNumber, message, appylIn, formInfoType);

            _formService.SendBrevoTemplateEmail(firstName, lastName, email, formInfoType);

            var fullName = firstName + lastName;

            var template = _configuration["Umbraco:CMS:EmailTemplates:ContactInquiry"];

            template = ReplaceOrRemove(template, "FormInfo", "Form Info", formInfo);
            template = ReplaceOrRemove(template, "FullName", "Full Name", fullName);
            template = ReplaceOrRemove(template, "ApplyIn", "Interested In", appylIn);
            template = ReplaceOrRemove(template, "Email", "Email", email);
            template = ReplaceOrRemove(template, "ContactNumber", "Contact Number", contactNumber);
            template = ReplaceOrRemove(template, "Message", "Message", message);

            // Send email
            _formService.SendEmail(template);


            return Json(new
            {
                success = true,
                redirectUrl = url
            });

        }


        private IPublishedContent? GetEmailSettings()
        {
            if (!UmbracoContextAccessor.TryGetUmbracoContext(out var context))
                return null;

            return _contentQuery
          .ContentAtRoot()
          .FirstOrDefault(x => x.ContentType.Alias == "smtpEmailCredentials");
        }

        private string ReplaceOrRemove(string template, string placeholder, string label, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                // Remove the entire <p> block if value is empty
                return Regex.Replace(
                    template,
                    $"<p><strong>{label}:</strong>\\s*{{{{{placeholder}}}}}</p>",
                    string.Empty,
                    RegexOptions.IgnoreCase
                );
            }

            return template.Replace($"{{{{{placeholder}}}}}", value);
        }

    }
}
