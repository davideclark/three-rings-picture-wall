using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ThreeRingsPictureWall.Models;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace ThreeRingsPictureWall.Controllers;

public class HomeController : Controller
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<HomeController> _logger;
    private readonly static List<string> _cachedThumbnailIds = new List<string>();
    private IConfiguration _configuration;

    public HomeController(ILogger<HomeController> logger, IMemoryCache memoryCache, IConfiguration configuration)
    {
        _logger = logger;
        _memoryCache = memoryCache;
        _configuration = configuration;
    }

    [HttpGet]
    [Route("Anniversaries")]
    public IActionResult AnniversariesIndex([FromQuery] int numberOfRows, [FromQuery] int numberOfColumns, [FromQuery] int pageNumber, [FromQuery] string ignoreIds, [FromQuery] string roles, [FromQuery] string cardSize, [FromQuery] string nameFontSize, string headingFontSize)
    {
        ViewData["anniversaryHeading"] = DateTime.Now.ToString("MMMM") + " Anniversaries";
        return Index(numberOfRows, numberOfColumns, pageNumber, ignoreIds, roles, cardSize, nameFontSize, headingFontSize);
    }

    public IActionResult Index([FromQuery] int numberOfRows, [FromQuery] int numberOfColumns, [FromQuery] int pageNumber, [FromQuery] string ignoreIds, [FromQuery] string roles, [FromQuery] string cardSize, [FromQuery] string nameFontSize, string headingFontSize)
    {
        if (roles == null)
        {
            roles = "";
        }

        if (numberOfRows == 0)
        {
            numberOfRows = 4;
        }
        if (numberOfColumns == 0)
        {
            numberOfColumns = 5;
        }
        if (ignoreIds == null)
        {
            ignoreIds = string.Empty;
        }
        if (cardSize == null)
        {
            cardSize = "18";
        }
        if (nameFontSize == null)
        {
            nameFontSize = "20";
        }
        if (headingFontSize == null)
        {
            headingFontSize = "50";
        }

        ViewData["numberOfRows"] = numberOfRows;
        ViewData["numberOfColumns"] = numberOfColumns;
        ViewData["pageNumber"] = pageNumber;
        ViewData["ignoreIds"] = ignoreIds;
        ViewData["roles"] = roles;
        ViewData["cardSize"] = cardSize;
        ViewData["nameFontSize"] = nameFontSize;
        ViewData["headingFontSize"] = headingFontSize;
        return View();
    }

    [HttpGet]
    [Route("help")]
    public IActionResult Help()
    {
        return View();
    }

    [HttpGet]
    [Route("clearcache")]
    public IActionResult ClearCache()
    {
        try
        {
            foreach (var id in _cachedThumbnailIds)
            {
                _memoryCache.Remove(id);
            }
            _cachedThumbnailIds.Clear();

            _memoryCache.Remove("threeRingsVolunteersResponse");
            return Ok("Cache Cleared");
        }
        catch (Exception ex)
        {
            var errorMessage = "Error Clearing cache ";
            _logger.Log(LogLevel.Error, errorMessage);
            return Problem(
            detail: ex.Message + ex.StackTrace,
            title: errorMessage);
        }
    }

    [HttpGet]
    [Route("home/volunteers")]
    public async Task<IActionResult> GetVolunteers([FromQuery] int numberOfRows, [FromQuery] int numberOfColumns, [FromQuery] int pageNumber, [FromQuery] string ignoreIds, [FromQuery] string roles, bool? anniversaries)
    {
        if (anniversaries == null)
        {
            anniversaries = false;
        }

        try
        {
            List<Volunteer> volunteers = new List<Volunteer>();
            var threeRingsVolunteersResponse = new ThreeRingsVolunteersResponse();

            // If cached volumteers do not exist
            if (!_memoryCache.TryGetValue("threeRingsVolunteersResponse", out string? cachedThreeRingsResponse))
            {
                var url = _configuration.GetValue<string>("ThreeRingsUrl") ?? throw new Exception("ThreeRingsUrl variable not set");
                var apiKey = _configuration.GetValue<string>("ThreeRingsApiKey") ?? throw new Exception("ApiKey variable not set");
                var ContactEmail = _configuration.GetValue<string>("ContactEmail") ?? throw new Exception("ContactEmail variable not set");

                url = url + "directory.json";
                var uri = new Uri(url);

                // If your developing your own app based off this please update the user agent to refelect your app name.
                var userAgent = "ThreeRingsPictureWall/1.0";

                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = uri,
                    Headers = {
                    { "Authorization", $"APIKEY {apiKey}" },
                    { "User-Agent", userAgent },
                    { "x-contact-email", ContactEmail}
                },
                };
                var httpClient = new HttpClient();

                HttpResponseMessage? response = null;
                response = httpClient.SendAsync(httpRequestMessage).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    threeRingsVolunteersResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ThreeRingsVolunteersResponse>(responseContent);

                    if (threeRingsVolunteersResponse == null)
                    {
                        Console.WriteLine($"Unable to DeserializeObject response from 3 Rings");
                        return Ok(volunteers);
                    }

                    //Cache the response content
                    MemoryCacheEntryOptions options = new MemoryCacheEntryOptions();
                    options.Priority = CacheItemPriority.Normal;
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60);
                    _memoryCache.Set("threeRingsVolunteersResponse", responseContent, options);
                }
                else
                {
                    var errorMessage = $"got non success response from API" + response.StatusCode + "\n";
                    throw new Exception(errorMessage);
                }
            }
            else
            {
                threeRingsVolunteersResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ThreeRingsVolunteersResponse>(cachedThreeRingsResponse);
            }

            // if anniversaries is true the remove all volunteers that did not start volunteering this month
            if (anniversaries == true)
            {
                List<ThreeRingsVolunteerResponse> anniversaryolunteers = new List<ThreeRingsVolunteerResponse>();

                foreach (var volunteer in threeRingsVolunteersResponse.Volunteers)
                {
                    var joinDate = DateTime.Parse(volunteer.VolunteerProperties
                        .Where(vp => vp.ContainsKey("join_date"))
                        .First().First().Value.Value);

                    bool isCurrentMonth = DateTime.Now.Month == joinDate.Month;

                    if (isCurrentMonth && joinDate < DateTime.Now.AddYears(-1))
                    {
                        anniversaryolunteers.Add(volunteer);
                    }
                }

                var sortedAnniversaryolunteers = anniversaryolunteers.OrderBy(volunteer => DateTime.Parse(volunteer.VolunteerProperties
                         .Where(vp => vp.ContainsKey("join_date"))
                         .First().First().Value.Value)).ToList();

                threeRingsVolunteersResponse.Volunteers = sortedAnniversaryolunteers;
            }

            // remove any volunteers we wish to ignore
            if (ignoreIds != null && ignoreIds.Length > 0)
            {

                var ignoreIdsList = ignoreIds.Split(',');
                if (threeRingsVolunteersResponse != null && ignoreIdsList.Count() > 0)
                {
                    foreach (var id in ignoreIdsList)
                    {
                        var volunteerToRemove = threeRingsVolunteersResponse.Volunteers.SingleOrDefault(v => v.Id == id);
                        if (volunteerToRemove != null)
                        {
                            threeRingsVolunteersResponse.Volunteers.Remove(volunteerToRemove);
                        }
                    }
                }
            }

            // filter by role
            var roleFilteredVolunteers = new List<ThreeRingsVolunteerResponse>();
            if (roles != null && roles.Length > 0)
            {

                var rolesList = roles.Split(',').ToList();
                // if (rolesList.Contains("Samaritan"))
                // {
                //     rolesList.Add("Samaritan & Shift Leader");
                // }
                rolesList = rolesList.ConvertAll(d => d.ToLower());

                if (threeRingsVolunteersResponse != null && rolesList.Count() > 0)
                {
                    foreach (var volunteer in threeRingsVolunteersResponse.Volunteers)
                    {
                        bool volunteerHasRole = false;
                        var roleListWithNotAllowedCharactersRemoved = new List<string>();
                        foreach (var role in volunteer.Roles)
                        {
                            // Remove any url reserved characters from the role.
                            var urlReservedCharacters = new List<char> { ';', '/', '?', ':', '@', '&', '=', '+', '$', ',', ' ' };
                            foreach (char urlReservedCharacter in urlReservedCharacters)
                            {
                                role.Name = role.Name.Replace(urlReservedCharacter.ToString(), string.Empty);
                            }

                            if (rolesList.Contains(role.Name.ToLower()))
                            {
                                volunteerHasRole = true;
                            }
                        }

                        if (volunteerHasRole)
                        {
                            roleFilteredVolunteers.Add(volunteer);
                        }
                    }
                    threeRingsVolunteersResponse.Volunteers = roleFilteredVolunteers;
                }
            }

            // We only return the volunteers for the requested page
            var volunteersPerPage = numberOfRows * numberOfColumns;
            var volunteerStartCount = pageNumber * volunteersPerPage;
            var volunteerEndCount = volunteerStartCount + volunteersPerPage;

            // Check for null response from the rings or no volunteers, if so just exist, this is mostly to keep the compilier happy
            if (threeRingsVolunteersResponse == null || threeRingsVolunteersResponse.Volunteers.Count() == 0)
            {
                return Ok(volunteers);
            }

            if (volunteerEndCount > threeRingsVolunteersResponse.Volunteers.Count)
            {
                volunteerEndCount = threeRingsVolunteersResponse.Volunteers.Count;
            }

            for (int volunteerCounter = volunteerStartCount; volunteerCounter < volunteerEndCount; volunteerCounter++)
            {
                var volunteer = new Volunteer();
                volunteer.FirstName = threeRingsVolunteersResponse.Volunteers[volunteerCounter].VolunteerProperties
                    .Where(vp => vp.ContainsKey("first_name"))
                    .First().First().Value.Value;

                volunteer.Surname = threeRingsVolunteersResponse.Volunteers[volunteerCounter].VolunteerProperties
                    .Where(vp => vp.ContainsKey("surname"))
                    .First().First().Value.Value;

                volunteer.Name = threeRingsVolunteersResponse.Volunteers[volunteerCounter].Name;

                volunteer.IdNumber = threeRingsVolunteersResponse.Volunteers[volunteerCounter].Id;

                volunteer.JoinDate = DateTime.Parse(threeRingsVolunteersResponse.Volunteers[volunteerCounter].VolunteerProperties
                        .Where(vp => vp.ContainsKey("join_date"))
                        .First().First().Value.Value);

                volunteers.Add(volunteer);
            }
            string volunteersJson = JsonSerializer.Serialize(volunteers);
            return Ok(volunteersJson);
        }
        catch (Exception ex)
        {
            var errorMessage = "Error getting volunteer list ";

            _logger.Log(LogLevel.Error, errorMessage);
            _logger.Log(LogLevel.Error, ex.Message);
            _logger.Log(LogLevel.Error, ex.StackTrace);
            return Problem(
            detail: ex.Message + ex.StackTrace,
            title: errorMessage);

        }
    }


    [HttpGet("home/volunteerPhoto/{IdNumber}")]
    public IActionResult GetVolunteerPhoto(string idNumber)
    {
        try
        {
            _logger.Log(LogLevel.Information, "Getting photo for  " + idNumber);

            // Build Volunteer photo on 3Rings
            var url = _configuration.GetValue<string>("ThreeRingsUrl") ?? throw new Exception("ThreeRingsUrl variable not set");
            url = url + "directory/" + idNumber + "/photos/thumb.jpg";
            var result = GetUrlContent(url, idNumber);

            // Incase there is no photo
            if (result != null && result.Result != null)
            {
                return File(result.Result, "image/jpg", "thumb.jpg");
            }

            throw new Exception("Unable to download photo " + url);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error getting thumbnail image for {idNumber} from 3 Rings";
            _logger.Log(LogLevel.Error, errorMessage);
            return Problem(
            detail: ex.Message + ex.StackTrace,
            title: errorMessage);
        }
    }

    public async Task<byte[]?> GetUrlContent(string url, string idNumber)
    {
        var apiKey = _configuration.GetValue<string>("ThreeRingsApiKey") ?? throw new Exception("ApiKey variable not set");
        var contactEmail = _configuration.GetValue<string>("ContactEmail") ?? throw new Exception("ContactEmail variable not set");

        // If cached thumbnail does not exist
        if (!_memoryCache.TryGetValue(idNumber, out byte[]? cacheValue))
        {
            var userAgent = "ThreeRingsPictureWall/1.0";

            var uri = new Uri(url);
            HttpClient httpClient = new HttpClient();
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = uri,
                Headers = {
                    { "Authorization", $"APIKEY {apiKey}" },
                    { "User-Agent", userAgent },
                    { "x-contact-email", contactEmail}
            },
            };

            var response = await httpClient.SendAsync(httpRequestMessage);
            if (response.IsSuccessStatusCode)
            {
                MemoryCacheEntryOptions options = new MemoryCacheEntryOptions();
                options.Priority = CacheItemPriority.Normal;
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60);
                _memoryCache.Set(idNumber, await response.Content.ReadAsByteArrayAsync(), options);
                _cachedThumbnailIds.Add(idNumber);
            }
            return response.IsSuccessStatusCode ? await response.Content.ReadAsByteArrayAsync() : null;
        }
        else
        {
            return cacheValue;
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
