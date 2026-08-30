using MudBlazor;

namespace CleanArchBlazorWasm.Web.Components.Theme;

public static class AppTheme
{
#if (Palette_Ocean)
    // Ocean - teal and marine blue, calm and nautical.
    public static readonly MudTheme Default = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#1C6E8C",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#3E9C87",
            SecondaryContrastText = "#FFFFFF",
            Tertiary = "#E3A857",
            TertiaryContrastText = "#24333A",
            Info = "#4F93B0",
            Success = "#3E9C87",
            Warning = "#D98A3B",
            Error = "#C0453A",
            Background = "#EAF4F6",
            Surface = "#FFFFFF",
            DrawerBackground = "#DCEAEE",
            DrawerText = "#1B333B",
            AppbarBackground = "#1C6E8C",
            AppbarText = "#FFFFFF",
            TextPrimary = "#1B333B",
            TextSecondary = "#5A727A",
            LinesDefault = "#CFE1E6",
            Divider = "#CFE1E6",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#5FB3D1",
            PrimaryContrastText = "#0D2530",
            Secondary = "#6FCBB0",
            SecondaryContrastText = "#0D2530",
            Tertiary = "#EFC178",
            TertiaryContrastText = "#2A2010",
            Info = "#7FBBD4",
            Success = "#7FCBB2",
            Warning = "#E3A968",
            Error = "#E3766A",
            Background = "#0E2530",
            Surface = "#142E3A",
            DrawerBackground = "#0A1D26",
            DrawerText = "#DDEEF2",
            AppbarBackground = "#123C4B",
            AppbarText = "#DDEEF2",
            TextPrimary = "#DDEEF2",
            TextSecondary = "rgba(221,238,242,0.70)",
            LinesDefault = "#1E3D49",
            Divider = "#1E3D49",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "6px",
        },
    };
#elif (Palette_Forest)
    // Forest - deep green, walnut brown and moss gold.
    public static readonly MudTheme Default = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#2E6B4F",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#8A6A46",
            SecondaryContrastText = "#FFFFFF",
            Tertiary = "#B79A2E",
            TertiaryContrastText = "#2A2410",
            Info = "#4C7A8C",
            Success = "#4C8C5E",
            Warning = "#C6903D",
            Error = "#A8432F",
            Background = "#EDF1E6",
            Surface = "#F8FAF4",
            DrawerBackground = "#DEE6D2",
            DrawerText = "#2A3324",
            AppbarBackground = "#2E6B4F",
            AppbarText = "#FFFFFF",
            TextPrimary = "#24301F",
            TextSecondary = "#5C6B54",
            LinesDefault = "#D3DEC7",
            Divider = "#D3DEC7",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#6FB891",
            PrimaryContrastText = "#12241A",
            Secondary = "#C6A377",
            SecondaryContrastText = "#241C10",
            Tertiary = "#D9C066",
            TertiaryContrastText = "#2A2410",
            Info = "#7FA8B8",
            Success = "#7FBF93",
            Warning = "#D9AE6E",
            Error = "#D97C6A",
            Background = "#16211A",
            Surface = "#1C2921",
            DrawerBackground = "#101A14",
            DrawerText = "#E4EBDE",
            AppbarBackground = "#1E4633",
            AppbarText = "#E4EBDE",
            TextPrimary = "#E4EBDE",
            TextSecondary = "rgba(228,235,222,0.70)",
            LinesDefault = "#2B3A2E",
            Divider = "#2B3A2E",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "6px",
        },
    };
#elif (Palette_Sunset)
    // Sunset - coral, plum magenta and amber.
    public static readonly MudTheme Default = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#D9552C",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#A3436B",
            SecondaryContrastText = "#FFFFFF",
            Tertiary = "#E3A73E",
            TertiaryContrastText = "#2E2410",
            Info = "#6D7BA6",
            Success = "#5C8C5D",
            Warning = "#D98A3B",
            Error = "#B8352E",
            Background = "#FBEEE6",
            Surface = "#FFF8F3",
            DrawerBackground = "#F2DACB",
            DrawerText = "#3A2A22",
            AppbarBackground = "#D9552C",
            AppbarText = "#FFFFFF",
            TextPrimary = "#362420",
            TextSecondary = "#6E5850",
            LinesDefault = "#EBD4C4",
            Divider = "#EBD4C4",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#E88A63",
            PrimaryContrastText = "#2A1610",
            Secondary = "#C97D9E",
            SecondaryContrastText = "#2A1620",
            Tertiary = "#EFC170",
            TertiaryContrastText = "#2A2210",
            Info = "#8F9AC2",
            Success = "#8FBF90",
            Warning = "#E3A968",
            Error = "#E37A6E",
            Background = "#241713",
            Surface = "#2C1E19",
            DrawerBackground = "#1B110D",
            DrawerText = "#F0E1D8",
            AppbarBackground = "#6E2E1B",
            AppbarText = "#F0E1D8",
            TextPrimary = "#F0E1D8",
            TextSecondary = "rgba(240,225,216,0.70)",
            LinesDefault = "#3A2A23",
            Divider = "#3A2A23",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "6px",
        },
    };
