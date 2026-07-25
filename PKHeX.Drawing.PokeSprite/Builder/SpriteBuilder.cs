using System.Drawing;
using PKHeX.Core;
using PKHeX.Drawing.PokeSprite.Properties;

namespace PKHeX.Drawing.PokeSprite;

public abstract class SpriteBuilder : ISpriteBuilder<Bitmap>
{
    public static bool ShowEggSpriteAsItem { get; set; } = true;
    public static bool ShowEncounterBall { get; set; } = true;
    public static SpriteBackgroundType ShowEncounterColor { get; set; } = SpriteBackgroundType.FullBackground;
    public static SpriteBackgroundType ShowEncounterColorPKM { get; set; }
    public static SpriteBackgroundType ShowTeraType { get; set; } = SpriteBackgroundType.TopStripe;
    public static bool ShowExperiencePercent { get; set; }
    public static byte ShowTeraOpacityStripe { get; set; }
    public static int ShowTeraThicknessStripe { get; set; }
    public static byte ShowTeraOpacityBackground { get; set; }
    public static byte ShowEncounterOpacityStripe { get; set; }
    public static byte ShowEncounterOpacityBackground { get; set; }
    public static int ShowEncounterThicknessStripe { get; set; }
    public static float FilterMismatchOpacity { get; set; }
    public static float FilterMismatchGrayscale { get; set; }

    /// <summary> Width of the generated Sprite image. </summary>
    public abstract int Width { get; }
    /// <summary> Height of the generated Sprite image. </summary>
    public abstract int Height { get; }

    /// <summary> Minimum amount of padding on the right side of the image when layering an item sprite. </summary>
    protected abstract int ItemShiftX { get; }
    /// <summary> Minimum amount of padding on the bottom side of the image when layering an item sprite. </summary>
    protected abstract int ItemShiftY { get; }
    /// <summary> Max width / height of an item image. </summary>
    protected abstract int ItemMaxSize { get; }

    protected abstract int EggItemShiftX { get; }
    protected abstract int EggItemShiftY { get; }

    public abstract bool HasFallbackMethod { get; }

    public abstract Bitmap Hover { get; }
    public abstract Bitmap View { get; }
    public abstract Bitmap Set { get; }
    public abstract Bitmap Delete { get; }
    public abstract Bitmap Transparent { get; }
    public abstract Bitmap Drag { get; }
    public abstract Bitmap UnknownItem { get; }
    public abstract Bitmap None { get; }
    public abstract Bitmap ItemTM { get; }
    public abstract Bitmap ItemTR { get; }

    private const double UnknownFormTransparency = 0.5;
    private const double ShinyTransparency = 0.7;
    private const double EggUnderLayerTransparency = 0.33;

    protected abstract string GetSpriteStringSpeciesOnly(ushort species);

    protected abstract string GetSpriteAll(ushort species, byte form, byte gender, uint formarg, bool shiny, EntityContext context);
    protected abstract string GetSpriteAllSecondary(ushort species, byte form, byte gender, uint formarg, bool shiny, EntityContext context);
    protected abstract string GetItemResourceName(int item);
    protected abstract Bitmap Unknown { get; }
    protected abstract Bitmap GetEggSprite(ushort species);
    public abstract Bitmap ShadowLugia { get; }

    /// <summary>
    /// Ensures all data is set up to generate sprites for the save file.
    /// </summary>
    public void Initialize(SaveFile sav)
    {
        if (sav.Generation != 3)
            return;

        // If the game is indeterminate, we might have different form sprites.
        // Currently, this only applies to Gen3's FireRed / LeafGreen
        Version = sav.Version;
        if (Version == GameVersion.FRLG)
            Version = ReferenceEquals(sav.Personal, PersonalTable.FR) ? GameVersion.FR : GameVersion.LG;
    }

    private GameVersion Version;

    private static byte GetDeoxysForm(GameVersion version) => version switch
    {
        GameVersion.FR => 1, // Attack
        GameVersion.LG => 2, // Defense
        GameVersion.E => 3, // Speed
        _ => 0,
    };

    private static byte GetArceusForm4(byte form) => form switch
    {
        > 9 => --form, // Realign to Gen5+ type indexes
        9 => byte.MaxValue, // Curse, make it show as unrecognized form since we don't have a sprite.
        _ => form,
    };

