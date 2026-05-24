namespace USJR_COMES_WEBSITE.ViewModels;

public class SiteSettingsRequest
{
    public string BackgroundType { get; set; } = "color";
    public string BackgroundValue { get; set; } = "#07130a";
    public double BackgroundOpacity { get; set; } = 1.0;
    public string BackgroundSize { get; set; } = "cover";
    public string BackgroundPosition { get; set; } = "center";
    public string NavbarColor { get; set; } = "";
    public string HeaderOrgName { get; set; } = "USJ-R COMPUTER ENGINEERING SOCIETY";
    public string HeaderOrgQuote { get; set; } = "Pressure is not a Curse, it is a Blessing.";
    public string? BackgroundImageBase64 { get; set; }
    public string? BackgroundImageMimeType { get; set; }
    public bool ClearBackgroundImage { get; set; }
    public string? HeaderLogoBase64 { get; set; }
    public string? HeaderLogoMimeType { get; set; }
    public bool ClearHeaderLogo { get; set; }
    public string BackgroundCustomSize { get; set; } = "";
    public int HeaderLogoHeight { get; set; } = 100;
    public int FooterLogoHeight { get; set; } = 60;
    public string BackgroundPattern { get; set; } = "circuit";

    // Footer
    public string FooterOrgSub { get; set; } = "This is the Official Website of USJ-R ComEs";
    public string FooterFacebookUrl { get; set; } = "https://www.facebook.com/usjrcomes";
    public string FooterWebsiteUrl { get; set; } = "#";

    // Robot / hero
    public string RobotAdelanteText { get; set; } = "ADELANTE";
    public string RobotSocietyText { get; set; } = "USJ-R Computer Engineering Society";
    public string RobotAbbreviation { get; set; } = "CpES";
    public string RobotTagline { get; set; } = "BUILDING THE\nFUTURE\nTHROUGH\nENGINEERING";

    // Theme
    public string ThemeAccent { get; set; } = "#28b450";
    public string ThemeAccentBright { get; set; } = "#00ff88";
    public string ThemeTextColor { get; set; } = "#e0e0e0";
    public string ThemeFontFamily { get; set; } = "default";
    public int ThemeFontSize { get; set; } = 100;
}