#elif (Palette_Lavender)
    // Lavender - violet, dusty rose and slate blue.
    public static readonly MudTheme Default = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#6B5B95",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#B5788C",
            SecondaryContrastText = "#FFFFFF",
            Tertiary = "#6E88B5",
            TertiaryContrastText = "#FFFFFF",
            Info = "#6E88B5",
            Success = "#5E8C6F",
            Warning = "#C9923D",
            Error = "#B0453F",
            Background = "#F3EFF7",
            Surface = "#FBF9FC",
            DrawerBackground = "#E4DCEE",
            DrawerText = "#332C3E",
            AppbarBackground = "#6B5B95",
            AppbarText = "#FFFFFF",
            TextPrimary = "#2E2838",
            TextSecondary = "#675D77",
            LinesDefault = "#E0D6EA",
            Divider = "#E0D6EA",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#A794D1",
            PrimaryContrastText = "#211A30",
            Secondary = "#D9A5B6",
            SecondaryContrastText = "#2E1B22",
            Tertiary = "#9FB3D9",
            TertiaryContrastText = "#1B2430",
            Info = "#9FB3D9",
            Success = "#8FBF9C",
            Warning = "#E0AF6E",
            Error = "#E08A82",
            Background = "#201A29",
            Surface = "#282032",
            DrawerBackground = "#17121E",
            DrawerText = "#ECE6F2",
            AppbarBackground = "#3E3260",
            AppbarText = "#ECE6F2",
            TextPrimary = "#ECE6F2",
            TextSecondary = "rgba(236,230,242,0.70)",
            LinesDefault = "#332B3F",
            Divider = "#332B3F",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "6px",
        },
    };
#elif (Palette_Slate)
    // Slate - steel blue-gray with a cyan accent.
    public static readonly MudTheme Default = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#3B5A6B",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#5C7A72",
            SecondaryContrastText = "#FFFFFF",
            Tertiary = "#4E9AA6",
            TertiaryContrastText = "#FFFFFF",
            Info = "#4E9AA6",
            Success = "#57856A",
            Warning = "#C4903F",
            Error = "#B04A3F",
            Background = "#EEF1F2",
            Surface = "#F8FAFB",
            DrawerBackground = "#DEE5E8",
            DrawerText = "#26343B",
            AppbarBackground = "#3B5A6B",
            AppbarText = "#FFFFFF",
            TextPrimary = "#212D33",
            TextSecondary = "#566269",
            LinesDefault = "#D4DDE1",
            Divider = "#D4DDE1",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#7FA6B8",
            PrimaryContrastText = "#16232A",
            Secondary = "#8FB0A6",
            SecondaryContrastText = "#14231D",
            Tertiary = "#6FC2CE",
            TertiaryContrastText = "#12262A",
            Info = "#6FC2CE",
            Success = "#7FBF97",
            Warning = "#D9AC6E",
            Error = "#D9827A",
            Background = "#171F23",
            Surface = "#1D262B",
            DrawerBackground = "#111719",
            DrawerText = "#E1E8EA",
            AppbarBackground = "#24404C",
            AppbarText = "#E1E8EA",
            TextPrimary = "#E1E8EA",
            TextSecondary = "rgba(225,232,234,0.70)",
            LinesDefault = "#2A363B",
            Divider = "#2A363B",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "6px",
        },
    };
#elif (Palette_Citrus)
    // Citrus - tangerine, lime green and sunflower yellow.
    public static readonly MudTheme Default = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#E08A1E",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#7FA639",
            SecondaryContrastText = "#FFFFFF",
            Tertiary = "#E3C23E",
            TertiaryContrastText = "#2E2810",
            Info = "#4C8CA6",
            Success = "#6E9C3F",
            Warning = "#D9942E",
            Error = "#C0452F",
            Background = "#FBF3E1",
            Surface = "#FFFBF0",
            DrawerBackground = "#F2E4C0",
            DrawerText = "#3A2E14",
            AppbarBackground = "#E08A1E",
            AppbarText = "#FFFFFF",
            TextPrimary = "#332B14",
            TextSecondary = "#6B6042",
            LinesDefault = "#EBDCB2",
            Divider = "#EBDCB2",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#F0AE5E",
            PrimaryContrastText = "#2A1D08",
            Secondary = "#A6C468",
            SecondaryContrastText = "#1E2A10",
            Tertiary = "#EFD87E",
            TertiaryContrastText = "#2E2810",
            Info = "#7FB3C4",
            Success = "#9CC46E",
            Warning = "#E3B25E",
            Error = "#E3766A",
            Background = "#24200F",
            Surface = "#2C2714",
            DrawerBackground = "#1A160A",
            DrawerText = "#F0E8D2",
            AppbarBackground = "#6B4515",
            AppbarText = "#F0E8D2",
            TextPrimary = "#F0E8D2",
            TextSecondary = "rgba(240,232,210,0.70)",
            LinesDefault = "#3A331B",
            Divider = "#3A331B",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "6px",
        },
    };
#else
    // Terracotta (default) - warm rust, olive and gold.
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
#endif
}