    /// <summary>
    /// Builds a new sprite image with the requested parameters.
    /// </summary>
    /// <param name="species">Entity Species ID</param>
    /// <param name="form">Entity Form index</param>
    /// <param name="gender">Entity gender</param>
    /// <param name="formarg">Entity <see cref="IFormArgument.FormArgument"/> raw value</param>
    /// <param name="heldItem">Entity held item ID</param>
    /// <param name="isEgg">Is currently in an egg</param>
    /// <param name="shiny">Is it shiny</param>
    /// <param name="context">Context the sprite is for</param>
    public Bitmap GetSprite(ushort species, byte form, byte gender, uint formarg, int heldItem, bool isEgg, Shiny shiny = Shiny.Never, EntityContext context = EntityContext.None)
    {
        if (species == 0)
            return None;

        if (context == EntityContext.Gen3 && species == (int)Species.Deoxys) // Depends on Gen3 save file version
            form = GetDeoxysForm(Version);
        else if (context == EntityContext.Gen4 && species == (int)Species.Arceus) // Curse type's existence in Gen4
            form = GetArceusForm4(form);

        var baseImage = GetBaseImage(species, form, gender, formarg, shiny.IsShiny(), context);
        return GetSprite(baseImage, species, heldItem, isEgg, shiny, context);
    }

    public Bitmap GetSprite(Bitmap baseSprite, ushort species, int heldItem, bool isEgg, Shiny shiny, EntityContext context = EntityContext.None)
    {
        if (isEgg)
            baseSprite = LayerOverImageEgg(baseSprite, species, heldItem != 0);
        if (heldItem > 0)
            baseSprite = LayerOverImageItem(baseSprite, heldItem, context);
        if (shiny.IsShiny())
        {
            if (shiny == Shiny.AlwaysSquare && !context.IsSquareShinyDifferentiated)
                shiny = Shiny.Always;
            baseSprite = LayerOverImageShiny(baseSprite, shiny);
        }
        return baseSprite;
    }

    private Bitmap GetBaseImage(ushort species, byte form, byte gender, uint formarg, bool shiny, EntityContext context)
    {
        var img = FormInfo.IsTotemForm(species, form, context)
            ? GetBaseImageTotem(species, form, gender, formarg, shiny, context)
            : GetBaseImageDefault(species, form, gender, formarg, shiny, context);
        return img ?? GetBaseImageFallback(species, form, gender, formarg, shiny, context);
    }

    private Bitmap? GetBaseImageTotem(ushort species, byte form, byte gender, uint formarg, bool shiny, EntityContext context)
    {
        var baseform = FormInfo.GetTotemBaseForm(species, form);
        var b = GetBaseImageDefault(species, baseform, gender, formarg, shiny, context);
        if (b is null)
            return null;

        SpriteUtil.GetSpriteGlow(b, 0, 165, 255, out var pixels, true);
        var layer = ImageUtil.GetBitmap(pixels, b.Width, b.Height, b.PixelFormat);
        return ImageUtil.LayerImage(b, layer, 0, 0);
    }

    private Bitmap? GetBaseImageDefault(ushort species, byte form, byte gender, uint formarg, bool shiny, EntityContext context)
    {
        var file = GetSpriteAll(species, form, gender, formarg, shiny, context);
        var resource = (Bitmap?)Resources.ResourceManager.GetObject(file);
        if (resource is null && HasFallbackMethod)
        {
            file = GetSpriteAllSecondary(species, form, gender, formarg, shiny, context);
            resource = (Bitmap?)Resources.ResourceManager.GetObject(file);
        }
        return resource;
    }

    private Bitmap GetBaseImageFallback(ushort species, byte form, byte gender, uint formarg, bool shiny, EntityContext context)
    {
        if (shiny) // try again without shiny
        {
            var img = GetBaseImageDefault(species, form, gender, formarg, false, context);
            if (img is not null)
                return img;
        }

        // try again without form
        var baseImage = (Bitmap?)Resources.ResourceManager.GetObject(GetSpriteStringSpeciesOnly(species));
        if (baseImage is null) // failed again
            return Unknown;
        return ImageUtil.LayerImage(baseImage, Unknown, 0, 0, UnknownFormTransparency);
    }

    private Bitmap LayerOverImageItem(Bitmap baseImage, int item, EntityContext context)
    {
        var itemimg = GetItemSprite(item, context);

        // Redraw item in bottom right corner; since images are cropped, try to not have them at the edge
        int x = baseImage.Width - itemimg.Width - ((ItemMaxSize - itemimg.Width) / 4) - ItemShiftX;
        int y = baseImage.Height - itemimg.Height - ItemShiftY;
        return ImageUtil.LayerImage(baseImage, itemimg, x, y);
    }

    public Bitmap GetItemSprite(int item, EntityContext context)
    {
        if (context is EntityContext.Gen7 && Gen7Expansion.IsItemID(item))
            return (Bitmap?)Resources.ResourceManager.GetObject($"aitem_{GetExpansionItemSpriteID(item)}") ?? UnknownItem;

        var lump = HeldItemLumpUtil.GetIsLump(item, context);
        return lump switch
        {
            HeldItemLumpImage.TechnicalMachine => ItemTM,
            HeldItemLumpImage.TechnicalRecord => ItemTR,
            _ => (Bitmap?)Resources.ResourceManager.GetObject(GetItemResourceName(item)) ?? UnknownItem,
        };
    }

