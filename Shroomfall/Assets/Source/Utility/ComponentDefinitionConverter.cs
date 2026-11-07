using Contract.DTO.Abstraction;
using Contract.DTO.Definition.EntityDomain.Component;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Assets.Source.Utility
{
    public class ComponentDefinitionConverter : JsonConverter
    {
        #region Attributes
        #endregion

        #region Properties
        #endregion

        public ComponentDefinitionConverter() { }

        #region Methods
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ComponentDefinitionDTO);
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);

            var type = obj.Properties()
                .FirstOrDefault(p =>
                    string.Equals(
                        p.Name,
                        "ComponentType",
                        StringComparison.OrdinalIgnoreCase))
                ?.Value?.Value<string>();

            ComponentDefinitionDTO result = type switch
            {
                "AIDefinitionDTO" => new AIDefinitionDTO(),
                "AppearanceDefinitionDTO" => new AppearanceDefinitionDTO(),
                "CollisionDefinitionDTO" => new CollisionDefinitionDTO(),
                "CharacteristicDefinitionDTO" => new CharacteristicDefinitionDTO(),
                "InventoryDefinitionDTO" => new InventoryDefinitionDTO(),
                "LifetimeDefinitionDTO" => new LifetimeDefinitionDTO(),
                "ProjectileDefinitionDTO" => new ProjectileDefinitionDTO(),
                "TriggeredEffectDefinitionDTO" => new TriggeredEffectDefinitionDTO(),

                _ => new ComponentDefinitionDTO()
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