using UnityEngine;
using System.Collections.Generic;

public class EquipmentController : MonoBehaviour
{
    public GameObject badge;
    public GameObject bowtie;
    public GameObject bracelet;
    public GameObject bunnyEars;
    public GameObject clownNose;
    public GameObject cowboyHat;
    public GameObject earRings;
    public GameObject floatie;
    public GameObject glasses;
    public GameObject goldChain;
    public GameObject goldRing;
    public GameObject gradHat;
    public GameObject hardHat;
    public GameObject magicHat;
    public GameObject monacle;
    public GameObject moustache;
    public GameObject neckTie;
    public GameObject partyHat;
    public GameObject pinButton;
    public GameObject pokaHat;
    public GameObject pylonHat;
    public GameObject vikingHat;
    public GameObject visor;
    public GameObject witchHat;
    
    private Dictionary<EquipmentType, GameObject> _equipment = null;
    private Dictionary<EquipmentType, GameObject> equipment
    {
        get
        {
            if(_equipment == null)
            {
                _equipment = new Dictionary<EquipmentType, GameObject>(){
                    { EquipmentType.Badge, badge },
                    { EquipmentType.Bowtie, bowtie },
                    { EquipmentType.BunnyEars, bunnyEars },
                    { EquipmentType.Button, pinButton },
                    { EquipmentType.ClownNose, clownNose },
                    { EquipmentType.CowboyHat, cowboyHat },
                    { EquipmentType.Disk, null },
                    { EquipmentType.Earing, earRings },
                    { EquipmentType.EmoChain, bracelet },
                    { EquipmentType.FingerRing, goldRing },
                    { EquipmentType.Floatie, floatie },
                    { EquipmentType.Glasses, glasses },
                    { EquipmentType.GoldChain, goldChain },
                    { EquipmentType.GradHat, gradHat },
                    { EquipmentType.GrippyTire, null },
                    { EquipmentType.HardHat, hardHat },
                    { EquipmentType.MagicHat, magicHat },
                    { EquipmentType.Monacle, monacle },
                    { EquipmentType.Moustache, moustache },
                    { EquipmentType.PartyHat, partyHat },
                    { EquipmentType.PokaHat, pokaHat },
                    { EquipmentType.Pylon, pylonHat },
                    { EquipmentType.Tie, neckTie },
                    { EquipmentType.VikingHat, vikingHat },
                    { EquipmentType.Visor, visor },
                    { EquipmentType.WitchHat, witchHat }
                };
            }
            
            return _equipment;
        }
    }
    
    void Awake()
    {
        foreach(var type in Util.EnumValues<EquipmentType>())
            SetItemEquipped(type, Profile.Equipment[type].equipped);
    }

    public void SetItemEquipped(EquipmentType type, bool equipped)
    {
        GameObject item = equipment[type];

        if(item)
            item.SetActive(equipped);
    }

    public bool IsItemEquipped(EquipmentType type)
    {
        GameObject item = equipment[type];
        return item != null ? item.activeSelf : false;
    }
}
