using Newtonsoft.Json;

namespace Master
{
    public class ConfigData : ModelBase
    {
        [JsonProperty("maxCannonSettings")]
        public uint maxCannonSettings { get; set; }

        [JsonProperty("maxBattery")]
        public uint maxBattery { get; set; }

        [JsonProperty("maxBarrel")]
        public uint maxBarrel { get; set; }

        [JsonProperty("maxBullet")]
        public uint maxBullet { get; set; }

        [JsonProperty("maxAccessories")]
        public uint maxAccessories { get; set; }

        [JsonProperty("maxGear")]
        public uint maxGear { get; set; }

        [JsonProperty("maxBulletPower")]
        public uint maxBulletPower { get; set; }

        [JsonProperty("maxBarrelSpeed")]
        public uint maxBarrelSpeed { get; set; }

        [JsonProperty("maxBatteryFvPoint")]
        public uint maxBatteryFvPoint { get; set; }

        [JsonProperty("maxGearPower")]
        public uint maxGearPower { get; set; }

        [JsonProperty("maxGearSpeed")]
        public uint maxGearSpeed { get; set; }

        [JsonProperty("maxGearFvPoint")]
        public uint maxGearFvPoint { get; set; }

        [JsonProperty("bulkPresentReceive")]
        public uint bulkPresentReceive { get; set; }

        [JsonProperty("maxPresentBox")]
        public uint maxPresentBox { get; set; }

        [JsonProperty("maxPresentHistory")]
        public uint maxPresentHistory { get; set; }
    }

    public class ConfigDataBase : DataBase<ConfigData>
    {
        public ConfigDataBase(string jsonName)
            : base(jsonName)
        {
            
        }

        public ConfigData Get()
        {
            return this.GetList()[0];
        }
    }
}