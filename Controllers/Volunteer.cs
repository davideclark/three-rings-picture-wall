using System.Globalization;

public class Volunteer
{
  public string Name { get; set; } = string.Empty;
  public string Inactivity { get; set; } = string.Empty;

  // First Name
  public string FirstName { get; set; } = string.Empty;
  public string Surname { get; set; } = string.Empty;
  //Sam Name
  public string SamName { get; set; } = string.Empty;
  public string Gender { get; set; } = string.Empty;
  //Date of Birth"
  public string DateOfBirth { get; set; } = string.Empty;
  public string CarRegistrationPlate { get; set; } = string.Empty;
  public string Website { get; set; } = string.Empty;
  public string Occupation { get; set; } = string.Empty;
  public DateTime JoinDate { get; set; }
  public string Commitment { get; set; } = string.Empty;
  public string AboutMe { get; set; } = string.Empty;
  public string Allergies { get; set; } = string.Empty;
  public string IdNumber { get; set; } = string.Empty;
  public string Address { get; set; } = string.Empty;
  public string TelephoneWork { get; set; } = string.Empty;
  public string TelephoneMobile { get; set; } = string.Empty;
  public string TelephoneMobileNetwork { get; set; } = string.Empty;
  public string TelephoneLandline { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string AlternateEmail { get; set; } = string.Empty;
  public string EmergencyContactDetails { get; set; } = string.Empty;
  public string Roles { get; set; } = string.Empty;
  public string Relationships { get; set; } = string.Empty;
  // public string JoinDateFormated { get; set; } = string.Empty;
  public string AnniversaryYear { get { return (DateTime.Now.Year - JoinDate.Year).ToString(); } }
  public string JoinDateFormated { get { return JoinDate.ToString("D", CultureInfo.InvariantCulture); } }
}