    private static int GetExpansionItemSpriteID(int item) => item switch
    {
        505 => 2642,
        506 => 2644,
        507 => 2645,
        508 => 2586,
        509 => 2647,
        510 => 2648,
        511 => 2650,
        512 => 2638,
        513 => 2640,
        514 => 2641,
        515 => 2577,
        516 => 2579,
        517 => 2584,
        518 => 2643,
        519 => 2646,
        520 => 2649,
        960 => 1880,
        961 => 1777,
        962 => 1879,
        963 => 1103,
        964 => 1104,
        965 => 544,
        966 => 1777,
        967 => 1778,
        968 => 2407,
        969 => 2408,
        970 => 2406,
        971 => 1582,
        972 => 1592,
        973 => 1117,
        974 => 1116,
        975 => 1253,
        976 => 1254,
        977 => 1109,
        978 => 1111,
        979 => 1110,
        980 => 1114,
        981 => 1112,
        982 => 1113,
        983 => 1115,
        984 => 312,
        985 => 299,
        986 => 1691,
        987 => 238,
        988 => 2344,
        989 => 1861,
        990 => 2345,
        991 => 2402,
        992 => 2403,
        993 => 2404,
        994 => 92,
        995 => 2635,
        996 => 2636,
        997 => 2559,
        998 => 2560,
        999 => 2561,
        1000 => 2562,
        1001 => 2563,
        1002 => 2564,
        1003 => 2565,
        1004 => 2566,
        1005 => 2569,
        1006 => 2570,
        1007 => 2571,
        1008 => 2572,
        1009 => 2573,
        1010 => 2574,
        1011 => 2575,
        1012 => 2576,
        1013 => 2578,
        1014 => 2580,
        1015 => 2581,
        1016 => 2582,
        1017 => 2583,
        1018 => 2585,
        1019 => 2587,
        1020 => 2637,
        1021 => 2639,
        1022 => 2567,
        1023 => 2568,
        _ => 0,
    };

    private static Bitmap LayerOverImageShiny(Bitmap baseImage, Shiny shiny)
    {
        // Add shiny star to top left of image.
        Bitmap rare;
        if (shiny is Shiny.AlwaysSquare)
            rare = Resources.rare_icon_alt_2;
        else
            rare = Resources.rare_icon_alt;
        return ImageUtil.LayerImage(baseImage, rare, 0, 0, ShinyTransparency);
    }

    private Bitmap LayerOverImageEgg(Bitmap baseImage, ushort species, bool hasItem)
    {
        if (ShowEggSpriteAsItem && !hasItem)
            return LayerOverImageEggAsItem(baseImage, species);
        return LayerOverImageEggTransparentSpecies(baseImage, species);
    }

    private Bitmap LayerOverImageEggTransparentSpecies(Bitmap baseImage, ushort species)
    {
        // Partially transparent species.
        baseImage.ChangeOpacity(EggUnderLayerTransparency);
        // Add the egg layer over-top with full opacity.
        var egg = GetEggSprite(species);
        return ImageUtil.LayerImage(baseImage, egg, 0, 0);
    }

    private Bitmap LayerOverImageEggAsItem(Bitmap baseImage, ushort species)
    {
        var egg = GetEggSprite(species);
        return ImageUtil.LayerImage(baseImage, egg, EggItemShiftX, EggItemShiftY); // similar to held item, since they can't have any
    }

    public static void LoadSettings(ISpriteSettings sprite)
    {
        ShowEggSpriteAsItem = sprite.ShowEggSpriteAsHeldItem;
        ShowEncounterBall = sprite.ShowEncounterBall;

        ShowEncounterColor = sprite.ShowEncounterColor;
        ShowEncounterColorPKM = sprite.ShowEncounterColorPKM;
        ShowEncounterThicknessStripe = sprite.ShowEncounterThicknessStripe;
        ShowEncounterOpacityBackground = sprite.ShowEncounterOpacityBackground;
        ShowEncounterOpacityStripe = sprite.ShowEncounterOpacityStripe;
        ShowExperiencePercent = sprite.ShowExperiencePercent;

        ShowTeraType = sprite.ShowTeraType;
        ShowTeraThicknessStripe   = sprite.ShowTeraThicknessStripe;
        ShowTeraOpacityBackground = sprite.ShowTeraOpacityBackground;
        ShowTeraOpacityStripe     = sprite.ShowTeraOpacityStripe;

        FilterMismatchOpacity = sprite.FilterMismatchOpacity;
        FilterMismatchGrayscale = sprite.FilterMismatchGrayscale;
    }
}
