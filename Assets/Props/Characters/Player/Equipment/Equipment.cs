using System;
using System.Collections.Generic;

public enum EquipmentType : int
{
    Badge,
    Bowtie,
    BunnyEars,
    Button,
    ClownNose,
    CowboyHat,
    Disk,
    Earing,
    EmoChain,
    FingerRing,
    Floatie,
    Glasses,
    GoldChain,
    GradHat,
    GrippyTire,
    HardHat,
    MagicHat,
    Monacle,
    Moustache,
    PartyHat,
    PokaHat,
    Pylon,
    Tie,
    VikingHat,
    Visor,
    WitchHat
}

public class EquipmentInfo
{
    public string title;
    public string description;
    public int price;
    public bool available;
    
    public EquipmentInfo(string title, string description, int price, bool available = true) {
        this.title = title;
        this.description = description;
        this.price = price;
        this.available = available;
    }
    
    public static readonly Dictionary<EquipmentType, EquipmentInfo> info = new Dictionary<EquipmentType, EquipmentInfo>(){
        { EquipmentType.Badge, new EquipmentInfo("Badge", "Allows access to restricted areas.", 10000) },
        { EquipmentType.Bowtie, new EquipmentInfo("Bow Tie", "Allows access to restricted areas.", 10000) },
        { EquipmentType.BunnyEars, new EquipmentInfo("Bunny Ears", "Allows access to restricted areas.", 10000) },
        { EquipmentType.Button, new EquipmentInfo("Button", "Allows access to restricted areas.", 10000) },
        { EquipmentType.ClownNose, new EquipmentInfo("Clown Nose", "Allows access to restricted areas.", 10000) },
        { EquipmentType.CowboyHat, new EquipmentInfo("Cowboy Hat", "Allows access to restricted areas.", 10000) },
        { EquipmentType.Disk, new EquipmentInfo("Disk", "Allows access to restricted areas.", 10000) },
        { EquipmentType.Earing, new EquipmentInfo("Earing", "Allows access to restricted areas.", 10000) },
        { EquipmentType.EmoChain, new EquipmentInfo("Emo Chain", "Allows access to restricted areas.", 10000) },
        { EquipmentType.FingerRing, new EquipmentInfo("Finger Ring", "Allows access to restricted areas.", 10000) },
        { EquipmentType.Floatie, new EquipmentInfo("Floatie", "Allows access to restricted areas.", 10000) },
        { EquipmentType.Glasses, new EquipmentInfo("Glasses", "Allows access to restricted areas.", 10000) },
        { EquipmentType.GoldChain, new EquipmentInfo("Gold Chain", "Allows access to restricted areas.", 10000) },
        { EquipmentType.GradHat, new EquipmentInfo("Grad Hat", "Allows access to restricted areas.", 10000) },
        { EquipmentType.GrippyTire, new EquipmentInfo("Grippy Tire", "Allows access to restricted areas.", 10000) },
        { EquipmentType.HardHat, new EquipmentInfo("Hard Hat", "Allows access to restricted areas.", 10000) },
        { EquipmentType.MagicHat, new EquipmentInfo("Magic Hat", "Allows access to restricted areas.", 10000) },
        { EquipmentType.Monacle, new EquipmentInfo("Monacle", "Allows access to restricted areas.", 10000) },
        { EquipmentType.Moustache, new EquipmentInfo("Moustache", "Allows access to restricted areas.", 10000) },
        { EquipmentType.PartyHat, new EquipmentInfo("Party Hat", "Allows access to restricted areas.", 10000) },
        { EquipmentType.PokaHat, new EquipmentInfo("Poka Hat", "Allows access to restricted areas.", 10000) },
        { EquipmentType.Pylon, new EquipmentInfo("Pylon", "Allows access to restricted areas.", 10000) },
        { EquipmentType.Tie, new EquipmentInfo("Tie", "Allows access to restricted areas.", 10000) },
        { EquipmentType.VikingHat, new EquipmentInfo("Viking Hat", "Allows access to restricted areas.", 10000) },
        { EquipmentType.Visor, new EquipmentInfo("Visor", "Allows access to restricted areas.", 10000) },
        { EquipmentType.WitchHat, new EquipmentInfo("Witch Hat", "Allows access to restricted areas.", 10000) },
    };
    
    public static readonly ConflictTable conflicts = new ConflictTable(Util.EnumCount<EquipmentType>(), true) {
        {
            EquipmentType.Bowtie,
            EquipmentType.GoldChain,
            EquipmentType.Tie
        },
        {
            EquipmentType.Glasses,
            EquipmentType.Monacle
        },
        {
            EquipmentType.BunnyEars,
            EquipmentType.CowboyHat,
            EquipmentType.GradHat,
            EquipmentType.HardHat,
            EquipmentType.MagicHat,
            EquipmentType.PartyHat,
            EquipmentType.PokaHat,
            EquipmentType.Pylon,
            EquipmentType.VikingHat,
            EquipmentType.Visor,
            EquipmentType.WitchHat
        }
    };
}
