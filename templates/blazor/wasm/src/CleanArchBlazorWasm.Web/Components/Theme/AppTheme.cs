using MudBlazor;

namespace CleanArchBlazorWasm.Web.Components.Theme;

public static class AppTheme
{
    public static readonly MudTheme Default = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#B2603A",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#7C8B6F",
            SecondaryContrastText = "#FFFFFF",
            Tertiary = "#C9A227",
            TertiaryContrastText = "#2E2A24",
            Info = "#5C7C8A",
            Success = "#6E8B5D",
            Warning = "#C98A3B",
            Error = "#B04632",
            Background = "#F5EFDF",
            Surface = "#FCF9F1",
            DrawerBackground = "#EFE6D2",
            DrawerText = "#3A332A",
            AppbarBackground = "#B2603A",
            AppbarText = "#FFFFFF",
            TextPrimary = "#2E2A24",
            TextSecondary = "#6B6255",
            LinesDefault = "#E3D6BE",
            Divider = "#E3D6BE",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#D98B5F",
            PrimaryContrastText = "#2E2A24",
            Secondary = "#9CAE8C",
            SecondaryContrastText = "#2E2A24",
            Tertiary = "#D9B95C",
            TertiaryContrastText = "#2E2A24",
            Info = "#7FA6B3",
            Success = "#8FB07A",
            Warning = "#D9A15C",
            Error = "#D97A63",
            Background = "#221D17",
            Surface = "#2B241C",
            DrawerBackground = "#1D1812",
            DrawerText = "#EDE6D9",
            AppbarBackground = "#5C3620",
            AppbarText = "#EDE6D9",
            TextPrimary = "#EDE6D9",
            TextSecondary = "rgba(237,230,217,0.70)",
            LinesDefault = "#3A2F24",
            Divider = "#3A2F24",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "6px",
        },
    };
}
