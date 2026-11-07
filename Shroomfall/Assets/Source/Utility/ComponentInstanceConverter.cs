using Contract.DTO.Abstraction;
using Contract.DTO.Runtime.EntityDomain.Component;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Assets.Source.Utility
{
    public class ComponentInstanceConverter : JsonConverter
    {
        #region Attributes
        #endregion

        #region Properties
        #endregion

        public ComponentInstanceConverter() { }

        #region Methods
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ComponentInstanceDTO);
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);

            var type = obj
                .Properties()
                .FirstOrDefault(p =>
                    string.Equals(
                        p.Name,
                        "ComponentType",
                        StringComparison.OrdinalIgnoreCase))
                ?.Value?
                .Value<string>();

            ComponentInstanceDTO result = type switch
            {
                "ActionInstanceDTO" => new ActionInstanceDTO(),
                "AIInstanceDTO" => new AIInstanceDTO(),
                "AppearanceInstanceDTO" => new AppearanceInstanceDTO(),
                "CollisionInstanceDTO" => new CollisionInstanceDTO(),
                "CharacteristicInstanceDTO" => new CharacteristicInstanceDTO(),
                "EconomicInstanceDTO" => new EconomicInstanceDTO(),
                "EffectContainerInstanceDTO" => new EffectContainerInstanceDTO(),
                "InventoryInstanceDTO" => new InventoryInstanceDTO(),
                "LifetimeInstanceDTO" => new LifetimeInstanceDTO(),
                "OwnershipInstanceDTO" => new OwnershipInstanceDTO(),
                "ProjectileInstanceDTO" => new ProjectileInstanceDTO(),
                "QuestProgressInstanceDTO" => new QuestProgressInstanceDTO(),
                "TransformInstanceDTO" => new TransformInstanceDTO(),
                "TriggeredEffectInstanceDTO" => new TriggeredEffectInstanceDTO(),
                "WorldItemPayloadInstanceDTO" => new WorldItemPayloadInstanceDTO(),

                _ => new ComponentInstanceDTO()
            };

            serializer.Populate(obj.CreateReader(), result);

            return result;
        }

        public override bool CanWrite => false;

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}