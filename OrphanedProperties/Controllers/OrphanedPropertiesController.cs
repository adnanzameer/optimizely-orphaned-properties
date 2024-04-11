using OrphanedProperties.Helpers;
using OrphanedProperties.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using OrphanedProperties.Models;
using System.Collections.Generic;

namespace OrphanedProperties.Controllers
{
    [Authorize(Policy = Constants.PolicyName)]
    [Route("[controller]")]
    public class OrphanedPropertiesController : Controller
    {
        private readonly IOrphanedPropertiesService
            _missingPropertiesService;

        public OrphanedPropertiesController(IOrphanedPropertiesService missingPropertiesService)
        {
            _missingPropertiesService = missingPropertiesService;
        }

        [Route("[action]")]
        public IActionResult Index(int? page)
        {
            var model = new OrphanedPropertiesViewModels
            {
                OrphanedProperties = Enumerable.Empty<OrphanedPropertyResult>(),
                QueryString = HttpContext.Request.QueryString.ToString(),
                PageNumber = page != null && page.Value != 0 ? page.Value : 1
            };

            var orphanedPropertyResults = _missingPropertiesService.GetMissingProperties();
            model.TotalItemsCount = orphanedPropertyResults.Count;
            var skip = (model.PageNumber - 1) * model.PageSize;
            var propertyResults = orphanedPropertyResults.Skip(skip).Take(model.PageSize).ToList();
            model.OrphanedProperties = propertyResults;

            return View(model);
        }

        [HttpPost]
        [Route("[action]")]
        public JsonResult Delete([FromBody] PropertiesDataModel data)
        {
            try
            {
                var orphanedPropertyResults = _missingPropertiesService.GetMissingProperties();

                var orphanedPropertyIds = new HashSet<int>(orphanedPropertyResults.Select(x => x.PropertyId));

                var propertyList = data.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var message = "";

                foreach (var value in propertyList)
                {
                    if (int.TryParse(value, out var propertyId) && orphanedPropertyIds.Contains(propertyId))
                    {
                        var response = _missingPropertiesService.ProcessProperty(propertyId);

                        message += $"{response.Description}\n";
                    }
                }

                return Json(new
                {
                    status = true,
                    message
                });

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }
    }
}