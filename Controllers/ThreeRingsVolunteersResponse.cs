using Newtonsoft.Json;

public class ThreeRingsVolunteersResponse
{
    [JsonProperty(PropertyName = "volunteers")]
    public List<ThreeRingsVolunteerResponse> Volunteers { get; set; } = new List<ThreeRingsVolunteerResponse>();
}

public class ThreeRingsVolunteerResponse
{

    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "org_id")]
    public string OrgId { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "url")]
    public string Url { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "roles")]
    public List<Role> Roles { get; set; } = new List<Role>();

    [JsonProperty(PropertyName = "volunteer_properties")]
    public List<Dictionary<string, Property>> VolunteerProperties { get; set; } = new List<Dictionary<string, Property>>();


}
public class Role
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "suffix")]
    public string Suffix { get; set; } = string.Empty;

}

public class Property
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "org_name")]
    public string OrgName { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "value")]
    public string Value { get; set; } = string.Empty;
}